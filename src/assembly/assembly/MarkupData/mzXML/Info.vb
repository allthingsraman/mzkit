﻿#Region "Microsoft.VisualBasic::b10713626e497fbd99afbc2ccbf6f2f5, assembly\assembly\MarkupData\mzXML\Info.vb"

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

    '   Total Lines: 72
    '    Code Lines: 43 (59.72%)
    ' Comment Lines: 11 (15.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 18 (25.00%)
    '     File Size: 2.06 KB


    '     Class dataProcessing
    ' 
    '         Properties: comment, processingOperation, softwares
    ' 
    '         Function: ToString
    ' 
    '     Class msInstrument
    ' 
    '         Properties: msInstrumentID, msManufacturer, msModel, software
    ' 
    '         Function: ToString
    ' 
    '     Structure software
    ' 
    '         Properties: name, type, version
    ' 
    '         Function: ToString
    ' 
    '     Structure CategoryValue
    ' 
    '         Properties: category, value
    ' 
    '     Structure parentFile
    ' 
    '         Properties: fileName, fileShal, fileType
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace MarkupData.mzXML

    Public Class dataProcessing

        <XmlElement("software")>
        Public Property softwares As software()
        Public Property processingOperation As software
        Public Property comment As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

    Public Class msInstrument

        <XmlAttribute>
        Public Property msInstrumentID As String
        Public Property msManufacturer As CategoryValue
        Public Property msModel As CategoryValue
        Public Property software As software

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

    Public Structure software

        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property version As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    Public Structure CategoryValue
        <XmlAttribute> Public Property category As String
        <XmlAttribute> Public Property value As String
    End Structure

    ''' <summary>
    ''' the orginal rawdata file of current mzXML file where it converts from
    ''' </summary>
    Public Structure parentFile

        ''' <summary>
        ''' the file path
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property fileName As String
        ''' <summary>
        ''' the file data operation name
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property fileType As String
        <XmlAttribute> Public Property fileShal As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Structure

End Namespace
