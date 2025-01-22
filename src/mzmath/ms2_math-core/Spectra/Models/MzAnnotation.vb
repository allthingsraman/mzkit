﻿#Region "Microsoft.VisualBasic::930c72bb2ed5d64e2097e0356998347b, mzmath\ms2_math-core\Spectra\Models\MzAnnotation.vb"

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

    '   Total Lines: 37
    '    Code Lines: 19 (51.35%)
    ' Comment Lines: 9 (24.32%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (24.32%)
    '     File Size: 1.17 KB


    '     Class MzAnnotation
    ' 
    '         Properties: annotation, productMz
    ' 
    '         Function: ToString
    '         Operators: -
    ' 
    '     Interface IMzAnnotation
    ' 
    '         Properties: annotation, mz
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace Spectra

    ''' <summary>
    ''' annotation of the m/z value
    ''' </summary>
    ''' <remarks>
    ''' could be ctype cast to mz number value
    ''' </remarks>
    Public Class MzAnnotation : Implements IMzAnnotation

        ''' <summary>
        ''' the target ion m/z value
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property productMz As Double Implements IMzAnnotation.mz
        ''' <summary>
        ''' the ion annotation result, could be the metabolite id 
        ''' or metabolite name, something else.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property annotation As String Implements IMzAnnotation.annotation

        Sub New()
        End Sub

        Sub New(mz As Double)
            productMz = mz
        End Sub

        Sub New(annotation As String, mz As Double)
            Me.productMz = mz
            Me.annotation = annotation
        End Sub

        Public Overrides Function ToString() As String
            Return $"{productMz.ToString("F4")} [{annotation}]"
        End Function

        ''' <summary>
        ''' make evaluation of the mass delta
        ''' </summary>
        ''' <param name="anno"></param>
        ''' <param name="mass"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(anno As MzAnnotation, mass As Double) As Double
            Return anno.productMz - mass
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(mz As MzAnnotation) As Double
            Return mz.productMz
        End Operator

    End Class

    Public Interface IMzAnnotation

        Property mz As Double
        Property annotation As String

    End Interface
End Namespace
