﻿Imports System.IO
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1.PrecursorType
Imports BioNovoGene.Analytical.MassSpectrometry.SpectrumTree.Query
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class SpectrumReader : Implements IDisposable

    Dim disposedValue As Boolean
    Dim file As StreamPack
    Dim mzIndex As MzIonSearch

    ''' <summary>
    ''' mapping of <see cref="BlockNode.Id"/> to the mass index <see cref="MassIndex.name"/>
    ''' </summary>
    ReadOnly map As Dictionary(Of String, String)
    ReadOnly spectrum As New Dictionary(Of String, BlockNode)
    ReadOnly targetSet As Index(Of String)

    Default Public ReadOnly Property GetIdMap(libname As String) As String
        Get
            Return map(libname)
        End Get
    End Property

    ''' <summary>
    ''' open the database file in readonly mode
    ''' </summary>
    ''' <param name="target_uuid">
    ''' only a subset of the spectrum will be
    ''' queried if the target idset has been 
    ''' specificed
    ''' </param>
    ''' <param name="file"></param>
    Sub New(file As Stream, Optional target_uuid As String() = Nothing)
        Me.file = New StreamPack(file, [readonly]:=True)
        Me.map = Me.file.ReadText("/map.json").LoadJSON(Of Dictionary(Of String, String))
        Me.targetSet = target_uuid.Indexing
    End Sub

    ''' <summary>
    ''' populate all spectrum which the exact mass+adducts matched 
    ''' the m/z query input.
    ''' </summary>
    ''' <param name="mz"></param>
    ''' <returns></returns>
    Public Iterator Function QueryByMz(mz As Double) As IEnumerable(Of BlockNode)
        Dim ions = mzIndex.QueryByMz(mz).ToArray
        Dim index As IEnumerable(Of String) = From i As IonIndex
                                              In ions
                                              Let i32 As Integer = i.node
                                              Select i32
                                              Distinct
                                              Let tag As String = i32.ToString
                                              Select tag
        For Each key As String In index
            If Not spectrum.ContainsKey(key) Then
                Dim path As String = $"/spectrum/{key.Last}/{key}.dat"
                Dim file As Stream = Me.file.OpenBlock(path)
                Dim spectrumNode = NodeBuffer.Read(New BinaryDataReader(file))

                SyncLock spectrum
                    Call spectrum.Add(key, spectrumNode)
                End SyncLock
            End If

            Yield spectrum(key)
        Next
    End Function

    ''' <summary>
    ''' evaluate the theoretically m/z value based on the 
    ''' exact mass and the given adducts type
    ''' </summary>
    ''' <param name="mass"></param>
    ''' <param name="adducts"></param>
    ''' <returns></returns>
    Private Shared Function evalMz(mass As MassIndex, adducts As MzCalculator()) As IEnumerable(Of IonIndex)
        Return adducts _
            .Select(Function(type)
                        Dim mzi As Double = type.CalcMZ(mass.exactMass)

                        If mzi <= 0 Then
                            Return New IonIndex() {}
                        Else
                            Return mass.spectrum _
                                .Select(Function(i)
                                            Return New IonIndex With {
                                                .mz = mzi,
                                                .node = i
                                            }
                                        End Function)
                        End If
                    End Function) _
            .IteratesALL
    End Function

    Public Function BuildSearchIndex(ParamArray adducts As MzCalculator()) As SpectrumReader
        Dim exactMass As MassIndex() = LoadMass().ToArray
        Dim mz As IonIndex() = exactMass _
            .Select(Function(mass)
                        Return evalMz(mass, adducts)
                    End Function) _
            .IteratesALL _
            .ToArray

        mzIndex = New MzIonSearch(mz, da:=Tolerance.DeltaMass(0.5))

        Return Me
    End Function

    Private Iterator Function LoadMass() As IEnumerable(Of MassIndex)
        Dim files = DirectCast(file.GetObject("/massSet/"), StreamGroup) _
            .ListFiles _
            .Select(Function(f) DirectCast(f, StreamBlock)) _
            .ToArray
        Dim hasIdTargets As Boolean = targetSet.Count > 0

        For Each ref As StreamBlock In files
            If hasIdTargets Then
                ' only a subset of the spectrum will be
                ' queried if the target idset has been 
                ' specificed
                If Not ref.fileName.BaseName Like targetSet Then
                    Continue For
                End If
            End If

            Dim bcode As String = file.ReadText(ref)
            Dim mass As BDictionary = BencodeDecoder.Decode(bcode).First
            Dim index As New MassIndex

            index.name = mass!name.ToString
            index.exactMass = Val(mass!exactMass.ToString)
            index.spectrum = DirectCast(mass!spectrum, BList) _
                .Select(Function(b) Integer.Parse(b.ToString)) _
                .AsList

            Yield index
        Next
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)
                Call file.Dispose()
            End If

            ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
            ' TODO: 将大型字段设置为 null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
    ' Protected Overrides Sub Finalize()
    '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class