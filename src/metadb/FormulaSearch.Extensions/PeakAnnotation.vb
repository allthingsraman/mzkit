﻿#Region "Microsoft.VisualBasic::a2c13ab27d722f5fa9e665cd98c167a2, metadb\FormulaSearch.Extensions\PeakAnnotation.vb"

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

    '   Total Lines: 165
    '    Code Lines: 111 (67.27%)
    ' Comment Lines: 40 (24.24%)
    '    - Xml Docs: 92.50%
    ' 
    '   Blank Lines: 14 (8.48%)
    '     File Size: 6.60 KB


    ' Class PeakAnnotation
    ' 
    '     Properties: formula, norm_score, precursor_matched, products, score
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: DoPeakAnnotation, GetAnnotatedPeaks, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1.PrecursorType
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.SplashID
Imports BioNovoGene.BioDeep.Chemoinformatics.Formula
Imports BioNovoGene.BioDeep.Chemoinformatics.Formula.MS
Imports BioNovoGene.BioDeep.MSFinder
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

''' <summary>
''' Do formula search and peak annotation result
''' </summary>
Public Class PeakAnnotation

    ''' <summary>
    ''' the product peak annotation result dataset, this array contains all spectrum peaks, includes
    ''' peak has been annotated or peaks has no annotation data.
    ''' </summary>
    ''' <returns></returns>
    Public Property products As ms2()
    ''' <summary>
    ''' the target metabolite formula source data, this could be parsed from
    ''' the database of the know metabolite or the de-novo formula prediction
    ''' result based on the algorithm 
    ''' </summary>
    ''' <returns></returns>
    Public Property formula As FormulaComposition
    Public Property precursor_matched As Boolean = False
    Public Property score As Double
    Public Property norm_score As Double

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(formula As FormulaComposition, products As IEnumerable(Of ProductIon))
        Call Me.New(
            formula:=formula,
            products:=products _
                .Select(Function(i)
                            Return New ms2 With {
                                .mz = i.Mass,
                                .intensity = i.Intensity,
                                .Annotation = i.Name
                            }
                        End Function)
        )
    End Sub

    Sub New(formula As FormulaComposition, products As IEnumerable(Of ms2))
        Me.formula = formula
        Me.products = products _
            .Select(Function(a)
                        Return New ms2 With {
                            .mz = a.mz,
                            .intensity = a.intensity,
                            .Annotation = If(a.Annotation.IsInteger, "", a.Annotation)
                        }
                    End Function) _
            .ToArray
    End Sub

    ''' <summary>
    ''' Filter and get a peak list that with <see cref="ms2.Annotation"/> not empty.
    ''' </summary>
    ''' <returns>
    ''' a collection of the peaks data with annotation data
    ''' </returns>
    Public Function GetAnnotatedPeaks() As ms2()
        Return products _
            .Where(Function(i) Not i.Annotation.StringEmpty(, True)) _
            .ToArray
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return formula.ToString
    End Function

    ''' <summary>
    ''' do score evaluation of the spectrum associated with the assembled formula composition 
    ''' </summary>
    ''' <param name="peaks"></param>
    ''' <param name="adduct"></param>
    ''' <param name="formula"></param>
    ''' <param name="da">
    ''' the mass tolerance error for matches the ms2 spectrum peaks, 
    ''' usually be the tolerance error for make spectrum centroid 
    ''' process
    ''' </param>
    ''' <returns></returns>
    Public Shared Function DoPeakAnnotation(peaks As ISpectrum, adduct As MzCalculator, formula As Formula,
                                            Optional da As Double = 0.1) As PeakAnnotation

        Dim assign As New FragmentAssigner(da)
        Dim exactMass As Double = formula.ExactMass
        Dim precursorMz As Double = adduct.CalcMZ(exactMass)
        Dim precursorHit As Boolean = False
        Dim peaksData As SpectrumPeak() = peaks.GetIons _
            .Select(Function(m)
                        If std.Abs(m.mz - exactMass) <= da Then
                            m.Annotation = "M"
                            precursorHit = True
                        ElseIf std.Abs(m.mz - precursorMz) <= da Then
                            m.Annotation = adduct.ToString
                            precursorHit = True
                        End If

                        Return New SpectrumPeak(m)
                    End Function) _
            .ToArray
        Dim adductInfo As New AdductIon(adduct)
        Dim result = assign.FastFragmnetAssigner(peaksData.AsList, formula, adductInfo)
        Dim fcom As New FormulaComposition(formula.CountsByElement, formula.ToString) With {
            .charge = adduct.charge,
            .massdiff = 0,
            .ppm = 0
        }
        Dim score As Double = 0
        ' 2024-08-29
        ' make union of the annotated products and
        ' un-annotated fragments
        Dim union As ms2() = result _
            .Select(Function(i)
                        Dim text As String

                        If i.Comment = "FragmentFormula" OrElse i.Comment = "Precursor" Then
                            text = i.Name
                            score += 1
                        ElseIf i.Type = AnnotationType.Loss Then
                            text = i.ShortName
                            score += 3
                        Else
                            text = $"{i.ShortName} [{i.Formula.EmpiricalFormula}]"
                            score += 3
                        End If

                        Return New ms2 With {
                            .mz = i.Mass,
                            .intensity = i.Intensity,
                            .Annotation = text
                        }
                    End Function) _
            .JoinIterates(peaks.GetIons) _
            .ToArray _
            .Centroid(Tolerance.DeltaMass(0.1), New RelativeIntensityCutoff(0.01)) _
            .ToArray

        Return New PeakAnnotation(fcom, union) With {
            .precursor_matched = precursorHit,
            .score = score,
            .norm_score = .score / (3 * union.Length)
        }
    End Function

    ''' <summary>
    ''' get the spectrum peak annotation result outputs
    ''' </summary>
    ''' <param name="pa"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(pa As PeakAnnotation) As ms2()
        Return pa.products
    End Operator

End Class
