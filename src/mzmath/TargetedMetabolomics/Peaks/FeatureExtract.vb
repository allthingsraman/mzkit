﻿#Region "Microsoft.VisualBasic::d09adb27d949c6efb04b666ee76977d7, G:/mzkit/src/mzmath/TargetedMetabolomics//Peaks/FeatureExtract.vb"

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
    '    Code Lines: 23
    ' Comment Lines: 10
    '   Blank Lines: 6
    '     File Size: 1.36 KB


    ' Class FeatureExtract
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetTICPeaks
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports BioNovoGene.Analytical.MassSpectrometry.Math.LinearQuantitative.Linear
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math

Public MustInherit Class FeatureExtract(Of Sample)

    Friend ReadOnly peakwidth As DoubleRange

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(peakwidth As DoubleRange)
        Me.peakwidth = peakwidth
    End Sub

    Public MustOverride Function GetSamplePeaks(sample As Sample) As IEnumerable(Of TargetPeakPoint)

    ''' <summary>
    ''' Evaluate all feature ROI from the TIC data
    ''' </summary>
    ''' <param name="TIC"></param>
    ''' <param name="sn">
    ''' the s/n threshold
    ''' </param>
    ''' <param name="baselineQuantile"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Function GetTICPeaks(TIC As IEnumerable(Of ChromatogramTick), sn As Double, baselineQuantile As Double) As IEnumerable(Of ROI)
        Return TIC _
            .Shadows _
            .PopulateROI(
                peakwidth:=peakwidth,
                baselineQuantile:=baselineQuantile,
                snThreshold:=sn
            )
    End Function

End Class
