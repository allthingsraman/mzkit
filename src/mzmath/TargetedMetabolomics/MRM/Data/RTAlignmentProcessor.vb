﻿#Region "Microsoft.VisualBasic::331dd96ec2f37040530ae50c5fa78d76, G:/mzkit/src/mzmath/TargetedMetabolomics//MRM/Data/RTAlignmentProcessor.vb"

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

    '   Total Lines: 88
    '    Code Lines: 73
    ' Comment Lines: 3
    '   Blank Lines: 12
    '     File Size: 3.78 KB


    '     Module RTAlignmentProcessor
    ' 
    '         Function: AcquireRT, getRt, processingRT
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MRM
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MRM.Data
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MRM.Models
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace MRM

    ''' <summary>
    ''' pre-processing for the rt shift
    ''' </summary>
    Public Module RTAlignmentProcessor

        <Extension>
        Public Function AcquireRT(samples As IEnumerable(Of String), ions As IonPair(), args As MRMArguments) As RTAlignment()
            Dim tolerance As Tolerance = args.tolerance
            Dim isomerism As IsomerismIonPairs() = IonPair.GetIsomerism(ions, tolerance).ToArray
            Dim raw = samples _
                .Select(Function(file)
                            Return MRMSamples.ExtractIonData(
                                ion_pairs:=isomerism,
                                mzML:=file,
                                assignName:=Function(i) i.accession,
                                tolerance:=tolerance
                            ).Select(Function(ion)
                                         Return (file.BaseName, ion)
                                     End Function)
                        End Function) _
                .IteratesALL _
                .GroupBy(Function(tuple) tuple.ion.name) _
                .ToArray
            Dim rt As RTAlignment() = raw _
                .Select(Function(ion)
                            Return ion.processingRT(args)
                        End Function) _
                .ToArray

            Return rt
        End Function

        <Extension>
        Private Function processingRT(samples As IEnumerable(Of (sampleName$, raw As IonChromatogram)), args As MRMArguments) As RTAlignment
            Dim data = samples.ToArray
            Dim ionPair As IsomerismIonPairs = data(Scan0).raw.ion
            Dim samplePeaks = data _
                .Select(Function(sample)
                            Dim ROI = sample.raw.chromatogram _
                                .Shadows _
                                .PopulateROI(
                                    baselineQuantile:=args.baselineQuantile,
                                    peakwidth:=args.peakwidth,
                                    angleThreshold:=args.angleThreshold,
                                    snThreshold:=args.sn_threshold
                                ).ToArray

                            Return (sample.sampleName, ROI)
                        End Function) _
                .Where(Function(sample) Not sample.ROI.IsNullOrEmpty) _
                .Select(Function(sample)
                            Dim rt As Double = sample.ROI.getRt(numOfIsomerism:=ionPair.ions.Length)

                            Return New NamedValue(Of Double) With {
                                .Name = sample.sampleName,
                                .Value = rt
                            }
                        End Function) _
                .ToArray

            Return New RTAlignment(ionPair, samplePeaks)
        End Function

        <Extension>
        Private Function getRt(ROI As IEnumerable(Of ROI), numOfIsomerism As Integer) As Double
            Dim peak As ROI = ROI _
                .OrderByDescending(Function(r) r.maxInto) _
                .First

            Return peak.rt
        End Function

    End Module

End Namespace
