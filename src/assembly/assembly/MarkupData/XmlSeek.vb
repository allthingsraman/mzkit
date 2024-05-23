﻿#Region "Microsoft.VisualBasic::75e718791d342bca63bd94313bf07cd5, assembly\assembly\MarkupData\XmlSeek.vb"

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

    '   Total Lines: 176
    '    Code Lines: 134 (76.14%)
    ' Comment Lines: 13 (7.39%)
    '    - Xml Docs: 23.08%
    ' 
    '   Blank Lines: 29 (16.48%)
    '     File Size: 7.28 KB


    '     Class XmlSeek
    ' 
    '         Properties: IndexKeys
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: LoadIndex, ParseFileType, parseIndex, parseIndexOfmzML, parseIndexOfmzXML
    '                   ReadScan, TryGetOffsets
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.DataReader
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports indexList = BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML.indexList
Imports indexOffsets = BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzXML.index

Namespace MarkupData

    ''' <summary>
    ''' binary data seeking of the xml data file.
    ''' </summary>
    Public Class XmlSeek : Implements IDisposable

        ReadOnly bin As BinaryDataReader
        ReadOnly reader As IDataReader
        ReadOnly loader As IScanReader
        ReadOnly indexOffset As Long
        ReadOnly sha1 As String

        Friend ReadOnly parser As XmlParser
        Friend ReadOnly fileName As String
        Friend ReadOnly type As XmlFileTypes = XmlFileTypes.mzXML

        Dim index As Dictionary(Of String, Long)
        Dim indexgroup As Dictionary(Of String, NamedValue(Of Long)())
        Dim disposedValue As Boolean

        Public ReadOnly Property IndexKeys As String()
            Get
                Return index.Keys.ToArray
            End Get
        End Property

        Sub New(file As String)
            bin = file.OpenBinaryReader(Encodings.UTF8)
            type = ParseFileType(file)
            parser = New XmlParser(bin.BaseStream, type)
            fileName = file.GetFullPath

            With parseIndex(bin.BaseStream, type, bin.Encoding)
                sha1 = .sha1
                indexOffset = .indexOffset
            End With

            Select Case type
                Case XmlFileTypes.imzML
                    reader = New imzMLScan
                Case XmlFileTypes.mzML
                    reader = New mzMLScan
                    loader = New mzMLScans
                Case XmlFileTypes.mzXML
                    reader = New mzXMLScan
                    loader = New mzXMLScans
                Case Else
                    Throw New NotImplementedException(type.Description)
            End Select

            bin.Seek(indexOffset, SeekOrigin.Begin)
        End Sub

        Public Function TryGetOffsets(group As String) As NamedValue(Of Long)()
            Return indexgroup.TryGetValue(group)
        End Function

        Public Function ReadScan(key As String) As MSScan
            Select Case type
                Case XmlFileTypes.mzXML
                    Return parser.ParseDataNode(Of mzXML.scan)(index(key)).DoCall(AddressOf loader.CreateScan)
                Case XmlFileTypes.mzML
                    Return parser.ParseDataNode(Of spectrum)(index(key)).DoCall(AddressOf loader.CreateScan)
                Case Else
                    Throw New NotImplementedException(type.Description)
            End Select
        End Function

        Public Function LoadIndex() As XmlSeek
            Dim index As NamedValue(Of Long)()

            If type = XmlFileTypes.mzML Then
                index = indexList.ParseIndexList(bin.BaseStream, indexOffset).GetOffsets.ToArray
            ElseIf type = XmlFileTypes.mzXML Then
                index = indexOffsets _
                    .ParseIndexList(bin, indexOffset) _
                    .Select(Function(idx) idx.GetOffsets) _
                    .IteratesALL _
                    .ToArray
            Else
                Throw New NotImplementedException(type.Description)
            End If

            Me.index = index.ToDictionary.FlatTable
            Me.indexgroup = index _
                .GroupBy(Function(idx) idx.Description) _
                .ToDictionary(Function(g) g.Key,
                              Function(g)
                                  Return g.ToArray
                              End Function)

            Return Me
        End Function

        Friend Shared Function parseIndex(bin As Stream, type As XmlFileTypes, encoding As Encoding) As (indexOffset As Long, sha1 As String)
            Dim tails As String = bin.Tails(128, encoding:=encoding)

            Select Case type
                Case XmlFileTypes.mzXML
                    Return parseIndexOfmzXML(tails)

                Case XmlFileTypes.mzML, XmlFileTypes.imzML
                    Return parseIndexOfmzML(tails)

                Case Else
                    Throw New NotImplementedException(type.Description)
            End Select
        End Function

        Private Shared Function parseIndexOfmzXML(tails As String) As (indexOffset As Long, sha1 As String)
            Dim offset As Long = tails.Match("[<]indexOffset.+\d+[<]/indexOffset>").GetValue.ParseLong
            Dim sha1 As String = tails.Match("[<]sha1.+[<]/sha1>").GetValue

            Return (offset, sha1)
        End Function

        Private Shared Function parseIndexOfmzML(tails As String) As (indexOffset As Long, sha1 As String)
            Dim offset As Long = tails.Match("[<]indexListOffset.+\d+[<]/indexListOffset>").GetValue.ParseLong
            Dim sha1 As String = tails.Match("[<]fileChecksum.+[<]/fileChecksum>").GetValue

            Return (offset, sha1)
        End Function

        Public Shared Function ParseFileType(file As String) As XmlFileTypes
            Select Case file.ExtensionSuffix.ToLower
                Case "mzxml" : Return XmlFileTypes.mzXML
                Case "mzml" : Return XmlFileTypes.mzML
                Case "imzml" : Return XmlFileTypes.imzML
                Case "mzdata" : Return XmlFileTypes.mzData
                Case Else
                    Throw New NotSupportedException($"unknown file data type: {file.ExtensionSuffix}!")
            End Select
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call bin.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
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
End Namespace
