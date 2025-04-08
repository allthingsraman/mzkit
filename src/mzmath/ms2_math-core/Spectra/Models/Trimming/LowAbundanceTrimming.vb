﻿#Region "Microsoft.VisualBasic::f67e31cb261cb321d5c6c8cb26ce39a0, mzmath\ms2_math-core\Spectra\Models\Trimming\LowAbundanceTrimming.vb"

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

    '   Total Lines: 102
    '    Code Lines: 57 (55.88%)
    ' Comment Lines: 29 (28.43%)
    '    - Xml Docs: 96.55%
    ' 
    '   Blank Lines: 16 (15.69%)
    '     File Size: 3.81 KB


    '     Class LowAbundanceTrimming
    ' 
    '         Properties: [Default], intoCutff, quantCutoff, threshold, Zero
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ParseScript, (+2 Overloads) Trim
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Namespace Spectra

    ''' <summary>
    ''' algorithm wrapper of the noise fragment filter by theirs intensity cutoff 
    ''' </summary>
    ''' <remarks>
    ''' the peak fragment <see cref="ms2.Annotation"/> metadata maybe lost due 
    ''' to the reason of intensity filter will removes some fragment object 
    ''' that with lower intensity value.
    ''' </remarks>
    Public MustInherit Class LowAbundanceTrimming

        Protected ReadOnly m_threshold As Double

        Public ReadOnly Property threshold As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_threshold
            End Get
        End Property

        Sub New(cutoff As Double)
            If cutoff > 1 Then
                m_threshold = cutoff / 100
            Else
                m_threshold = cutoff
            End If

            If cutoff <= 0 Then
                Call $"the threshold value for trimming low abundance fragment is ZERO or negative value, no item will be trimmed!".Warning
            End If
        End Sub

        ''' <summary>
        ''' default ``5%`` percentage cutoff
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property intoCutff As New RelativeIntensityCutoff(0.05)
        Public Shared ReadOnly Property quantCutoff As New QuantileIntensityCutoff(0.65)

        ''' <summary>
        ''' intocutoff = 0.05
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property [Default] As New [Default](Of LowAbundanceTrimming)(intoCutff)

        Public Shared ReadOnly Property Zero As New RelativeIntensityCutoff(0)

        ''' <summary>
        ''' Removes the fragment peaks which its intensity value which is lower than the specific threshold
        ''' </summary>
        ''' <param name="spectrum"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Trim(spectrum As IEnumerable(Of ms2)) As ms2()
            If m_threshold <= 0 Then
                Return spectrum.ToArray
            Else
                Return lowAbundanceTrimming(spectrum.ToArray)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Trim(mz As Double(), into As Double()) As (mz As Double(), into As Double())
            If m_threshold > 0 AndAlso mz.Length > 0 Then
                Call lowAbundanceTrimming(mz, into)
            End If

            Return (mz, into)
        End Function

        Protected MustOverride Sub lowAbundanceTrimming(ByRef mz As Double(), ByRef into As Double())
        Protected MustOverride Function lowAbundanceTrimming(spectrum As ms2()) As ms2()
        Public MustOverride Overrides Function ToString() As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="str">
        ''' 1. 0.xxx percentage
        ''' 2. q:0.xxx quantile
        ''' </param>
        ''' <returns></returns>
        Public Shared Function ParseScript(str As String) As LowAbundanceTrimming
            If str.StartsWith("q:") Then
                Dim threshold = str.Replace("q:", "").Trim.DoCall(AddressOf ParseDouble)
                Dim quantile As New QuantileIntensityCutoff(threshold)

                Return quantile
            Else
                Dim threshold = Val(Strings.Trim(str))
                Dim intocutoff As New RelativeIntensityCutoff(threshold)

                Return intocutoff
            End If
        End Function
    End Class
End Namespace
