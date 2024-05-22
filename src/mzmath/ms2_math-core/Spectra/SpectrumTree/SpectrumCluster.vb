﻿#Region "Microsoft.VisualBasic::d892d8f64f470b66d339013bc2d41c12, mzmath\ms2_math-core\Spectra\SpectrumTree\SpectrumCluster.vb"

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

    '   Total Lines: 78
    '    Code Lines: 50 (64.10%)
    ' Comment Lines: 15 (19.23%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (16.67%)
    '     File Size: 2.81 KB


    '     Class SpectrumCluster
    ' 
    '         Properties: cluster, Length, MaxIntensity, MID, Representative
    '                     RepresentativeFeature
    ' 
    '         Function: GetClusterVector, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports stdNum = System.Math

Namespace Spectra

    ''' <summary>
    ''' A collection of <see cref="PeakMs2"/> spectrum data
    ''' </summary>
    Public Class SpectrumCluster : Implements IEnumerable(Of PeakMs2)

        ''' <summary>
        ''' 当前的这个质谱图聚类之中的代表性质谱图
        ''' </summary>
        ''' <returns></returns>
        Public Property Representative As PeakMs2

        ''' <summary>
        ''' 在这个属性之中也会通过包含有<see cref="Representative"/>代表质谱图
        ''' </summary>
        ''' <returns></returns>
        Public Property cluster As PeakMs2()

        ''' <summary>
        ''' <see cref="cluster"/> members count
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return cluster.Length
            End Get
        End Property

        Public ReadOnly Property MID As String
            Get
                Return $"M{stdNum.Round(Representative.mz)}T{stdNum.Round(Representative.rt)}"
            End Get
        End Property

        Public ReadOnly Property RepresentativeFeature As String
            Get
                Return $"{Representative.file}#{Representative.scan}"
            End Get
        End Property

        Public ReadOnly Property MaxIntensity As Double
            Get
                Return cluster.Max(Function(peak) peak.Ms2Intensity)
            End Get
        End Property

        Public Function GetClusterVector(mz2Err As Tolerance) As Double()
            Return cluster _
                .Select(Function(peak)
                            Dim scores = GlobalAlignment.TwoDirectionSSM(Representative.mzInto, peak.mzInto, mz2Err)
                            Dim score As Double = stdNum.Min(scores.forward, scores.reverse)

                            Return score
                        End Function) _
                .ToArray
        End Function

        Public Overrides Function ToString() As String
            Return Representative.ToString & $"  with {cluster.Length} cluster members."
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of PeakMs2) Implements IEnumerable(Of PeakMs2).GetEnumerator
            For Each spectrum As PeakMs2 In cluster
                Yield spectrum
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
