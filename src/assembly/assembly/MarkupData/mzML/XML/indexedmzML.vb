﻿#Region "Microsoft.VisualBasic::176bcd6a630965452a5ffaf383f93fc9, assembly\assembly\MarkupData\mzML\XML\indexedmzML.vb"

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

    '   Total Lines: 39
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.51 KB


    '     Class indexedmzML
    ' 
    '         Properties: fileChecksum, FilePath, indexList, indexListOffset, MimeType
    '                     mzML
    ' 
    '         Function: LoadFile, LoadScans
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text.Xml.Linq

Namespace MarkupData.mzML

    <XmlRoot("indexedmzML", [Namespace]:=indexedmzML.xmlns)>
    <XmlType("indexedmzML", [Namespace]:=indexedmzML.xmlns)>
    Public Class indexedmzML : Implements IFileReference

        Public Property mzML As mzML
        Public Property indexList As indexList
        Public Property indexListOffset As Long
        Public Property fileChecksum As String

        Private Property FilePath As String Implements IFileReference.FilePath

        Private ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
            Get
                Return {New ContentType With {.MIMEType = MIME.Xml, .FileExt = ".xml"}}
            End Get
        End Property

        Public Const xmlns As String = "http://psi.hupo.org/ms/mzml"

        Public Shared Iterator Function LoadScans(file As String) As IEnumerable(Of spectrum)
            For Each scan As spectrum In file.LoadXmlDataSet(Of spectrum)(, xmlns:=xmlns)
                Yield scan
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadFile(file As String) As indexedmzML
            Return file.LoadXml(Of indexedmzML)
        End Function
    End Class
End Namespace
