﻿#Region "Microsoft.VisualBasic::dc8f09faca442980b6908d1f6f0008f4, mzkit\Rscript\Library\mzkit.insilicons\metaDNA.vb"

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

    '   Total Lines: 538
    '    Code Lines: 377
    ' Comment Lines: 88
    '   Blank Lines: 73
    '     File Size: 22.83 KB


    ' Module metaDNAInfer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CreateKEGGSearch, DIAInfer, ExportNetwork, getResultTable, handleSample
    '               InferTable, loadCompoundLibrary, loadKeggNetwork, loadMetaDNAInferNetwork, MetaDNAAlgorithm
    '               MgfSeeds, readReactionClassTable, ResultAlignments, ResultTable, SaveAlgorithmPerfermance
    '               SetInferNetwork, SetKeggLibrary, SetSearchRange
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1.PrecursorType
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.BioDeep
Imports BioNovoGene.BioDeep.MetaDNA
Imports BioNovoGene.BioDeep.MetaDNA.Infer
Imports BioNovoGene.BioDeep.MetaDNA.Visual
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.genomics.Data
Imports SMRUCC.genomics.Data.KEGG.Metabolism
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports KeggCompound = SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject.Compound
Imports kegReactionClass = SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject.ReactionClass
Imports MetaDNAAlgorithm = BioNovoGene.BioDeep.MetaDNA.Algorithm
Imports ReactionClass = SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject.ReactionClass
Imports ReactionClassTbl = BioNovoGene.BioDeep.MetaDNA.Visual.ReactionClass
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' Metabolic Reaction Network-based Recursive Metabolite Annotation for Untargeted Metabolomics
''' </summary>
<Package("metadna",
         Category:=APICategories.ResearchTools,
         Cites:="X. Shen, R. Wang, X. Xiong, Y. Yin, Y. Cai, Z. Ma, N. Liu, and Z.-J. Zhu* (Corresponding Author), 
         Metabolic Reaction Network-based Recursive Metabolite Annotation for Untargeted Metabolomics, 
         Nature Communications, 
         2019, 10: 1516.")>
<RTypeExport("metadna", GetType(MetaDNAAlgorithm))>
Module metaDNAInfer

    Sub New()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(MetaDNAResult()), AddressOf getResultTable)
    End Sub

    Private Function getResultTable(list As MetaDNAResult(), args As list, env As Environment) As dataframe
        Dim data As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        data.columns(NameOf(MetaDNAResult.ROI_id)) = list.Select(Function(i) i.ROI_id).ToArray
        data.columns(NameOf(MetaDNAResult.query_id)) = list.Select(Function(i) i.query_id).ToArray
        data.columns(NameOf(MetaDNAResult.mz)) = list.Select(Function(i) i.mz).ToArray
        data.columns(NameOf(MetaDNAResult.rt)) = list.Select(Function(i) i.rt).ToArray
        data.columns(NameOf(MetaDNAResult.intensity)) = list.Select(Function(i) i.intensity).ToArray
        data.columns(NameOf(MetaDNAResult.ppm)) = list.Select(Function(i) i.ppm).ToArray
        data.columns(NameOf(MetaDNAResult.KEGGId)) = list.Select(Function(i) i.KEGGId).ToArray
        data.columns(NameOf(MetaDNAResult.precursorType)) = list.Select(Function(i) i.precursorType).ToArray
        data.columns(NameOf(MetaDNAResult.name)) = list.Select(Function(i) i.name).ToArray
        data.columns(NameOf(MetaDNAResult.formula)) = list.Select(Function(i) i.formula).ToArray
        data.columns(NameOf(MetaDNAResult.exactMass)) = list.Select(Function(i) i.exactMass).ToArray
        data.columns(NameOf(MetaDNAResult.mzCalc)) = list.Select(Function(i) i.mzCalc).ToArray

        data.columns(NameOf(MetaDNAResult.inferLevel)) = list.Select(Function(i) i.inferLevel).ToArray
        data.columns(NameOf(MetaDNAResult.inferSize)) = list.Select(Function(i) i.inferSize).ToArray
        data.columns(NameOf(MetaDNAResult.forward)) = list.Select(Function(i) i.forward).ToArray
        data.columns(NameOf(MetaDNAResult.reverse)) = list.Select(Function(i) i.reverse).ToArray
        data.columns(NameOf(MetaDNAResult.jaccard)) = list.Select(Function(i) i.jaccard).ToArray
        data.columns(NameOf(MetaDNAResult.mirror)) = list.Select(Function(i) i.mirror).ToArray
        data.columns(NameOf(MetaDNAResult.score1)) = list.Select(Function(i) i.score1).ToArray
        data.columns(NameOf(MetaDNAResult.score2)) = list.Select(Function(i) i.score2).ToArray
        data.columns(NameOf(MetaDNAResult.pvalue)) = list.Select(Function(i) i.pvalue).ToArray

        data.columns(NameOf(MetaDNAResult.seed)) = list.Select(Function(i) i.seed).ToArray
        data.columns(NameOf(MetaDNAResult.parentTrace)) = list.Select(Function(i) i.parentTrace).ToArray
        data.columns(NameOf(MetaDNAResult.partnerKEGGId)) = list.Select(Function(i) i.partnerKEGGId).ToArray
        data.columns(NameOf(MetaDNAResult.reaction)) = list.Select(Function(i) i.reaction).ToArray
        data.columns(NameOf(MetaDNAResult.KEGG_reaction)) = list.Select(Function(i) i.KEGG_reaction).ToArray

        Return data
    End Function

    ''' <summary>
    ''' Load network graph model from the kegg metaDNA infer network data.
    ''' </summary>
    ''' <param name="debugOutput"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("read.metadna.infer")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function loadMetaDNAInferNetwork(debugOutput As Object, Optional env As Environment = Nothing) As Object
        If debugOutput Is Nothing Then
            Return Nothing
        ElseIf debugOutput.GetType Is GetType(String) Then
            debugOutput = DirectCast(debugOutput, String).LoadXml(Of MetaDNA.Visual.XML)
        End If

        If Not TypeOf debugOutput Is MetaDNA.Visual.XML Then
            Return REnv.Internal.debug.stop(New InvalidCastException, env)
        End If

        Return DirectCast(debugOutput, MetaDNA.Visual.XML).CreateGraph
    End Function

    ''' <summary>
    ''' load kegg reaction class data in table format from given file
    ''' </summary>
    ''' <param name="file">csv table file or a directory with raw xml model data file in it.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("reaction_class.table")>
    <RApiReturn(GetType(ReactionClassTbl()))>
    Public Function readReactionClassTable(file As String, Optional env As Environment = Nothing) As Object
        If file.ExtensionSuffix("csv") Then
            Return file.LoadCsv(Of ReactionClassTbl).ToArray
        ElseIf file.DirectoryExists Then
            Return kegReactionClass _
                .ScanRepository(file, loadsAll:=True) _
                .Select(Function(cls)
                            Return cls.reactantPairs _
                                .Select(Function(r)
                                            Return New ReactionClassTbl With {
                                                .define = cls.definition,
                                                .from = r.from,
                                                .[to] = r.to,
                                                .rId = cls.entryId,
                                                .category = Integer.Parse(cls.category.Match("\d"))
                                            }
                                        End Function)
                        End Function) _
                .IteratesALL _
                .ToArray
        Else
            Return Internal.debug.stop($"unable to determin the data source type of the given file '{file}'", env)
        End If
    End Function

#Region "metadna algorithm"

    <ExportAPI("metadna")>
    <RApiReturn(GetType(MetaDNAAlgorithm))>
    Public Function MetaDNAAlgorithm(Optional ms1ppm As Object = "ppm:20",
                                     Optional mzwidth As Object = "da:0.3",
                                     Optional dotcutoff As Double = 0.5,
                                     Optional allowMs1 As Boolean = True,
                                     Optional maxIterations As Integer = 1000,
                                     Optional env As Environment = Nothing) As Object

        Dim ms1Err As [Variant](Of Tolerance, Message) = Math.getTolerance(ms1ppm, env)
        Dim mz2Err As [Variant](Of Tolerance, Message) = Math.getTolerance(mzwidth, env)

        If ms1Err Like GetType(Message) Then
            Return ms1Err.TryCast(Of Message)
        ElseIf mz2Err Like GetType(Message) Then
            Return mz2Err.TryCast(Of Message)
        End If

        Return New MetaDNAAlgorithm(ms1Err, dotcutoff, mz2Err, allowMs1, maxIterations)
    End Function

    <ExportAPI("range")>
    <RApiReturn(GetType(MetaDNAAlgorithm))>
    Public Function SetSearchRange(metadna As Algorithm,
                                   <RRawVectorArgument>
                                   precursorTypes As Object,
                                   Optional env As Environment = Nothing) As Object

        Dim types As String() = CLRVector.asCharacter(precursorTypes)

        If env.globalEnvironment.options.verbose Then
            Call base.print("Set precursor types:", , env)
            Call base.print(types, , env)
        End If

        Return metadna.SetSearchRange(types)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="metadna"></param>
    ''' <param name="kegg">
    ''' a collection of the kegg compound data.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("load.kegg")>
    <RApiReturn(GetType(MetaDNAAlgorithm))>
    Public Function SetKeggLibrary(metadna As Algorithm,
                                   <RRawVectorArgument> kegg As Object,
                                   Optional env As Environment = Nothing) As Object

        Dim library As pipeline = pipeline.TryCreatePipeline(Of KeggCompound)(kegg, env)

        If library.isError Then
            Return library.getError
        End If

        Return metadna.SetKeggLibrary(library.populates(Of KeggCompound)(env))
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="metadna"></param>
    ''' <param name="links">
    ''' a collection of the reaction class data
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("load.kegg_network")>
    <RApiReturn(GetType(MetaDNAAlgorithm))>
    Public Function SetInferNetwork(metadna As Algorithm,
                                    <RRawVectorArgument> links As Object,
                                    Optional env As Environment = Nothing) As Object

        Dim network As pipeline = pipeline.TryCreatePipeline(Of ReactionClass)(links, env)

        If network.isError Then
            Return network.getError
        End If

        Return metadna.SetNetwork(network.populates(Of ReactionClass)(env))
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="metadna"></param>
    ''' <param name="sample">
    ''' a collection of the mzkit peak ms2 data objects
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("load.raw")>
    <RApiReturn(GetType(MetaDNAAlgorithm))>
    Public Function handleSample(metadna As Algorithm,
                                 <RRawVectorArgument> sample As Object,
                                 Optional env As Environment = Nothing) As Object

        Dim raw As pipeline = pipeline.TryCreatePipeline(Of PeakMs2)(sample, env)

        If raw.isError Then
            Return raw.getError
        End If

        Return metadna.SetSamples(raw.populates(Of PeakMs2)(env))
    End Function

    <ExportAPI("DIA.infer")>
    <RApiReturn(GetType(CandidateInfer))>
    Public Function DIAInfer(metaDNA As Algorithm,
                             <RRawVectorArgument> sample As Object,
                             <RRawVectorArgument> Optional seeds As Object = Nothing,
                             Optional env As Environment = Nothing) As Object

        Dim raw As pipeline = pipeline.TryCreatePipeline(Of PeakMs2)(sample, env)
        Dim infer As CandidateInfer()

        If raw.isError Then
            Return raw.getError
        End If

        If seeds Is Nothing Then
            infer = metaDNA _
                .SetSamples(raw.populates(Of PeakMs2)(env)) _
                .DIASearch _
                .ToArray
        ElseIf TypeOf seeds Is dataframe Then
            infer = DirectCast(seeds, dataframe).InferTable(raw, metaDNA, env)
        Else
            Throw New NotImplementedException
        End If

        Return infer
    End Function

    <Extension>
    Private Function InferTable(seeds As dataframe, raw As pipeline, metaDNA As Algorithm, env As Environment) As CandidateInfer()
        Dim id As String() = DirectCast(seeds, dataframe).getColumnVector(1)
        Dim kegg_id As String() = DirectCast(seeds, dataframe).getColumnVector(2)
        Dim rawFile As UnknownSet = UnknownSet.CreateTree(raw.populates(Of PeakMs2)(env), metaDNA.ms1Err)
        Dim annoSet As NamedValue(Of String)() = id _
            .Select(Function(uid, i) (uid, kegg_id(i))) _
            .GroupBy(Function(map) map.uid) _
            .Select(Function(map)
                        Return map _
                            .GroupBy(Function(anno) anno.Item2) _
                            .Select(Function(anno)
                                        Return New NamedValue(Of String) With {
                                            .Name = map.Key,
                                            .Value = anno.Key
                                        }
                                    End Function)
                    End Function) _
            .IteratesALL _
            .Where(Function(map)
                       Return map.Value.IsPattern("C\d+")
                   End Function) _
            .ToArray
        Dim seedsRaw As AnnotatedSeed()

        If env.globalEnvironment.options.verbose Then
            Call base.print("Create seeds by dataframe...", , env)
        End If

        seedsRaw = rawFile.CreateAnnotatedSeeds(annoSet).ToArray

        If env.globalEnvironment.options.verbose Then
            Call base.print($"We create {seedsRaw.Length} seeds for running metaDNA algorithm!", , env)
        End If

        Return metaDNA _
            .SetSamples(rawFile) _
            .DIASearch(seedsRaw) _
            .ToArray
    End Function

    ''' <summary>
    ''' create seeds from mgf file data
    ''' </summary>
    ''' <param name="seeds"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.seeds")>
    <RApiReturn(GetType(AnnotatedSeed))>
    Public Function MgfSeeds(<RRawVectorArgument> seeds As Object, Optional env As Environment = Nothing) As Object
        Dim seedList As pipeline = pipeline.TryCreatePipeline(Of PeakMs2)(seeds, env)

        If seedList.isError Then
            Return seedList.getError
        End If

        Return seedList _
            .populates(Of PeakMs2)(env) _
            .MgfSeeds _
            .ToArray
    End Function

#End Region

#Region "result output"

    ''' <summary>
    ''' get result alignments raw data for data plots.
    ''' </summary>
    ''' <param name="DIAinfer"></param>
    ''' <param name="table"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("result.alignment")>
    <RApiReturn(GetType(MetaDNARawSet))>
    Public Function ResultAlignments(<RRawVectorArgument> DIAinfer As Object,
                                     <RRawVectorArgument> table As Object,
                                     Optional env As Environment = Nothing) As Object

        Dim raw As pipeline = pipeline.TryCreatePipeline(Of CandidateInfer)(DIAinfer, env)
        Dim filter As pipeline = pipeline.TryCreatePipeline(Of MetaDNAResult)(table, env)

        If raw.isError Then
            Return raw.getError
        ElseIf filter.isError Then
            Return filter.getError
        End If

        Dim rawFilter As MetaDNAResult() = filter.populates(Of MetaDNAResult)(env).ToArray

        Return raw _
            .populates(Of CandidateInfer)(env) _
            .ExportInferRaw(rawFilter)
    End Function

    <ExportAPI("as.table")>
    <RApiReturn(GetType(MetaDNAResult))>
    Public Function ResultTable(metaDNA As Algorithm,
                                <RRawVectorArgument>
                                result As Object,
                                Optional unique As Boolean = False,
                                Optional env As Environment = Nothing) As Object

        Dim data As pipeline = pipeline.TryCreatePipeline(Of CandidateInfer)(result, env)

        If data.isError Then
            Return data.getError
        End If

        Return metaDNA _
            .ExportTable(data.populates(Of CandidateInfer)(env), unique) _
            .ToArray
    End Function

    <ExportAPI("as.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function ExportNetwork(<RRawVectorArgument> result As Object, Optional env As Environment = Nothing) As Object
        Dim data As pipeline = pipeline.TryCreatePipeline(Of MetaDNAResult)(result, env)

        If data.isError Then
            Return data.getError
        End If

        Return data.populates(Of MetaDNAResult)(env).ExportNetwork
    End Function

    <ExportAPI("as.ticks")>
    Public Function SaveAlgorithmPerfermance(metaDNA As Algorithm) As dataframe
        Dim counter = metaDNA.GetPerfermanceCounter
        Dim iteration As Integer() = counter.Select(Function(c) c.iteration).ToArray
        Dim ticks As String() = counter.Select(Function(c) c.ticks.FormatTime).ToArray
        Dim inferLinks As Integer() = counter.Select(Function(c) c.inferLinks).ToArray
        Dim seeding As Integer() = counter.Select(Function(c) c.seeding).ToArray
        Dim candidates As Integer() = counter.Select(Function(c) c.candidates).ToArray

        Return New dataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {NameOf(iteration), iteration},
                {NameOf(ticks), ticks},
                {NameOf(inferLinks), inferLinks},
                {NameOf(seeding), seeding},
                {NameOf(candidates), candidates}
            }
        }
    End Function
