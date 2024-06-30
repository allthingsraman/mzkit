﻿#Region "Microsoft.VisualBasic::d31f06319f4357aaa069a827e6c83e98, metadna\metaDNA\Infer\InferLink.vb"

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
'    Code Lines: 15 (38.46%)
' Comment Lines: 16 (41.03%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 8 (20.51%)
'     File Size: 1.20 KB


'     Class InferLink
' 
'         Properties: inferSize, kegg, level, parentTrace, rawFile
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.Xml
Imports BioNovoGene.BioDeep.MSEngine

Namespace Infer

    Public Class InferLink : Inherits AlignmentOutput

        <XmlElement> Public Property level As InferLevel

        ''' <summary>
        ''' 当前的Feature被推断为的目标KEGG代谢物编号
        ''' </summary>
        ''' <returns></returns>
        Public Property kegg As MzQuery

        ''' <summary>
        ''' 起始值为100
        ''' </summary>
        ''' <returns></returns>
        <XmlElement> Public Property parentTrace As Double
        ''' <summary>
        ''' 推断链的长度
        ''' </summary>
        ''' <returns></returns>
        <XmlElement> Public Property inferSize As Integer

        ''' <summary>
        ''' the source file of where <see cref="query"/> comes from.
        ''' </summary>
        ''' <returns></returns>
        <XmlElement> Public Property rawFile As String

        Public Overrides Function ToString() As String
            Return $"[{level.Description}] {kegg} {MyBase.ToString}"
        End Function

    End Class
End Namespace
