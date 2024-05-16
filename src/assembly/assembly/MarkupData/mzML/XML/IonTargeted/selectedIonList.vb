﻿#Region "Microsoft.VisualBasic::35f8d49f1b3e0038c67fd77b479895c3, assembly\assembly\MarkupData\mzML\XML\IonTargeted\selectedIonList.vb"

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

    '   Total Lines: 29
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.06 KB


    '     Class selectedIonList
    ' 
    '         Properties: selectedIon
    ' 
    '         Function: GetIonIntensity, GetIonMz
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML.ControlVocabulary

Namespace MarkupData.mzML.IonTargeted

    Public Class selectedIonList : Inherits List

        <XmlElement>
        Public Property selectedIon As Params()

        Public Function GetIonMz() As Double()
            Return selectedIon _
                .Select(Function(ion)
                            Return ion.cvParams.FirstOrDefault(Function(a) a.name = "selected ion m/z")?.value
                        End Function) _
                .Select(AddressOf Val) _
                .ToArray
        End Function

        Public Function GetIonIntensity() As Double()
            Return selectedIon _
                .Select(Function(ion)
                            Return ion.cvParams.FirstOrDefault(Function(a) a.name = "peak intensity")?.value
                        End Function) _
                .Select(AddressOf Val) _
                .ToArray
        End Function
    End Class
End Namespace
