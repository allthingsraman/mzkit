﻿#Region "Microsoft.VisualBasic::4341df10dd88c31a525d577cf17253de, assembly\assembly\MarkupData\mzML\XML\softwareList.vb"

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

    '   Total Lines: 44
    '    Code Lines: 33 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (25.00%)
    '     File Size: 1.59 KB


    '     Class softwareList
    ' 
    '         Properties: softwares
    ' 
    '         Function: GenericEnumerator, ToArray, ToString
    ' 
    '     Class software
    ' 
    '         Properties: id, version
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML.ControlVocabulary
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace MarkupData.mzML

    Public Class softwareList : Inherits List
        Implements Enumeration(Of NamedValue(Of String))

        <XmlElement("software")>
        Public Property softwares As software()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToArray() As NamedValue(Of String)()
            Return Me.AsEnumerable.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return softwares.JoinBy("; ")
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedValue(Of String)) Implements Enumeration(Of NamedValue(Of String)).GenericEnumerator
            For Each si As software In softwares _
                .SafeQuery _
                .Where(Function(s) Not s.cvParams.IsNullOrEmpty)

                Yield New NamedValue(Of String)(si.cvParams.First.name, si.version, si.id)
            Next
        End Function
    End Class

    Public Class software : Inherits Params

        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property version As String

        Public Overrides Function ToString() As String
            Return $"{cvParams.First.name}({version})"
        End Function

    End Class
End Namespace
