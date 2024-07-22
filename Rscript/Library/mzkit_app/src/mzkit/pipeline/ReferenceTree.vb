﻿#Region "Microsoft.VisualBasic::258fbcc9a73902c885e99edd7d748432, Rscript\Library\mzkit_app\src\mzkit\pipeline\ReferenceTree.vb"

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

'   Total Lines: 617
'    Code Lines: 398 (64.51%)
' Comment Lines: 143 (23.18%)
'    - Xml Docs: 90.21%
' 
'   Blank Lines: 76 (12.32%)
'     File Size: 24.82 KB


' Module ReferenceTreePkg
' 
'     Function: addBucket, compress, createJaccardSet, CreateNew, embedding
'               GetSpectrum, GetTestSample, open, (+2 Overloads) QuerySingle, (+2 Overloads) QueryTree
'               ReadPack, set_dotcutoff
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.SpectrumTree
Imports BioNovoGene.Analytical.MassSpectrometry.SpectrumTree.PackLib
Imports BioNovoGene.Analytical.MassSpectrometry.SpectrumTree.Query
Imports BioNovoGene.Analytical.MassSpectrometry.SpectrumTree.Tree
Imports BioNovoGene.BioDeep.MassSpectrometry.MoleculeNetworking
Imports BioNovoGene.BioDeep.MSEngine
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' the spectrum tree reference library tools
''' </summary>
''' <remarks>
''' the spectrum data is clustering and save in family 
''' tree data structure.
''' </remarks>
<Package("spectrumTree")>
Module ReferenceTreePkg

    ''' <summary>
    ''' create new reference spectrum database
    ''' </summary>
    ''' <param name="file">
    ''' A file path to save the spectrum reference database
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("new")>
    <RApiReturn(GetType(ReferenceTree), GetType(ReferenceBinaryTree), GetType(SpectrumPack))>
    Public Function CreateNew(file As Object,
                              Optional type As ClusterTypes = ClusterTypes.Default,
                              Optional env As Environment = Nothing) As Object

        Dim buffer = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Dim stream As Stream = buffer.TryCast(Of Stream)
        Call stream.Seek(Scan0, SeekOrigin.Begin)

        Select Case type
            Case ClusterTypes.Binary : Return New ReferenceBinaryTree(stream)
            Case ClusterTypes.Tree, ClusterTypes.Default : Return New ReferenceTree(stream)
            Case ClusterTypes.Pack : Return New SpectrumPack(stream)
            Case Else
                Return Internal.debug.stop(New NotImplementedException(type.Description), env)
        End Select
    End Function

    ''' <summary>
    ''' Extract the test sample data for run evaluation of the annotation workflow
    ''' </summary>
    ''' <param name="packlib"></param>
    ''' <param name="n"></param>
    ''' <param name="source_name"></param>
    ''' <returns></returns>
    <ExportAPI("get_testSample")>
    <RApiReturn("peaktable", "rawdata")>
    Public Function GetTestSample(packlib As SpectrumReader,
                                  Optional n As Integer = 30,
                                  Optional rtmax As Double = 840,
                                  Optional source_name As String = "get_testSample") As Object

        Dim testData = packlib.GetTestSample(n, rtmax:=rtmax)
        Dim peaktable As Peaktable() = testData.peaks
        Dim raw As New mzPack With {
            .MS = testData.Ms,
            .source = source_name
        }

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"peaktable", peaktable},
                {"rawdata", raw}
            }
        }
    End Function

    ''' <summary>
    ''' open the spectrum pack reference database file
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <keywords>read data</keywords>
    <ExportAPI("readpack")>
    <RApiReturn(GetType(SpectrumReader))>
    Public Function ReadPack(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If buf Like GetType(Message) Then
            Return buf.TryCast(Of Message)
        End If

        Return New SpectrumReader(buf.TryCast(Of Stream))
    End Function

    ''' <summary>
    ''' ### open the spectrum reference database
    ''' 
    ''' open the reference spectrum database file and 
    ''' then create a host to run spectrum cluster 
    ''' search
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <param name="target_uuid">
    ''' a character vector of the target metabolite biodeep_id, default value
    ''' is NULL means load all reference spectrum from the required reference 
    ''' database file. this function will just load a subset of the reference 
    ''' spectrum data from the database file is this parameter value is not 
    ''' NULL.
    ''' </param>
    ''' <param name="adducts">
    ''' the precursor types for build the mass index for the reference library, this 
    ''' parameter is required for reference library model in stream pack object type.
    ''' </param>
    ''' <returns>
    ''' the reference library object in different search mode, all library object 
    ''' is inherits based on the <see cref="Ms2Search"/> object.
    ''' </returns>
    ''' <remarks>
    ''' the data format is test via the magic header
    ''' </remarks>
    ''' <keywords>read data</keywords>
    <ExportAPI("open")>
    <RApiReturn(GetType(PackAlignment), GetType(TreeSearch))>
    Public Function open(<RRawVectorArgument>
                         file As Object,
                         Optional dotcutoff As Double = 0.6,
                         <RRawVectorArgument(TypeCodes.string)>
                         Optional adducts As Object = "[M]+|[M+H]+",
                         <RRawVectorArgument(TypeCodes.string)>
                         Optional target_uuid As Object = Nothing,
                         Optional env As Environment = Nothing) As Object

        Dim buffer = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Dim buf As Stream = buffer.TryCast(Of Stream)
        Dim isHDS = StreamPack.TestMagic(buf)
        Dim targets As String() = CLRVector.asCharacter(target_uuid)
        Dim println = env.WriteLineHandler

        Call buf.Seek(Scan0, SeekOrigin.Begin)

        If Not targets.IsNullOrEmpty Then
            Call println("[spectrumTree::open] only a subset of the metabolite reference spectrum will be run annotation query:")
            Call println(targets)
        End If

        If isHDS Then
            Dim precursor_types = Math.GetPrecursorTypes(adducts, env)
            Dim referenceSpectrum = New SpectrumReader(buf, targets).BuildSearchIndex(println, precursor_types)

            Call println("precursor adducts for filtering targets:")
            Call println(adducts)

            Return New PackAlignment(referenceSpectrum, dotcutoff)
        Else
            Return New TreeSearch(buffer.TryCast(Of Stream)).SetCutoff(dotcutoff)
        End If
    End Function

    ''' <summary>
    ''' export all reference spectrum from the given library object
    ''' </summary>
    ''' <param name="pack"></param>
    ''' <returns></returns>
    <ExportAPI("export_spectrum")>
    Public Function export_reference(pack As PackAlignment) As PeakMs2()
        Return pack.GetReferenceSpectrum.ToArray
    End Function

    ''' <summary>
    ''' set dot cutoff parameter for the cos score similarity algorithm
    ''' </summary>
    ''' <param name="search">
    ''' The spectrum library stream engine
    ''' </param>
    ''' <param name="cutoff">
    ''' cutoff threshold value of the cos score
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("dotcutoff")>
    Public Function set_dotcutoff(search As TreeSearch, cutoff As Double) As TreeSearch
        Call search.SetCutoff(cutoff)
        Return search
    End Function

    ''' <summary>
    ''' enable internal parallel for the spectrum alignment search?
    ''' </summary>
    ''' <param name="search"></param>
    ''' <param name="enable"></param>
    ''' <returns></returns>
    <ExportAPI("parallel")>
    Public Function set_parallel(search As Object, enable As Boolean) As Object
        If TypeOf search Is PackAlignment Then
            DirectCast(search, PackAlignment).parallel = enable
        End If

        Return search
    End Function

    ''' <summary>
    ''' construct a fragment set library for run spectrum search in jaccard index matches method
    ''' </summary>
    ''' <param name="libname"></param>
    ''' <param name="mz"></param>
    ''' <param name="mzset"></param>
    ''' <param name="rt"></param>
    ''' <param name="cutoff"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("jaccardSet")>
    Public Function createJaccardSet(libname As String(),
                                     mz As Double(),
                                     mzset As String(),
                                     Optional rt As Double() = Nothing,
                                     Optional cutoff As Double = 0.1,
                                     Optional filter_complex_adducts As Boolean = False,
                                     Optional env As Environment = Nothing) As JaccardSearch

        Dim dataset As JaccardSet() = libname _
            .Select(Function(a, i)
                        Return New JaccardSet With {
                            .libname = a,
                            .mz1 = mz(i),
                            .rt = rt.ElementAtOrDefault(i),
                            .ms2 = mzset(i) _
                                .Replace("["c, "") _
                                .Replace("]"c, "") _
                                .Replace(" "c, "") _
                                .Split(","c) _
                                .Select(AddressOf Conversion.Val) _
                                .ToArray
                        }
                    End Function) _
            .ToArray
        Dim println = env.WriteLineHandler

        Call println($"Do jaccard match with cutoff value: {cutoff}!")

        Return New JaccardSearch(dataset, cutoff, filter_complex_adducts)
    End Function

    ''' <summary>
    ''' do spectrum family alignment via cos similarity
    ''' </summary>
    ''' <param name="tree">
    ''' The reference spectrum tree object to search 
    ''' </param>
    ''' <param name="x">
    ''' The query spectrum data from the sample raw data files
    ''' </param>
    ''' <param name="maxdepth">
    ''' The max depth of the tree search
    ''' </param>
    ''' <param name="treeSearch">
    ''' Do alignment in family tree search mode?
    ''' </param>
    ''' <param name="top_hits">
    ''' the top n hits of the candidate result populated for the each query input,
    ''' set this parameter value to zero or negative value means no limits.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' function returns nothing means no query hits or the 
    ''' given input query sample data <paramref name="x"/>
    ''' </returns>
    <ExportAPI("query")>
    <RApiReturn(GetType(ClusterHit))>
    <Extension>
    Public Function QueryTree(tree As Ms2Search, x As Object,
                              Optional maxdepth As Integer = 1024,
                              Optional treeSearch As Boolean = False,
                              Optional top_hits As Integer = 3,
                              Optional env As Environment = Nothing) As Object

        If x Is Nothing Then
            Call env.AddMessage("The given spectrum input data is nothing!", MSG_TYPES.WRN)
            Return Nothing
        ElseIf tree Is Nothing Then
            Return Internal.debug.stop("the required spectrum reference library could not be nothing!", env)
        End If

        If TypeOf x Is LibraryMatrix Then
            Return DirectCast(x, LibraryMatrix).QuerySingle(tree, maxdepth, treeSearch, top_n:=top_hits, env)
        ElseIf TypeOf x Is PeakMs2 Then
            Return DirectCast(x, PeakMs2).QuerySingle(tree, maxdepth, treeSearch, top_n:=top_hits, env)
        ElseIf TypeOf x Is list Then
            Return DirectCast(x, list).QueryTree(tree, maxdepth, treeSearch, top_n:=top_hits, env)
        Else
            Throw New NotImplementedException("Not supported input spectrum data type: " & x.GetType.FullName)
        End If
    End Function

    <Extension>
    Private Function QuerySingle(x As LibraryMatrix, tree As Ms2Search,
                                 Optional maxdepth As Integer = 1024,
                                 Optional treeSearch As Boolean = False,
                                 Optional top_n As Integer = 3,
                                 Optional env As Environment = Nothing) As Object

        Dim centroid = tree.Centroid(DirectCast(x, LibraryMatrix).ms2)
        Dim result As ClusterHit()

        If (Not treeSearch) AndAlso x.parentMz <= 0.0 Then
            Return Internal.debug.stop($"mz query required a positive m/z value!", env)
        ElseIf x.Length = 0 Then
            Return Nothing
        End If
        If treeSearch Then
            result = {DirectCast(tree, TreeSearch).Search(centroid, maxdepth:=maxdepth)}
        Else
            ' 20240722 the reference spectrum will be filtered
            ' by the precursor ion
            result = tree.Search(centroid, mz1:=x.parentMz).ToArray
        End If

        Dim basePeak As ms2 = x.ms2 _
            .OrderByDescending(Function(a) a.intensity) _
            .First
        Dim output As New List(Of ClusterHit)

        ' assign the query ion information
        For Each hit As ClusterHit In result
            If Not hit Is Nothing Then
                hit.queryId = x.name
                hit.queryMz = x.parentMz
                hit.basePeak = basePeak.mz
                hit.queryIntensity = x.totalIon

                Call output.Add(hit)
            End If
        Next

        If output.Count = 0 Then
            Return Nothing
        ElseIf top_n <= 0 OrElse output.Count <= top_n Then
            ' populate all
            Return output.ToArray
        Else
            Return output _
                .OrderByDescending(Function(d) d.totalScore) _
                .Take(top_n) _
                .ToArray
        End If
    End Function

    <Extension>
    Private Function QuerySingle(x As PeakMs2, tree As Ms2Search,
                                 Optional maxdepth As Integer = 1024,
                                 Optional treeSearch As Boolean = False,
                                 Optional top_n As Integer = 3,
                                 Optional env As Environment = Nothing) As Object

        Dim centroid = tree.Centroid(DirectCast(x, PeakMs2).mzInto)
        Dim result As ClusterHit()

        If (Not treeSearch) AndAlso x.mz <= 0.0 Then
            Return Internal.debug.stop($"mz query required a positive m/z value!", env)
        ElseIf x.mzInto.Length = 0 Then
            Return Nothing
        End If
        If treeSearch AndAlso TypeOf tree Is TreeSearch Then
            result = {DirectCast(tree, TreeSearch).Search(centroid, maxdepth:=maxdepth)}
        Else
            result = tree.Search(centroid, mz1:=x.mz).ToArray
        End If

        Dim basePeak As ms2 = x.mzInto _
            .OrderByDescending(Function(a) a.intensity) _
            .First
        Dim output As New List(Of ClusterHit)

        ' assign the query ion information
        For Each hit As ClusterHit In result
            If Not hit Is Nothing Then
                hit.queryId = x.lib_guid
                hit.queryMz = x.mz
                hit.queryRt = x.rt
                hit.basePeak = basePeak.mz
                hit.queryIntensity = If(x.intensity <= 0.0, x.Ms2Intensity, x.intensity)

                Call output.Add(hit)
            End If
        Next

        If output.Count = 0 Then
            Return Nothing
        ElseIf top_n <= 0 OrElse output.Count <= top_n Then
            ' populate all hits
            Return output.ToArray
        Else
            Return output _
                .OrderByDescending(Function(d) d.totalScore) _
                .Take(top_n) _
                .ToArray
        End If
    End Function

    <Extension>
    Private Function QueryTree(input As list, tree As Ms2Search,
                               Optional maxdepth As Integer = 1024,
                               Optional treeSearch As Boolean = False,
                               Optional top_n As Integer = 3,
                               Optional env As Environment = Nothing) As Object

        Dim output As New list With {.slots = New Dictionary(Of String, Object)}
        Dim result As Object
        Dim println = env.WriteLineHandler
        Dim i As i32 = 1
        Dim n As Integer = input.length
        Dim d As Integer = input.length * 0.01 + 1
        Dim t0 = now()

        For Each name As String In input.getNames
            result = input(name)

            If Not result Is Nothing Then
                result = tree.QueryTree(result, maxdepth, treeSearch, top_hits:=top_n, env:=env)
            End If

            If Program.isException(result) Then
                Return result
            Else
                Call output.add(name, result)
            End If

            If n > 8 AndAlso ++i Mod d = 0 Then
                Call println($"[query_tree, {(now() - t0).FormatTime}] {(i / n * 100).ToString("F2")}% {name}...")
            End If
        Next

        Return output
    End Function

    Const no_biodeep_id As String = "No metabolite uuid or biodeep id was provided!"
    Const no_formula As String = "A valid formula string text for evaluate the positive exact mass value of the target metabolite must be provided!"

    ''' <summary>
    ''' push the reference spectrum data into the spectrum reference tree library
    ''' </summary>
    ''' <param name="tree">
    ''' The reference spectrum database, which the spectrum data 
    ''' is store in family tree style
    ''' </param>
    ''' <param name="x">
    ''' A new spectrum data to push into the reference database
    ''' </param>
    ''' <param name="args">
    ''' additional parameters for create the spectrum library in spectrum pack format:
    ''' 
    ''' 1. uuid, BioDeepID, biodeep_id is used for the metabolite unique reference id
    ''' 2. chemical_formula, formula is used for the metabolite exact mass value
    ''' 
    ''' and the spectrum input of x should be the same metabolite if save data as 
    ''' the spectrum pack data.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <keywords>save data</keywords>
    <ExportAPI("addBucket")>
    Public Function addBucket(tree As Object,
                              <RRawVectorArgument> x As Object,
                              Optional ignore_error As Boolean = False,
                              <RListObjectArgument>
                              Optional args As list = Nothing,
                              Optional env As Environment = Nothing) As Object

        Dim list As pipeline = pipeline.TryCreatePipeline(Of PeakMs2)(x, env)

        If list.isError Then
            Return list.getError
        ElseIf tree Is Nothing Then
            Return Internal.debug.stop("The required reference library object can not be nothing!", env)
        End If

        If TypeOf tree Is ReferenceTree Then
            For Each spectrum As PeakMs2 In list.populates(Of PeakMs2)(env)
                Call DirectCast(tree, ReferenceTree).Push(spectrum)
            Next
        ElseIf TypeOf tree Is SpectrumPack Then
            Dim uuid As String = args.getValue(Of String)({"uuid", "BioDeepID", "biodeep_id"}, env)
            Dim formula As String = args.getValue(Of String)({"formula", "chemical_formula"}, env)
            Dim name As String = args.getValue({"name", "Name"}, env, [default]:="")

            If uuid.StringEmpty Then
                If ignore_error Then
                    Call env.AddMessage(no_biodeep_id)
                    Return tree
                Else
                    Return Internal.debug.stop(no_biodeep_id, env)
                End If
            End If
            If formula.StringEmpty Then
                If ignore_error Then
                    Call env.AddMessage(no_formula)
                    Return tree
                Else
                    Return Internal.debug.stop(no_formula, env)
                End If
            End If

            If Not name.StringEmpty Then
                ' 20220419
                '
                ' due to the reason of the data file inside the stream pack
                ' is indexed used the uuid string as the reference path
                ' symbol \ or / in the metabolite name will generates an
                ' incorrect reference path, so these two symbol needs to be 
                ' removed from the name
                uuid = $"{uuid}|{SpectrumPack.PathName(name)}"
            End If

            For Each spectrum As PeakMs2 In list.populates(Of PeakMs2)(env)
                Call DirectCast(tree, SpectrumPack).Push(uuid, formula, spectrum)
            Next
        Else
            Return Message.InCompatibleType(GetType(ReferenceTree), tree.GetType, env)
        End If

        Return tree
    End Function

    ''' <summary>
    ''' Compress and make cleanup of the spectrum library
    ''' </summary>
    ''' <param name="spectrumLib"></param>
    ''' <param name="file">A file object for write the spectrum library.</param>
    ''' <param name="metadb">
    ''' metabolite annotation database library for get annotation information
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("compress")>
    Public Function compress(spectrumLib As SpectrumReader, file As Object, metadb As IMetaDb,
                             Optional nspec As Integer = 5,
                             Optional xrefDb As String = Nothing,
                             Optional test As Integer = -1,
                             Optional env As Environment = Nothing) As Object

        Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.ReadWrite, env)

        If buf Like GetType(Message) Then
            Return buf.TryCast(Of Message)
        End If

        Dim newPool As New SpectrumPack(buf.TryCast(Of Stream))
        Dim annoData As Func(Of String, (name As String, formula As String)) = AddressOf metadb.GetAnnotation
        Dim xrefData As Func(Of String, Dictionary(Of String, String)) = AddressOf metadb.GetDbXref

        Call New Compression(annoData, xrefData, println:=Sub(s) base.print(s,, env)) _
            .SpectrumCompression(
                spectrumLib, newPool,
                nspec:=nspec,
                test:=test,
                xrefDb:=xrefDb
            )
        Call newPool.Dispose()

        Return True
    End Function

    ''' <summary>
    ''' do embedding of the spectrum data
    ''' </summary>
    ''' <param name="x">a set of the spectrum data, usually be a mzpack object</param>
    ''' <param name="mslevel">
    ''' only works when the input dataset is <see cref="mzPack"/>
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("embedding")>
    <RApiReturn(GetType(VectorModel))>
    Public Function embedding(<RRawVectorArgument>
                              x As Object,
                              Optional mslevel As Integer = 2,
                              Optional env As Environment = Nothing) As Object

        Dim spec As PeakMs2() = Nothing
        Dim pullSpec As pipeline = pipeline.TryCreatePipeline(Of PeakMs2)(x, env)

        If pullSpec.isError Then
            If TypeOf x Is mzPack Then
                env.AddMessage("create the ion peak embedding for one mzpack sample data.", MSG_TYPES.WRN)
                spec = DirectCast(x, mzPack).GetSpectrum(mslevel)
            Else
                pullSpec = pipeline.TryCreatePipeline(Of mzPack)(x, env)

                If pullSpec.isError Then
                    Return pullSpec.getError
                End If

                Dim spectrums As New SpecEmbedding

                For Each sample As mzPack In pullSpec.populates(Of mzPack)(env)
                    spec = sample.GetSpectrum(mslevel)
                    spectrums.AddSample(spec)
                Next

                Return spectrums.CreateEmbedding
            End If
        Else
            spec = pullSpec _
                .populates(Of PeakMs2)(env) _
                .ToArray
        End If

        Dim ions As New IonEmbedding()

        For Each ms As PeakMs2 In spec
            Call ions.Add(ms)
        Next

        Return ions.CreateEmbedding
    End Function

    <Extension>
    Private Function GetSpectrum(pool As mzPack, msLevel As Integer) As PeakMs2()
        If msLevel = 2 Then
            Return pool.GetMs2Peaks.ToArray
        Else
            Return pool.MS _
                .Select(Function(m1) New PeakMs2(m1.scan_id, m1.GetMs)) _
                .ToArray
        End If
    End Function
End Module
