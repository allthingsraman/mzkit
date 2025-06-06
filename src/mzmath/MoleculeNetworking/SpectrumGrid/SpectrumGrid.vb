﻿#Region "Microsoft.VisualBasic::fdb96d8625ca16aea049db26f67f4c00, mzmath\MoleculeNetworking\SpectrumGrid\SpectrumGrid.vb"

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

    '   Total Lines: 295
    '    Code Lines: 217 (73.56%)
    ' Comment Lines: 34 (11.53%)
    '    - Xml Docs: 76.47%
    ' 
    '   Blank Lines: 44 (14.92%)
    '     File Size: 12.17 KB


    ' Class SpectrumGrid
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AssignPeaks, Clustering, GetTotal, GetUnmapped, GridLineDecompose
    '               GridLineNoDecompose, SetRawDataFiles, SumNorm
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Algorithm
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

''' <summary>
''' Make sample spectrum aligns to the peaktable based on the pearson correlation method
''' </summary>
Public Class SpectrumGrid

    ''' <summary>
    ''' spectrum cluster which is indexed via the rt window
    ''' </summary>
    Dim clusters As BlockSearchFunction(Of SpectrumLine)
    Dim filenames As String()
    Dim unmapped As New List(Of SpectrumLine)

    ReadOnly rt_win As Double = 7.5

    ''' <summary>
    ''' less than 2 means no decompose of the spectrum;
    ''' this parameter value usually be 2 as we usually needs for processing of the DL, SR strucutres.
    ''' </summary>
    ReadOnly dia_n As Integer = 1
    ReadOnly dotcutoff As Double = 0.85

    Sub New(Optional rt_win As Double = 7.5,
            Optional dia_n As Integer = 1,
            Optional dotcutoff As Double = 0.85)

        Me.dotcutoff = dotcutoff
        Me.dia_n = dia_n
        Me.rt_win = rt_win
    End Sub

    ''' <summary>
    ''' get all cluster data result
    ''' </summary>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetTotal() As IEnumerable(Of SpectrumLine)
        Return clusters.raw.AsEnumerable
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetUnmapped() As IEnumerable(Of SpectrumLine)
        Return unmapped.AsEnumerable
    End Function

    Public Function SetRawDataFiles(files As IEnumerable(Of NamedCollection(Of PeakMs2))) As SpectrumGrid
        If dia_n < 2 Then
            Call VBDebugger.EchoLine($"spectrum data will not be processed via DIA decompose.")
        Else
            Call VBDebugger.EchoLine($"each cluster of the spectrum data will be decomposed into {dia_n} sub-clusters.")
        End If

        clusters = New BlockSearchFunction(Of SpectrumLine)(Clustering(files), Function(a) a.rt, 5, fuzzy:=True)
        Return Me
    End Function

    ''' <summary>
    ''' make spectrum clustering
    ''' </summary>
    ''' <param name="rawdata">the spectrum rawdata from multiple rawdata files</param>
    ''' <returns>
    ''' a collection of the ion groups
    ''' </returns>
    Private Iterator Function Clustering(rawdata As IEnumerable(Of NamedCollection(Of PeakMs2))) As IEnumerable(Of SpectrumLine)
        Dim ions As New List(Of PeakMs2)
        Dim files As New List(Of String)
        Dim qc_filter As New Regex(".+QC.+\d+", RegexOptions.Compiled Or RegexOptions.Singleline)

        For Each file As NamedCollection(Of PeakMs2) In rawdata
            Call files.Add(file.name)
            Call ions.AddRange(file)
        Next

        ' group the spectrum ions via the precursor ion m/z
        Dim parent_groups As NamedCollection(Of PeakMs2)() = ions _
            .GroupBy(Function(i) i.mz, offsets:=1.5) _
            .ToArray

        ' removes QC files for the cor test
        filenames = files _
            .Where(Function(name)
                       Return Not name.IsPattern(qc_filter)
                   End Function) _
            .ToArray

        Call VBDebugger.EchoLine("make spectrum alignment for each precursor ion...")

        For Each ion_group As NamedCollection(Of PeakMs2) In TqdmWrapper.Wrap(parent_groups, wrap_console:=App.EnableTqdm)
            Dim tree As New BinaryClustering(equals:=dotcutoff)

            ' ion groups has the same precursor ion m/z
            ' due to the reason of precursor mz has already been
            ' grouped before
            tree = tree.Tree(ion_group)

            For Each cluster As NamedCollection(Of PeakMs2) In tree.GetClusters
                ' split by rt
                Dim rt_groups As NamedCollection(Of PeakMs2)() = cluster _
                    .GroupBy(Function(a) a.rt, offsets:=rt_win) _
                    .ToArray

                If dia_n < 2 Then
                    For Each line As SpectrumLine In GridLineNoDecompose(rt_groups)
                        Yield line
                    Next
                Else
                    For Each line As SpectrumLine In GridLineDecompose(rt_groups)
                        Yield line
                    Next
                End If
            Next
        Next
    End Function

    Private Iterator Function GridLineDecompose(rt_groups As NamedCollection(Of PeakMs2)()) As IEnumerable(Of SpectrumLine)
        For Each group As NamedCollection(Of PeakMs2) In rt_groups
            Dim nmf As New DIADecompose(group, tqdm:=False)
            Dim dia_nmf = nmf.DecomposeSpectrum(dia_n).ToArray

            For Each decompose_group As NamedCollection(Of PeakMs2) In dia_nmf
                If decompose_group.Length = 0 Then
                    Continue For
                End If

                Dim fileIndex = decompose_group.GroupBy(Function(si) si.file) _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return a.ToArray
                                  End Function)
                Dim i2 As Double() = filenames _
                    .Select(Function(name)
                                Return If(fileIndex.ContainsKey(name),
                                    fileIndex(name).Average(Function(i) i.intensity), 0)
                            End Function) _
                    .ToArray

                Yield New SpectrumLine With {
                    .intensity = SumNorm(i2),
                    .cluster = decompose_group.value,
                    .rt = Val(decompose_group.name),
                    .mz = decompose_group.Average(Function(si) si.mz)
                }
            Next
        Next
    End Function

    ''' <summary>
    ''' just populate each cluster as <see cref="SpectrumLine"/> data model
    ''' </summary>
    ''' <param name="rt_groups"></param>
    ''' <returns></returns>
    Private Iterator Function GridLineNoDecompose(rt_groups As NamedCollection(Of PeakMs2)()) As IEnumerable(Of SpectrumLine)
        For Each group As NamedCollection(Of PeakMs2) In rt_groups
            Dim fileIndex = group.GroupBy(Function(si) si.file) _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return a.ToArray
                              End Function)
            Dim i2 As Double() = filenames _
                .Select(Function(name)
                            Return If(fileIndex.ContainsKey(name),
                                fileIndex(name).Average(Function(i) i.intensity), 0)
                        End Function) _
                .ToArray
            ' needs make sum of the spectrum in each rawdata file?
            If fileIndex.Any(Function(a) a.Value.Length > 1) Then
                fileIndex = fileIndex _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Dim duplicated = a.Value

                                      If duplicated.Length = 1 Then
                                          Return duplicated
                                      End If

                                      Dim sum = duplicated.SpectrumSum
                                      Dim max = duplicated.OrderByDescending(Function(ai) ai.intensity).First

                                      max = New PeakMs2(max) With {
                                         .mzInto = sum.Array
                                      }

                                      Return {max}
                                  End Function)
            End If

            Yield New SpectrumLine With {
                .intensity = SumNorm(i2),
                .cluster = fileIndex.Values _
                    .IteratesALL _
                    .ToArray,
                .rt = Val(group.name),
                .mz = group.Average(Function(si) si.mz)
            }
        Next
    End Function

    Private Function SumNorm(ByRef v As Double()) As Double()
        Dim pos = v.Where(Function(vi) vi > 0).ToArray

        ' all element is zero!
        If pos.Length = 0 Then
            Return v
        End If

        Dim minPos As Double = pos.Min / 2

        For i As Integer = 0 To v.Length - 1
            If v(i) <= 0.0 Then
                v(i) = minPos
            End If
        Next

        Return SIMD.Multiply.f64_scalar_op_multiply_f64(10000, SIMD.Divide.f64_op_divide_f64_scalar(v, v.Sum))
    End Function

    Public Iterator Function AssignPeaks(peaks As IEnumerable(Of xcms2),
                                         Optional assign_top As Integer = 3,
                                         Optional positive_cor As Boolean = True) As IEnumerable(Of RawPeakAssign)
        Dim q As New SpectrumLine
        Dim mapped As New Dictionary(Of String, SpectrumLine)

        For Each peak As xcms2 In TqdmWrapper.Wrap(peaks.ToArray, wrap_console:=App.EnableTqdm)
            Dim i1 As Double() = SumNorm(peak(filenames))
            Dim candidates = clusters _
                .Search(q.SetRT(peak.rt), tolerance:=rt_win) _
                .Where(Function(c)
                           Return std.Abs(c.mz - peak.mz) < 0.5
                       End Function) _
                .AsParallel _
                .Select(Function(c)
                            Dim cor As Double, pval As Double
                            cor = Correlations.GetPearson(i1, c.intensity, prob2:=pval, throwMaxIterError:=False)
                            Return (c, cor, pval, score:=cor / (std.Abs(peak.rt - c.rt) + 1))
                        End Function) _
                .Where(Function(c) If(positive_cor, c.cor > 0, True)) _
                .OrderByDescending(Function(c) c.score) _
                .Take(assign_top) _
                .ToArray

            For Each candidate In candidates
                mapped(candidate.c.hashKey) = candidate.c

                Yield New RawPeakAssign With {
                    .peak = peak,
                    .ms2 = candidate.c.cluster _
                        .Select(Function(c)
                                    c = New PeakMs2(c)

                                    If c.meta Is Nothing Then
                                        c.meta = New Dictionary(Of String, String) From {
                                            {"ROI", peak.ID}
                                        }
                                    Else
                                        c.meta!ROI = peak.ID
                                    End If

                                    c.mz = peak.mz
                                    c.meta!cor = candidate.cor
                                    c.meta!pval = candidate.pval
                                    c.meta!rt_offset = std.Abs(c.rt - peak.rt)

                                    Return c
                                End Function) _
                        .ToArray,
                    .cor = candidate.cor,
                    .score = candidate.score,
                    .pval = candidate.pval,
                    .v1 = i1,
                    .v2 = candidate.c.intensity
                }
            Next
        Next

        Call unmapped.Clear()

        For Each line As SpectrumLine In clusters.raw
            If Not mapped.ContainsKey(line.hashKey) Then
                Call unmapped.Add(line)
            End If
        Next
    End Function

End Class
