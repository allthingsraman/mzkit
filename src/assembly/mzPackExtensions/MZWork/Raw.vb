﻿#Region "Microsoft.VisualBasic::7f4791419fc86091299cf295a716fc31, assembly\mzPackExtensions\MZWork\Raw.vb"

    ' Author:
    ' 
    '       xieguigang (gg.xie@bionovogene.com, BioNovoGene Co., LTD.)
    ' 
    ' Copyright (c) 2018 gg.xie@bionovogene.com, BioNovoGene Co., LTD.
    ' 
    ' 
    ' MIT License
    ' 
    ' 
    ' Permission is hereby granted, free of charge, to any person obtaining a copy
    ' of this software and associated documentation files (the "Software"), to deal
    ' in the Software without restriction, including without limitation the rights
    ' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    ' copies of the Software, and to permit persons to whom the Software is
    ' furnished to do so, subject to the following conditions:
    ' 
    ' The above copyright notice and this permission notice shall be included in all
    ' copies or substantial portions of the Software.
    ' 
    ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    ' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    ' SOFTWARE.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 235
    '    Code Lines: 164 (69.79%)
    ' Comment Lines: 31 (13.19%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 40 (17.02%)
    '     File Size: 8.07 KB


    '     Class Raw
    ' 
    '         Properties: cache, cacheFileExists, isInMemory, isLoaded, numOfScan1
    '                     numOfScan2, rtmax, rtmin, source
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: FindMs1Scan, FindMs2Scan, GetCacheFileSize, GetLoadedMzpack, GetMs1Scans
    '                   GetMs2Scans, GetSnapshot, GetUVscans, LoadMzpack, UnloadMzpack
    ' 
    '         Sub: loadMemory, SaveAs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization
Imports Microsoft.VisualBasic.Linq

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

Namespace MZWork

    Public Class Raw

#Region "Cache Store"

        ''' <summary>
        ''' 原始数据文件位置
        ''' </summary>
        ''' <returns></returns>
        <MessagePackMember(0)> Public Property source As String

        ''' <summary>
        ''' mzpack二进制缓存文件位置
        ''' </summary>
        ''' <returns></returns>
        <MessagePackMember(5)> Public Property cache As String

        <MessagePackMember(1)> Public Property rtmin As Double
        <MessagePackMember(2)> Public Property rtmax As Double

        ''' <summary>
        ''' MS1扫描的数量
        ''' </summary>
        ''' <returns></returns>
        <MessagePackMember(3)> Public Property numOfScan1 As Integer
        ''' <summary>
        ''' MS2扫描的数量
        ''' </summary>
        ''' <returns></returns>
        <MessagePackMember(4)> Public Property numOfScan2 As Integer

