﻿#Region "Microsoft.VisualBasic::c4e48306f930085e9ff024bfbdbafc52, mzkit\src\assembly\assembly\MarkupData\mzML\XML\Decoder\BinaryData.vb"

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

'   Total Lines: 19
'    Code Lines: 14
' Comment Lines: 0
'   Blank Lines: 5
'     File Size: 715 B


'     Class BinaryData
' 
'         Properties: binaryDataArrayList, dataProcessingRef, defaultArrayLength, id, index
'                     sourceFileRef
' 
' 
' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML.ControlVocabulary

Namespace MarkupData.mzML

    Public Class BinaryData : Inherits Params

        <XmlAttribute> Public Property index As Integer
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property defaultArrayLength As String
        <XmlAttribute> Public Property dataProcessingRef As String
        <XmlAttribute> Public Property sourceFileRef As String

        Public Property binaryDataArrayList As binaryDataArrayList

    End Class
End Namespace
