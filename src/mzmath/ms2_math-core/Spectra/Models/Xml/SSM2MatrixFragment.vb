﻿#Region "Microsoft.VisualBasic::6e5952e6613c7c7a0ffa25f2ee76ba7c, G:/mzkit/src/mzmath/ms2_math-core//Spectra/Models/Xml/SSM2MatrixFragment.vb"

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

    '   Total Lines: 61
    '    Code Lines: 40
    ' Comment Lines: 11
    '   Blank Lines: 10
    '     File Size: 1.86 KB


    '     Class SSM2MatrixFragment
    ' 
    '         Properties: da, mz, query, ref
    ' 
    '         Function: createFragment, FromXml, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml
Imports System.Xml.Serialization

Namespace Spectra.Xml

    ''' <summary>
    ''' tuple data of [mz, query_intensity, reference_intensity]
    ''' </summary>
    Public Class SSM2MatrixFragment

        ''' <summary>
        ''' The m/z value of the query fragment
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property mz As Double

#Region "Fragment intensity"
        <XmlAttribute> Public Property query As Double
        <XmlAttribute> Public Property ref As Double
#End Region

        ''' <summary>
        ''' Mass delta between the query and reference fragment in unit ``da``
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property da As String

        Public Shared Function FromXml(node As XmlNode, nodeName$) As SSM2MatrixFragment()
            Return (From child As XmlNode
                    In node.ChildNodes
                    Where child.Name = nodeName) _
 _
                .Select(AddressOf createFragment) _
                .ToArray
        End Function

        Private Shared Function createFragment(feature As XmlNode) As SSM2MatrixFragment
            Dim data = feature.Attributes
            Dim mz, query, ref As Double
            Dim da As String

            With data
                mz = !mz.Value
                query = !query.Value.ParseDouble
                ref = !ref.Value.ParseDouble
                da = !da.Value
            End With

            Return New SSM2MatrixFragment With {
                .mz = mz,
                .query = query,
                .ref = ref,
                .da = da
            }
        End Function

        Public Overrides Function ToString() As String
            Return mz
        End Function
    End Class
End Namespace