#End Region

        Dim loaded As mzPack
        Dim ms1 As Dictionary(Of String, ScanMS1)
        Dim ms2 As Dictionary(Of String, ScanMS2)

        ''' <summary>
        ''' is not loaded mzpack object is nothing 
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isLoaded As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Not loaded Is Nothing
            End Get
        End Property

        Public ReadOnly Property isInMemory As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return loaded IsNot Nothing AndAlso Not cache.FileExists
            End Get
        End Property

        ''' <summary>
        ''' the cache of the mzpack data file is exists in the temp storage or not?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property cacheFileExists As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return cache.FileExists
            End Get
        End Property

        Public Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(inMemory As mzPack)
            Call loadMemory(inMemory)
        End Sub

        Sub New(copy As Raw)
            Me.cache = copy.cache
            Me.source = copy.source
            Me.rtmin = copy.rtmin
            Me.rtmax = copy.rtmax
            Me.numOfScan1 = copy.numOfScan1
            Me.numOfScan2 = copy.numOfScan2
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetLoadedMzpack() As mzPack
            Return loaded
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSnapshot() As Image
            Return loaded.Thumbnail
        End Function

        Private Sub loadMemory(inMemory As mzPack)
            loaded = inMemory
            ms1 = loaded.MS.ToDictionary(Function(scan) scan.scan_id)
            ms2 = loaded.MS _
                .Select(Function(m1) m1.products) _
                .IteratesALL _
                .ToDictionary(Function(m2)
                                  Return m2.scan_id
                              End Function)

            If inMemory.source.StringEmpty(, True) Then
                inMemory.source = source.BaseName
            End If

            If ms1.Count > 0 Then
                Me.rtmin = Aggregate m1 As ScanMS1 In ms1.Values Into Min(m1.rt)
                Me.rtmax = Aggregate m1 As ScanMS1 In ms1.Values Into Max(m1.rt)
            Else
                Me.rtmin = 0
                Me.rtmax = 1400
            End If

            Me.numOfScan1 = ms1.Count
            Me.numOfScan2 = ms2.Count
        End Sub

        ''' <summary>
        ''' the mzpack will has no MS scan data if the source file is missing
        ''' </summary>
        ''' <param name="reload"></param>
        ''' <param name="verbose"></param>
        ''' <param name="strict"></param>
        ''' <returns>
        ''' this function returns itself
        ''' </returns>
        Public Function LoadMzpack(reload As Action(Of String, String),
                                   Optional verbose As Boolean = True,
                                   Optional strict As Boolean = True) As Raw
            If isLoaded Then
                Return Me
            End If

            If strict Then
                Using file As Stream = cache.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                    Call loadMemory(mzPack.ReadAll(file, verbose:=verbose, checkVer1DuplicatedId:=True))
                End Using
            Else
mzPackReader:
                Try
                    Using file As Stream = cache.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                        Call loadMemory(mzPack.ReadAll(file, verbose:=verbose))
                    End Using
                Catch ex As Exception
                    Call ($"It seems that mzPack cache file of {source} is damaged,{vbCrLf} mzkit will going to reload of the source file.").PrintException

                    SyncLock reload
                        Call reload(source, cache)
                    End SyncLock

                    GoTo mzPackReader
                End Try
            End If

            Return Me
        End Function

        Public Function UnloadMzpack() As Raw
            If Not loaded Is Nothing Then
                Erase loaded.MS

                If Not loaded.Thumbnail Is Nothing Then
                    loaded.Thumbnail.Dispose()
                    loaded.Thumbnail = Nothing
                End If

                If Not loaded.Scanners Is Nothing Then
                    loaded.Scanners.Clear()
                End If

                loaded = Nothing
            End If

            Call ms2.Clear()
            Call ms1.Clear()

            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetUVscans() As IEnumerable(Of UVScan)
            Return loaded.GetUVScans
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindMs2Scan(id As String) As ScanMS2
            Return ms2.TryGetValue(id)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindMs1Scan(id As String) As ScanMS1
            Return ms1.TryGetValue(id)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCacheFileSize() As Long
            Return cache.FileLength
        End Function

        Public Function GetMs1Scans() As IEnumerable(Of ScanMS1)
            If loaded Is Nothing Then
                Using file As Stream = cache.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                    Return mzPack.ReadAll(file).MS
                End Using
            Else
                Return loaded.MS
            End If
        End Function

        Public Function GetMs2Scans(Optional ignores_empty As Boolean = True) As IEnumerable(Of ScanMS2)
            Call LoadMzpack(Sub(m, s) Console.WriteLine($"[{m}] -> [{s}]"))

            If ignores_empty Then
                Return From ms As ScanMS2
                       In ms2.Values
                       Where ms.size > 0
                       Select ms
            Else
                Return ms2.Values
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SaveAs(file As String)
            Call cache.FileCopy(file)
        End Sub

        ''' <summary>
        ''' Create a raw data file reference to a local mzpack file
        ''' </summary>
        ''' <param name="filepath">
        ''' the file path to the mzpack file
        ''' </param>
        ''' <returns></returns>
        Public Shared Function UseMzPack(filepath As String) As Raw
            If Not filepath.ExtensionSuffix("mzpack") Then
                Throw New InvalidDataException("the given file path must be a mzpack data file!")
            End If

            Return New Raw With {
                .cache = filepath.GetFullPath,
                .numOfScan1 = 0,
                .numOfScan2 = 0,
                .rtmax = 0,
                .rtmin = 0,
                .source = .cache
            }
        End Function

    End Class
End Namespace