#End Region

#Region "kegg"

    ''' <summary>
    ''' create the kegg compound ms1 annotation query engine.
    ''' </summary>
    ''' <param name="kegg">
    ''' a set of kegg compound data
    ''' </param>
    ''' <param name="precursors">
    ''' a character vector of the ms1 precursor ion names or 
    ''' a list of the given mzcalculator object models.
    ''' </param>
    ''' <param name="mzdiff">
    ''' the mass tolerance value to match between the 
    ''' experiment m/z value and the reference m/z value
    ''' which is calculated from the compound exact mass
    ''' with a given specific ion precursor type.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' a data query engine model to run ms1 data search 
    ''' for the kegg metaolite compounds.
    ''' </returns>
    <ExportAPI("annotationSet")>
    <RApiReturn(GetType(KEGGHandler))>
    Public Function CreateKEGGSearch(<RRawVectorArgument> kegg As Object,
                                     <RRawVectorArgument()>
                                     Optional precursors As Object = "[M]+|[M+H]+|[M+H-H2O]+",
                                     Optional mzdiff As Object = "ppm:20",
                                     <RRawVectorArgument(TypeCodes.string)>
                                     Optional excludes As Object = Nothing,
                                     Optional env As Environment = Nothing) As Object

        Dim keggSet = pipeline.TryCreatePipeline(Of KeggCompound)(kegg, env)
        Dim mzErr = Math.getTolerance(mzdiff, env)

        If keggSet.isError Then
            Return keggSet.getError
        ElseIf mzErr Like GetType(Message) Then
            Return mzErr.TryCast(Of Message)
        End If

        Dim typeList As pipeline = pipeline.TryCreatePipeline(Of MzCalculator)(precursors, env, suppress:=True)
        Dim calculators As MzCalculator()
        Dim excludesEntry As Index(Of String) = CLRVector.asCharacter(excludes).Indexing

        If typeList.isError Then
            typeList = pipeline.TryCreatePipeline(Of String)(precursors, env, suppress:=True)

            If typeList.isError Then
                Return typeList.getError
            Else
                Dim types As String() = typeList _
                    .populates(Of String)(env) _
                    .Select(Function(str) str.Split("|"c)) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray

                calculators = Provider.Calculators(types)
            End If
        Else
            calculators = typeList _
                .populates(Of MzCalculator)(env) _
                .ToArray
        End If

        Return KEGGHandler.CreateIndex(
            compounds:=keggSet _
                .populates(Of KeggCompound)(env) _
                .Where(Function(c)
                           Return Not c.entry Like excludesEntry
                       End Function),
            types:=calculators,
            tolerance:=mzErr.TryCast(Of Tolerance)
        )
    End Function

    ''' <summary>
    ''' load kegg compounds
    ''' </summary>
    ''' <param name="repo">
    ''' the file path to the messagepack data repository
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("kegg.library")>
    <RApiReturn(GetType(KeggCompound))>
    Public Function loadCompoundLibrary(repo As String) As Object
        If repo.FileExists Then
            Using file As Stream = repo.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return KEGGCompoundPack.ReadKeggDb(file)
            End Using
        Else
            Return CompoundRepository _
                .ScanRepository(repo, ignoreGlycan:=False) _
                .DoCall(AddressOf pipeline.CreateFromPopulator)
        End If
    End Function

    ''' <summary>
    ''' load the kegg reaction class data.
    ''' </summary>
    ''' <param name="repo"></param>
    ''' <returns></returns>
    <ExportAPI("kegg.network")>
    <RApiReturn(GetType(ReactionClass))>
    Public Function loadKeggNetwork(repo As String) As Object
        If repo.FileExists Then
            Using file As Stream = repo.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return ReactionClassPack.ReadKeggDb(file)
            End Using
        Else
            Return ReactionClass _
                .ScanRepository(repo) _
                .DoCall(AddressOf pipeline.CreateFromPopulator)
        End If
    End Function

#End Region

End Module
