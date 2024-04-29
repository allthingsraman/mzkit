﻿#Region "Microsoft.VisualBasic::7035a7d82405a86e11e336a8f24d9799, E:/mzkit/src/assembly/assembly//MarkupData/mzML/XML/Decoder/binaryDataArray.vb"

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

    '   Total Lines: 43
    '    Code Lines: 36
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.72 KB


    '     Class binaryDataArray
    ' 
    '         Properties: binary, cvParams, encodedLength
    ' 
    '         Function: GetCompressionType, GetPrecision, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML.ControlVocabulary
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace MarkupData.mzML

    Public Class binaryDataArray : Implements IBase64Container

        Public Property encodedLength As Integer

        <XmlElement(NameOf(cvParam))>
        Public Property cvParams As cvParam()
        Public Property binary As String Implements IBase64Container.BinaryArray

        Public Overrides Function ToString() As String
            Return binary
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPrecision() As Integer Implements IBase64Container.GetPrecision
            If Not cvParams.KeyItem("64-bit float") Is Nothing Then
                Return 64
            ElseIf Not cvParams.KeyItem("32-bit float") Is Nothing Then
                Return 32
            Else
                Throw New NotImplementedException
            End If
        End Function

        Public Function GetCompressionType() As CompressionMode Implements IBase64Container.GetCompressionType
            If Not cvParams.KeyItem("zlib compression") Is Nothing Then
                Return CompressionMode.zlib
            ElseIf Not cvParams.KeyItem("no compression") Is Nothing Then
                Return CompressionMode.none
            ElseIf Not cvParams.KeyItem("gzip compression") Is Nothing Then
                Return CompressionMode.gzip
            Else
                Throw New NotImplementedException
            End If
        End Function
    End Class
End Namespace
