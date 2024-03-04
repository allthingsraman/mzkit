﻿#Region "Microsoft.VisualBasic::385a473a618dd77f9a395bf4bd0d54a6, mzkit\src\metadna\metaDNA\Algorithm.vb"

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

'   Total Lines: 366
'    Code Lines: 267
' Comment Lines: 46
'   Blank Lines: 53
'     File Size: 13.48 KB


' Class Algorithm
' 
'     Properties: ms1Err
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: (+2 Overloads) alignKeggCompound, (+2 Overloads) DIASearch, ExportTable, GetBestQuery, GetCandidateSeeds
'               GetPerfermanceCounter, GetUnknownSet, querySingle, RunInfer, RunIteration
'               SetKeggLibrary, SetNetwork, SetReportHandler, (+2 Overloads) SetSamples, SetSearchRange
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1.PrecursorType
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.Xml
Imports BioNovoGene.BioDeep.Chemoinformatics
Imports BioNovoGene.BioDeep.MetaDNA.Infer
Imports BioNovoGene.BioDeep.MSEngine
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject
Imports std = System.Math

''' <summary>
''' implements of the metadna algorithm in VisualBasic language
''' </summary>
Public Class Algorithm

    ''' <summary>
    ''' tolerance error between two ms1 m/z in ppm
    ''' </summary>
    ReadOnly ms1ppm As Tolerance
    ReadOnly dotcutoff As Double
    ReadOnly MSalignment As AlignmentProvider
    ReadOnly mzwidth As Tolerance
    ReadOnly allowMs1 As Boolean = True

    Dim precursorTypes As MzCalculator()
    Dim typeOrders As Index(Of String)

    Dim unknowns As UnknownSet
    Dim kegg As MSSearch(Of GenericCompound)
    Dim network As Networking
    Dim maxIterations As Integer = 1000
    Dim report As Action(Of String)

    Public ReadOnly Property ms1Err As Tolerance
        Get
            Return ms1ppm
        End Get
    End Property

#Region "algorithm initialization"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="ms1ppm"></param>
    ''' <param name="dotcutoff"></param>
    ''' <param name="mzwidth">
    ''' the product m/z tolerance of the ms2 data
    ''' </param>
    ''' <param name="allowMs1"></param>
    ''' <param name="maxIterations"></param>
    Sub New(ms1ppm As Tolerance,
            dotcutoff As Double,
            mzwidth As Tolerance,
            Optional allowMs1 As Boolean = True,
            Optional maxIterations As Integer = 1000)

        Me.ms1ppm = ms1ppm
        Me.dotcutoff = dotcutoff
        Me.MSalignment = New CosAlignment(mzwidth, LowAbundanceTrimming.Default)
        Me.mzwidth = mzwidth
        Me.allowMs1 = allowMs1
        Me.maxIterations = maxIterations
        Me.report = AddressOf Console.WriteLine
    End Sub

    Public Function SetReportHandler(report As Action(Of String)) As Algorithm
        Me.report = report
        Return Me
    End Function

    Public Function SetSearchRange(ParamArray precursorTypes As String()) As Algorithm
        Me.precursorTypes = precursorTypes _
            .Select(Function(name)
                        Return Parser.ParseMzCalculator(name, name.Last)
                    End Function) _
            .ToArray
        Me.typeOrders = precursorTypes

        Return Me
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetUnknownSet() As UnknownSet
        Return unknowns
    End Function

    ''' <summary>
    ''' create sample data set: <see cref="unknowns"/>
    ''' </summary>
    ''' <param name="sample"></param>
    ''' <returns></returns>
    Public Function SetSamples(sample As IEnumerable(Of PeakMs2), Optional autoROIid As Boolean = True) As Algorithm
        If autoROIid Then
            ' 20210318
            ' toarray is required at here
            ' or stack overflow error will be happends
            sample = (Iterator Function() As IEnumerable(Of PeakMs2)
                          For Each peak As PeakMs2 In sample
                              If Not peak.meta.ContainsKey("ROI") Then
                                  If CInt(peak.rt) = 0 Then
                                      peak.meta!ROI = $"M{CInt(peak.mz)}"
                                  Else
                                      peak.meta!ROI = $"M{CInt(peak.mz)}T{CInt(peak.rt)}"
                                  End If
                              End If

                              Yield peak
                          Next
                      End Function)().ToArray
        End If

        unknowns = UnknownSet.CreateTree(sample, ms1ppm)

        Return Me
    End Function

    Public Function SetSamples(sample As UnknownSet) As Algorithm
        unknowns = sample
        Return Me
    End Function

    ''' <summary>
    ''' 必须要先执行<see cref="SetSearchRange"/>
    ''' </summary>
    ''' <param name="library"></param>
    ''' <returns></returns>
    Public Function SetKeggLibrary(library As IEnumerable(Of Compound)) As Algorithm
        kegg = CompoundSolver.CreateIndex(library, precursorTypes, ms1ppm)
        Return Me
    End Function

    Public Function SetLibrary(solver As CompoundSolver) As Algorithm
        kegg = solver
        Return Me
    End Function

    Public Function SetNetwork(classLinks As IEnumerable(Of ReactionClass)) As Algorithm
        network = KEGGNetwork.CreateNetwork(classLinks)
        Return Me
    End Function

    Public Function SetNetwork(networking As Networking) As Algorithm
        network = networking
        Return Me
    End Function
