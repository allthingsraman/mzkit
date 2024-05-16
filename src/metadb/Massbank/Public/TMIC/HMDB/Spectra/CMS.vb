﻿#Region "Microsoft.VisualBasic::d10ebbe86b1bc6e39b03baed1561e7a4, metadb\Massbank\Public\TMIC\HMDB\Spectra\CMS.vb"

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

    '   Total Lines: 25
    '    Code Lines: 17
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 969 B


    '     Class CMS
    ' 
    '         Properties: derivative_exact_mass, derivative_formula, derivative_mw, derivative_smiles, derivative_type
    '                     peakList
    ' 
    '     Class c_ms_peak
    ' 
    '         Properties: c_ms_id
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace TMIC.HMDB.Spectra

    <XmlType("c-ms")> Public Class CMS : Inherits SpectraFile
        Implements IPeakList(Of c_ms_peak)

        <XmlElement("derivative-smiles")> Public Property derivative_smiles As NullableValue
        <XmlElement("derivative-exact-mass")> Public Property derivative_exact_mass As NullableValue
        <XmlElement("derivative-formula")> Public Property derivative_formula As NullableValue
        <XmlElement("derivative-mw")> Public Property derivative_mw As NullableValue
        <XmlElement("derivative-type")> Public Property derivative_type As NullableValue

        <XmlArray("c-ms-peaks")>
        Public Property peakList As c_ms_peak() Implements IPeakList(Of c_ms_peak).peakList

    End Class

    <XmlType("c-ms-peak")> Public Class c_ms_peak : Inherits MSPeak

        <XmlElement("c-ms-id")>
        Public Property c_ms_id As String

    End Class
End Namespace