#End Region

    ''' <summary>
    ''' Create infer network
    ''' </summary>
    ''' <param name="seeds"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function RunIteration(seeds As IEnumerable(Of AnnotatedSeed)) As IEnumerable(Of InferLink)
        Return seeds _
            .ToArray _
            .AsParallel _
            .Select(AddressOf RunInfer) _
            .IteratesALL
    End Function

    Private Iterator Function RunInfer(seed As AnnotatedSeed) As IEnumerable(Of InferLink)
        For Each kegg_id As String In network.FindPartners(seed.kegg_id)
            Dim compound As GenericCompound = kegg.GetCompound(kegg_id)

            If compound Is Nothing OrElse compound.ExactMass <= 0 Then
                Continue For
            End If

            For Each hit As InferLink In alignKeggCompound(seed, compound)
                Yield hit
            Next
        Next
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function alignKeggCompound(seed As AnnotatedSeed, compound As GenericCompound) As IEnumerable(Of InferLink)
        Return precursorTypes _
            .Select(Function(type)
                        Return alignKeggCompound(type, seed, compound)
                    End Function) _
            .IteratesALL
    End Function

    Private Iterator Function alignKeggCompound(type As MzCalculator, seed As AnnotatedSeed, compound As GenericCompound) As IEnumerable(Of InferLink)
        Dim mz As Double = type.CalcMZ(compound.ExactMass)
        Dim candidates As PeakMs2() = unknowns.QueryByParentMz(mz).ToArray

        If candidates.IsNullOrEmpty Then
            Return
        End If

        For Each hit As PeakMs2 In candidates
            Dim alignment As InferLink = GetBestQuery(hit, seed)
            Dim kegg As New MzQuery With {
                .mz = mz,
                .unique_id = compound.Identity,
                .precursorType = type.ToString,
                .ppm = PPMmethod.PPM(mz, hit.mz)
            }

            If alignment Is Nothing Then
                Continue For
            End If

            alignment.kegg = kegg

            If std.Min(alignment.forward, alignment.reverse) < dotcutoff Then
                If alignment.jaccard >= 0.5 Then
                    alignment.level = InferLevel.Ms2
                    alignment.parentTrace *= (0.95 * dotcutoff)
                ElseIf allowMs1 Then
                    alignment.alignments = Nothing
                    alignment.level = InferLevel.Ms1
                    alignment.parentTrace *= (0.5 * dotcutoff)
                Else
                    Continue For
                End If
            Else
                alignment.level = InferLevel.Ms2
                alignment.parentTrace *= std.Min(alignment.forward, alignment.reverse)
            End If

            Yield alignment
        Next
    End Function

    Private Function GetBestQuery(hit As PeakMs2, seed As AnnotatedSeed) As InferLink
        Dim max As InferLink = Nothing
        Dim align As AlignmentOutput
        Dim score As (forward#, reverse#)

        For Each ref In seed.products.Where(Function(sd) sd.Key <> hit.lib_guid)
            align = MSalignment.CreateAlignment(hit.mzInto, ref.Value.ms2)
            score = MSalignment.GetScore(align.alignments)

            If max Is Nothing OrElse std.Min(score.forward, score.reverse) > std.Min(max.forward, max.reverse) Then
                max = New InferLink With {
                    .reverse = score.reverse,
                    .forward = score.forward,
                    .jaccard = GlobalAlignment.JaccardIndex(hit.mzInto, ref.Value.ms2, mzwidth),
                    .alignments = align.alignments,
                    .query = New Meta With {
                        .id = hit.lib_guid,
                        .mz = hit.mz,
                        .scan_time = hit.rt,
                        .intensity = hit.intensity
                    },
                    .reference = New Meta With {
                        .id = seed.ToString,
                        .mz = seed.parent.mz,
                        .scan_time = seed.parent.scan_time,
                        .intensity = seed.parent.intensity
                    },
                    .parentTrace = seed.parentTrace,
                    .inferSize = seed.inferSize + 1,
                    .rawFile = hit.file
                }
            End If
        Next

        Return max
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ExportTable(result As IEnumerable(Of CandidateInfer), Optional unique As Boolean = False) As IEnumerable(Of MetaDNAResult)
        Dim data = result.ExportTable(kegg, keggNetwork:=network)

        If unique Then
            data = data.GetUniques(typeOrders)
        End If

        Return data.OrderBy(Function(r) r.rt)
    End Function

    ReadOnly perfermanceCounter As New List(Of (Integer, TimeSpan, Integer, Integer, Integer))

    Public Function GetPerfermanceCounter() As (iteration As Integer, ticks As TimeSpan, inferLinks As Integer, seeding As Integer, candidates As Integer)()
        Return perfermanceCounter.ToArray
    End Function

    ''' <summary>
    ''' 基于种子进行推断注释
    ''' </summary>
    ''' <param name="seeds"></param>
    ''' <returns></returns>
    Public Iterator Function DIASearch(seeds As IEnumerable(Of AnnotatedSeed)) As IEnumerable(Of CandidateInfer)
        Dim result As InferLink()
        Dim seeding As New SeedsProvider(
            unknowns:=unknowns,
            ranges:=precursorTypes,
            kegg:=kegg
        )
        Dim candidates As CandidateInfer()
        Dim i As i32 = 1
        Dim n As Integer = 0
        Dim start As Long = App.NanoTime
        Dim tickTime As TimeSpan

        Call perfermanceCounter.Clear()

        Do
            result = RunIteration(seeds).ToArray
            candidates = seeding.CandidateInfers(infer:=result).ToArray
            seeds = seeding.Seeding(infers:=candidates).ToArray

            For Each seed As AnnotatedSeed In seeds
                Call unknowns.AddTrace(seed.products.Keys)
            Next
            For Each infer As CandidateInfer In candidates
                Yield infer
            Next

            n += candidates.Length
            tickTime = TimeSpan.FromTicks(App.NanoTime - start)
            start = App.NanoTime

            Call perfermanceCounter.Add((CInt(i), tickTime, result.Length, seeds.Count, n))
            Call report($"[iteration {++i}, {tickTime.FormatTime}] infers {result.Length}, find {seeds.Count} seeds, {n} current candidates ...")

            If i > maxIterations Then
                Call report($"Max iteration number {maxIterations} has been reached, exit metaDNA infer loop!")
                Exit Do
            End If
        Loop While result.Length > 0
    End Function

    ''' <summary>
    ''' 单纯的进行推断，没有种子做基础
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 算法模块测试用
    ''' </remarks>
    Public Function DIASearch() As IEnumerable(Of CandidateInfer)
        Return DIASearch(GetCandidateSeeds)
    End Function

    Private Function GetCandidateSeeds() As IEnumerable(Of AnnotatedSeed)
        Call report("Create candidate seeds by query KEGG library...")

        Return unknowns _
            .EnumerateAllUnknownFeatures _
            .AsParallel _
            .Select(AddressOf querySingle) _
            .IteratesALL
    End Function

    Private Iterator Function querySingle(unknown As PeakMs2) As IEnumerable(Of AnnotatedSeed)
        Dim seedRef As New LibraryMatrix With {
            .ms2 = unknown.mzInto,
            .name = unknown.ToString
        }

        For Each DIAseed As MzQuery In kegg.QueryByMz(unknown.mz)
            Yield New AnnotatedSeed With {
                .id = unknown.lib_guid,
                .kegg_id = DIAseed.unique_id,
                .parent = New ms1_scan With {
                    .mz = unknown.mz,
                    .scan_time = unknown.rt,
                    .intensity = unknown.Ms2Intensity
                },
                .products = New Dictionary(Of String, LibraryMatrix) From {
                    {unknown.lib_guid, seedRef}
                },
                .parentTrace = 100,
                .inferSize = 1
            }
        Next
    End Function

End Class
