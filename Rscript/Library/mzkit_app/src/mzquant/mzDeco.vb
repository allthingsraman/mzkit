﻿#Region "Microsoft.VisualBasic::59a0f011b71b9720bce41f32807b7746, Rscript\Library\mzkit_app\src\mzquant\mzDeco.vb"

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

'   Total Lines: 1021
'    Code Lines: 680 (66.60%)
' Comment Lines: 219 (21.45%)
'    - Xml Docs: 84.02%
' 
'   Blank Lines: 122 (11.95%)
'     File Size: 42.02 KB


' Module mzDeco
' 
'     Function: adjust_to_seconds, dumpPeaks, extractAlignedPeaks, get_ionPeak, ms1Scans
'               mz_deco, mz_groups, peakAlignment, peaksetMatrix, peakSubset
'               peaktable, pull_xic, readPeakData, readPeaktable, readSamples
'               readXcmsFeaturePeaks, readXcmsPeaks, readXIC, RI_calc, RI_reference
'               writePeaktable, writeSamples, writeXIC, writeXIC1, xic_deco
'               xic_dtw_list, xic_matrix_list, XICpool_func
' 
'     Sub: Main
'     Class xic_deco_task
' 
'         Constructor: (+1 Overloads) Sub New
'         Sub: Solve
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.SingleCells.Deconvolute
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.genomics.Analysis.HTS.DataFrame
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports deco_math = BioNovoGene.Analytical.MassSpectrometry.Math.Extensions
Imports Matrix = SMRUCC.genomics.Analysis.HTS.DataFrame.Matrix
Imports std = System.Math
Imports vec = SMRUCC.Rsharp.Runtime.Internal.Object.vector

''' <summary>
''' Extract peak and signal data from rawdata
''' 
''' Data processing is the computational process of converting raw LC-MS 
''' data to biological knowledge and involves multiple processes including 
''' raw data deconvolution and the chemical identification of metabolites.
''' 
''' The process of data deconvolution, sometimes called peak picking, is 
''' in itself a complex process caused by the complexity of the data and 
''' variation introduced during the process of data acquisition related to 
''' mass-to-charge ratio, retention time and chromatographic peak area.
''' </summary>
<Package("mzDeco")>
<RTypeExport("peak_feature", GetType(PeakFeature))>
<RTypeExport("mz_group", GetType(MzGroup))>
<RTypeExport("peak_set", GetType(PeakSet))>
<RTypeExport("xcms2", GetType(xcms2))>
<RTypeExport("rt_shift", GetType(RtShift))>
<RTypeExport("RI_refer", GetType(RIRefer))>
Module mzDeco

    Sub Main()
        Call Internal.Object.Converts.addHandler(GetType(PeakFeature()), AddressOf peaktable)
        Call Internal.Object.Converts.addHandler(GetType(xcms2()), AddressOf peaksetMatrix)

        Call generic.add("readBin.mz_group", GetType(Stream), AddressOf readXIC)
        Call generic.add("readBin.peak_feature", GetType(Stream), AddressOf readSamples)
        Call generic.add("readBin.peak_set", GetType(Stream), AddressOf readPeaktable)

        Call generic.add("writeBin", GetType(MzGroup), AddressOf writeXIC1)
        Call generic.add("writeBin", GetType(MzGroup()), AddressOf writeXIC)
        Call generic.add("writeBin", GetType(PeakFeature()), AddressOf writeSamples)
        Call generic.add("writeBin", GetType(PeakSet), AddressOf writePeaktable)
    End Sub

    Private Function writePeaktable(table As PeakSet, args As list, env As Environment) As Object
        Dim con As Stream = args!con
        Call SaveXcms.DumpSample(table, con)
        Call con.Flush()
        Return True
    End Function

    Private Function readPeaktable(file As Stream, args As list, env As Environment) As Object
        Return SaveXcms.ReadSample(file)
    End Function

    Private Function writeSamples(samples As PeakFeature(), args As list, env As Environment) As Object
        Dim con As Stream = args!con
        Call SaveSample.DumpSample(samples, con)
        Call con.Flush()
        Return True
    End Function

    Private Function writeXIC1(xic As MzGroup, args As list, env As Environment) As Object
        Return writeXIC({xic}, args, env)
    End Function

    Private Function writeXIC(xic As MzGroup(), args As list, env As Environment) As Object
        Dim con As Stream = args!con
        Call SaveXIC.DumpSample(xic, con)
        Call con.Flush()
        Return True
    End Function

    Private Function readSamples(file As Stream, args As list, env As Environment) As Object
        Return SaveSample.ReadSample(file).ToArray
    End Function

    Private Function readXIC(file As Stream, args As list, env As Environment) As Object
        Return SaveXIC.ReadSample(file).ToArray
    End Function

    Private Function peaksetMatrix(peakset As xcms2(), args As list, env As Environment) As dataframe
        Dim table As New dataframe With {
           .columns = New Dictionary(Of String, Array)
        }
        Dim allsampleNames = peakset _
            .Select(Function(i) i.Properties.Keys) _
            .IteratesALL _
            .Distinct _
            .OrderBy(Function(a) a) _
            .ToArray

        table.rownames = peakset _
            .Select(Function(p) p.ID) _
            .ToArray

        table.add(NameOf(xcms2.mz), peakset.Select(Function(a) a.mz))
        table.add(NameOf(xcms2.mzmin), peakset.Select(Function(a) a.mzmin))
        table.add(NameOf(xcms2.mzmax), peakset.Select(Function(a) a.mzmax))
        table.add(NameOf(xcms2.rt), peakset.Select(Function(a) a.rt))
        table.add(NameOf(xcms2.rtmin), peakset.Select(Function(a) a.rtmin))
        table.add(NameOf(xcms2.rtmax), peakset.Select(Function(a) a.rtmax))
        table.add(NameOf(xcms2.npeaks), peakset.Select(Function(a) a.npeaks))

        For Each name As String In allsampleNames
            Call table.add(name, peakset.Select(Function(i) i(name)))
        Next

        Return table
    End Function

    Private Function peaktable(x As PeakFeature(), args As list, env As Environment) As dataframe
        Dim table As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        table.add(NameOf(PeakFeature.mz), x.Select(Function(a) a.mz))
        table.add(NameOf(PeakFeature.rt), x.Select(Function(a) a.rt))
        table.add(NameOf(PeakFeature.RI), x.Select(Function(a) a.RI))
        table.add(NameOf(PeakFeature.rtmin), x.Select(Function(a) a.rtmin))
        table.add(NameOf(PeakFeature.rtmax), x.Select(Function(a) a.rtmax))
        table.add(NameOf(PeakFeature.maxInto), x.Select(Function(a) a.maxInto))
        table.add(NameOf(PeakFeature.baseline), x.Select(Function(a) a.baseline))
        table.add(NameOf(PeakFeature.integration), x.Select(Function(a) a.integration))
        table.add(NameOf(PeakFeature.area), x.Select(Function(a) a.area))
        table.add(NameOf(PeakFeature.noise), x.Select(Function(a) a.noise))
        table.add(NameOf(PeakFeature.nticks), x.Select(Function(a) a.nticks))
        table.add(NameOf(PeakFeature.snRatio), x.Select(Function(a) a.snRatio))
        table.add(NameOf(PeakFeature.rawfile), x.Select(Function(a) a.rawfile))

        table.rownames = x.Select(Function(a) a.xcms_id).ToArray

        Return table
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="pool"></param>
    ''' <param name="features_mz"></param>
    ''' <param name="errors"></param>
    ''' <param name="rtRange"></param>
    ''' <param name="baseline"></param>
    ''' <param name="joint"></param>
    ''' <param name="dtw"></param>
    ''' <param name="parallel"></param>
    ''' <returns>a vector of <see cref="xcms2"/></returns>
    <Extension>
    Private Function xic_deco(pool As XICPool, features_mz As Double(),
                              errors As Tolerance,
                              rtRange As DoubleRange,
                              baseline As Double,
                              joint As Boolean,
                              dtw As Boolean,
                              parallel As Boolean) As Object

        VectorTask.n_threads = App.CPUCoreNumbers

        If features_mz.Length = 1 Then
            ' extract the aligned data
            Return xic_deco_task.extractAlignedPeaks(
                pool.DtwXIC(features_mz(0), errors).ToArray,
                rtRange:=rtRange,
                baseline:=baseline,
                joint:=joint, xic_align:=True, rt_shifts:=Nothing)
        Else
            Dim task As New xic_deco_task(pool, features_mz, errors, rtRange, baseline, joint, dtw)

            If parallel Then
                Call task.Run()
            Else
                Call task.Solve()
            End If

            Dim result = xcms2.MakeUniqueId(task.out).ToArray
            Dim vec As New vec(result, RType.GetRSharpType(GetType(xcms2)))
            Dim rt_diff As RtShift() = task.rt_shifts.ToArray

            Call vec.setAttribute("rt.shift", rt_diff)

            Return vec
        End If
    End Function

    ''' <summary>
    ''' read the peaktable file that in xcms2 output format
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns>A collection set of the <see cref="xcms2"/> peak features data object</returns>
    ''' <keywords>read data</keywords>
    <ExportAPI("read.xcms_peaks")>
    <RApiReturn(GetType(PeakSet))>
    Public Function readXcmsPeaks(file As String,
                                  Optional tsv As Boolean = False,
                                  Optional general_method As Boolean = False) As Object

        If file.ExtensionSuffix("dat", "xcms") Then
            ' read binary file
            Using buf As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return SaveXcms.ReadSample(buf)
            End Using
        End If

        If Not general_method Then
            Return SaveXcms.ReadTextTable(file, tsv)
        Else
            Return New PeakSet With {
                .peaks = file.LoadCsv(Of xcms2)(mute:=True).ToArray
            }
        End If
    End Function

    ''' <summary>
    ''' cast peaktable to expression matrix object
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("to_expression")>
    Public Function expression(x As PeakSet) As Matrix
        Dim sampleNames As String() = x.sampleNames
        Dim features As New List(Of DataFrameRow)

        For Each ion As xcms2 In x.peaks
            Call features.Add(New DataFrameRow With {
                .geneID = ion.ID,
                .experiments = ion(sampleNames)
            })
        Next

        Return New Matrix With {
            .sampleID = sampleNames,
            .tag = "peaktable",
            .expression = features.ToArray
        }
    End Function

    ''' <summary>
    ''' cast peaktable to mzkit expression matrix
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("to_matrix")>
    Public Function to_matrix(x As PeakSet) As MzMatrix
        Dim mz As Double() = x.peaks.Select(Function(a) a.mz).ToArray
        Dim mzmin As Double() = x.peaks.Select(Function(a) a.mzmin).ToArray
        Dim mzmax As Double() = x.peaks.Select(Function(a) a.mzmax).ToArray
        Dim samples As New List(Of PixelData)
        Dim table As xcms2() = x.peaks

        For Each name As String In x.sampleNames
            Call samples.Add(New PixelData With {
                .label = name,
                .intensity = table _
                    .Select(Function(a) a(name)) _
                    .ToArray
            })
        Next

        Return New MzMatrix With {
            .matrixType = FileApplicationClass.LCMS,
            .mz = mz,
            .mzmin = mzmin,
            .mzmax = mzmax,
            .tolerance = 0,
            .matrix = samples.ToArray
        }
    End Function

    ''' <summary>
    ''' save mzkit peaktable object to csv table file
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="file">the file path to the target csv table file</param>
    ''' <returns></returns>
    <ExportAPI("write.xcms_peaks")>
    Public Function writeXcmsPeaktable(x As PeakSet, file As String) As Boolean
        Return x.peaks.SaveTo(file, silent:=True)
    End Function

    ''' <summary>
    ''' cast dataset to mzkit peaktable object
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.peak_set")>
    Public Function create_peakset(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim pull = pipeline.TryCreatePipeline(Of xcms2)(x, env)
        Dim peaks As New List(Of xcms2)

        If pull.isError Then
            ' deal with dataframe?
            If TypeOf x Is dataframe Then
                Dim df As dataframe = x
                Dim mz As Double() = CLRVector.asNumeric(df!mz)
                Dim mzmin As Double() = CLRVector.asNumeric(df!mzmin)
                Dim mzmax As Double() = CLRVector.asNumeric(df!mzmax)
                Dim rt As Double() = CLRVector.asNumeric(df!rt)
                Dim rtmin As Double() = CLRVector.asNumeric(df!rtmin)
                Dim rtmax As Double() = CLRVector.asNumeric(df!rtmax)
                Dim RI As Double() = CLRVector.asNumeric(df!RI)
                Dim ID As String() = df.getRowNames.UniqueNames

                Call df.delete("ID", "mz", "mzmin", "mzmax", "rt", "rtmin", "rtmax", "RI", "npeaks")

                Dim offset As Integer
                Dim v As Dictionary(Of String, Double)
                Dim matrix As NamedCollection(Of Double)() = df.columns _
                    .Select(Function(i)
                                Return New NamedCollection(Of Double)(i.Key, CLRVector.asNumeric(i.Value))
                            End Function) _
                    .ToArray

                For i As Integer = 0 To mz.Length - 1
                    offset = i
                    v = matrix.ToDictionary(Function(a) a.name, Function(a) a(offset))

                    Call peaks.Add(New xcms2(v) With {
                        .ID = ID(i),
                        .mz = mz(i),
                        .mzmax = mzmax(i),
                        .mzmin = mzmin(i),
                        .RI = RI(i),
                        .rt = rt(i),
                        .rtmax = rtmax(i),
                        .rtmin = rtmin(i)
                    })
                Next
            Else
                Return pull.getError
            End If
        End If

        Return New PeakSet(peaks)
    End Function

    ''' <summary>
    ''' Try to cast the dataframe to the mzkit peak feature object set
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    ''' <keywords>read data</keywords>
    <ExportAPI("read.xcms_features")>
    <RApiReturn(GetType(PeakFeature))>
    Public Function readXcmsFeaturePeaks(file As dataframe) As Object
        Dim mz As Double() = CLRVector.asNumeric(file.getVector("mz", True))
        Dim mzmin As Double() = CLRVector.asNumeric(file.getVector("mzmin", True))
        Dim mzmax As Double() = CLRVector.asNumeric(file.getVector("mzmax", True))
        Dim rt As Double() = CLRVector.asNumeric(file.getVector("rt", True))
        Dim rtmin As Double() = CLRVector.asNumeric(file.getVector("rtmin", True))
        Dim rtmax As Double() = CLRVector.asNumeric(file.getVector("rtmax", True))
        Dim into As Double() = CLRVector.asNumeric(file.getVector("into", True))
        Dim maxo As Double() = CLRVector.asNumeric(file.getVector("maxo", True))
        Dim sn As Double() = CLRVector.asNumeric(file.getVector("sn", True))
        Dim xcms_id As String() = mz.Select(Function(mzi, i) $"M{CInt(mzi)}T{CInt(rt(i))}").UniqueNames

        Return xcms_id _
            .Select(Function(id, i)
                        Return New PeakFeature With {
                            .xcms_id = xcms_id(i),
                            .rt = rt(i),
                            .area = into(i),
                            .baseline = 0,
                            .integration = 1,
                            .maxInto = maxo(i),
                            .mz = mz(i),
                            .noise = 0,
                            .nticks = 0,
                            .rawfile = Nothing,
                            .RI = 0,
                            .rtmax = rtmax(i),
                            .rtmin = rtmin(i)
                        }
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' make sample column projection
    ''' </summary>
    ''' <param name="peaktable">A xcms liked peaktable object, is a collection 
    ''' of the <see cref="xcms2"/> peak feature data.</param>
    ''' <param name="sampleNames">A character vector of the sample names for make 
    ''' the peaktable projection.</param>
    ''' <returns>A sub-table of the input original peaktable data</returns>
    <ExportAPI("peak_subset")>
    <RApiReturn(GetType(PeakSet))>
    Public Function peakSubset(peaktable As PeakSet, sampleNames As String()) As Object
        Return peaktable.Subset(sampleNames)
    End Function

    ''' <summary>
    ''' helper function for find ms1 peaks based on the given mz/rt tuple data
    ''' </summary>
    ''' <param name="peaktable">the peaktable object, is a collection of the <see cref="xcms2"/> object.</param>
    ''' <param name="mz">target ion m/z</param>
    ''' <param name="rt">target ion rt in seconds.</param>
    ''' <param name="mzdiff">the mass tolerance error in data unit delta dalton, 
    ''' apply for matches between the peaktable precursor m/z and the given ion mz value.</param>
    ''' <param name="rt_win">the rt window size for matches the rt. should be in data unit seconds.</param>
    ''' <returns>data is re-ordered via the tolerance error</returns>
    <ExportAPI("find_xcms_ionPeaks")>
    <RApiReturn(GetType(xcms2))>
    Public Function get_ionPeak(peaktable As PeakSet, mz As Double, rt As Double,
                                Optional mzdiff As Double = 0.01,
                                Optional rt_win As Double = 90) As Object

        Return peaktable.FindIonSet(mz, rt, mzdiff, rt_win) _
            .OrderBy(Function(a)
                         Return std.Abs(a.mz - mz + 0.0001) *
                             std.Abs(a.rt - rt + 0.0001)
                     End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' adjust the reteintion time data to unit seconds
    ''' </summary>
    ''' <param name="rt_data"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("adjust_to_seconds")>
    Public Function adjust_to_seconds(<RRawVectorArgument> rt_data As Object, Optional env As Environment = Nothing) As Object
        Dim pull_data As pipeline = pipeline.TryCreatePipeline(Of IRetentionTime)(rt_data, env)

        If pull_data.isError Then
            Return pull_data.getError
        End If

        Dim ds As New List(Of IRetentionTime)

        For Each xi As IRetentionTime In pull_data.populates(Of IRetentionTime)(env)
            xi.rt *= 60
            ds.Add(xi)
        Next

        Return ds.ToArray
    End Function

    ''' <summary>
    ''' Create RI reference dataset.
    ''' </summary>
    ''' <returns>a collection of the mzkit ri reference object model 
    ''' which is matched via the xcms peaktable.</returns>
    <ExportAPI("RI_reference")>
    <RApiReturn(GetType(RIRefer))>
    Public Function RI_reference(xcms_id As String(),
                                 mz As Double(),
                                 rt As Double(),
                                 ri As Double()) As Object
        Return xcms_id _
            .Select(Function(id, i)
                        Return New RIRefer() With {
                            .name = id,
                            .mz = mz(i),
                            .rt = rt(i),
                            .RI = ri(i)
                        }
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' RI calculation of a speicifc sample data
    ''' </summary>
    ''' <param name="peakdata">should be a collection of the peak data from a single sample file.</param>
    ''' <param name="RI">should be a collection of the <see cref="RIRefer"/> data.</param>
    ''' <param name="C">
    ''' the number of carbon atoms for kovats retention index
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("RI_cal")>
    Public Function RI_calc(peakdata As PeakFeature(),
                            <RRawVectorArgument>
                            Optional RI As Object = Nothing,
                            Optional ppm As Double = 20,
                            Optional dt As Double = 15,
                            Optional rawfile As String = Nothing,
                            Optional by_id As Boolean = False,
                            Optional C As list = Nothing,
                            Optional env As Environment = Nothing) As Object

        Dim refer_points As New List(Of PeakFeature)

        If RI Is Nothing Then
            ' ri reference from the peakdata which has RI value assigned
            Call refer_points.AddRange(From pk As PeakFeature In peakdata.SafeQuery Where pk.RI > 0 Order By pk.RI)
        Else
            Dim RIrefers As pipeline = pipeline.TryCreatePipeline(Of RIRefer)(RI, env)

            If RIrefers.isError Then
                Return RIrefers.getError
            End If

            Dim ri_refers As RIRefer() = RIrefers.populates(Of RIRefer)(env).OrderBy(Function(i) i.rt).ToArray
            Dim ppmErr As Tolerance = Tolerance.PPM(ppm)

            'For i As Integer = 0 To ri_refers.Length - 1
            '    ri_refers(i).rt *= 60
            'Next

            If by_id Then
                ' the RI is already has been assigned the peak id
                ' get peak feature data by its id directly!
                Dim peak1Index = peakdata.ToDictionary(Function(p1) p1.xcms_id)

                For Each refer As RIRefer In ri_refers
                    Dim target As PeakFeature = peak1Index(refer.name)

                    target.RI = refer.RI
                    refer_points.Add(target)
                Next
            Else
                ' find a ri reference point at first
                ' find a set of the candidate points
                For Each refer As RIRefer In ri_refers
                    Dim target As PeakFeature = peakdata _
                        .Where(Function(pi) ppmErr(pi.mz, refer.mz) AndAlso std.Abs(pi.rt - refer.rt) <= dt) _
                        .OrderByDescending(Function(pi) pi.maxInto) _
                        .FirstOrDefault

                    If target Is Nothing Then
                        Return Internal.debug.stop({
                            $"the required retention index reference point({refer.ToString}) could not be found! please check the rt window parameter(dt) is too small?",
                            $"retention_index_reference: {ri_refers.GetJson}",
                            $"rawfile tag: {rawfile}",
                            $"ms1_pars: {ppm} PPM, rt_win {dt} sec"
                        }, env)
                    End If

                    target.RI = refer.RI
                    refer_points.Add(target)
                Next
            End If
        End If

        ' order raw data by rt
        peakdata = peakdata.OrderBy(Function(i) i.rt).ToArray
        refer_points = refer_points.OrderBy(Function(i) i.rt).AsList
        ' add a fake point
        refer_points.Add(New PeakFeature With {
            .RI = refer_points.Last.RI + 100,
            .rt = peakdata.Last.rt,
            .xcms_id = peakdata.Last.xcms_id
        })

        Dim a As (rt As Double, ri As Double)
        Dim b As (rt As Double, ri As Double)
        Dim offset As Integer = 0
        Dim id_a, id_b As String
        Dim c_atoms As Dictionary(Of String, Integer) = Nothing

        If Not C Is Nothing Then
            c_atoms = C.AsGeneric(Of Integer)(env)

            If Not c_atoms.ContainsKey(peakdata(0).xcms_id) Then
                c_atoms.Add(peakdata(0).xcms_id, 0)
            End If
            If Not c_atoms.ContainsKey(peakdata.Last.xcms_id) Then
                c_atoms.Add(peakdata.Last.xcms_id, c_atoms.Values.Max + 1)
            End If
        End If

        If peakdata(0).RI > 0 Then
            a = (peakdata(0).rt, peakdata(0).RI)
            id_a = peakdata(0).xcms_id
            offset = 1
            b = (refer_points(1).rt, refer_points(1).RI)
            id_b = refer_points(1).xcms_id
        Else
            a = (peakdata(0).rt, 0)
            b = (refer_points(0).rt, refer_points(0).RI)
            id_a = peakdata(0).xcms_id
            id_b = refer_points(0).xcms_id
        End If

        For i As Integer = offset To peakdata.Length - 1
            peakdata(i).rawfile = If(rawfile, peakdata(i).rawfile)

            If peakdata(i).RI = 0 Then
                If c_atoms Is Nothing Then
                    peakdata(i).RI = deco_math.RetentionIndex(peakdata(i), a, b)
                Else
                    peakdata(i).RI = deco_math.KovatsRI(c_atoms(id_a), c_atoms(id_b), peakdata(i).rt, a.rt, b.rt)
                End If
            Else
                a = b
                id_a = id_b
                offset += 1
                b = (refer_points(offset).rt, refer_points(offset).RI)
                id_b = refer_points(offset).xcms_id
            End If
        Next

        ' and then evaluate the ri for each peak points
        Return peakdata
    End Function

    ''' <summary>
    ''' Chromatogram data deconvolution
    ''' </summary>
    ''' <param name="ms1">
    ''' a collection of the ms1 data or the mzpack raw data object, this parameter could also be
    ''' a XIC pool object which contains a collection of the ion XIC data for run deconvolution.
    ''' </param>
    ''' <param name="tolerance">the mass tolerance for extract the XIC data for run deconvolution.</param>
    ''' <param name="feature">
    ''' a numeric vector of target feature ion m/z value for extract the XIC data.
    ''' </param>
    ''' <param name="parallel">
    ''' run peak detection algorithm on mutliple xic data in parallel mode?
    ''' </param>
    ''' <returns>a vector of the peak deconvolution data,
    ''' in format of xcms peak table liked or mzkit <see cref="PeakFeature"/>
    ''' data object.
    ''' 
    ''' the result data vector may contains the rt shift data result, where you can get this shift
    ''' value via the ``rt.shift`` attribute name, rt shift data model is clr type: <see cref="RtShift"/>.
    ''' </returns>
    ''' <example>
    ''' require(mzkit);
    ''' 
    ''' imports "mzDeco" from "mz_quantify";
    ''' 
    ''' let rawdata = open.mzpack("/path/to/rawdata.mzXML");
    ''' let ms1 = rawdata |> ms1_scans();
    ''' let peaks = mz_deco(ms1, tolerance = "da:0.01", peak.width = [3,30], 
    '''    dtw = TRUE);
    ''' 
    ''' write.peaks(peaks, file = "/data/save_debug.dat");
    ''' </example>
    <ExportAPI("mz_deco")>
    <RApiReturn(GetType(PeakFeature), GetType(xcms2))>
    Public Function mz_deco(<RRawVectorArgument>
                            ms1 As Object,
                            Optional tolerance As Object = "ppm:20",
                            Optional baseline# = 0.65,
                            <RRawVectorArgument>
                            Optional peak_width As Object = "3,15",
                            Optional joint As Boolean = False,
                            Optional parallel As Boolean = False,
                            Optional dtw As Boolean = False,
                            <RRawVectorArgument>
                            Optional feature As Object = Nothing,
                            Optional rawfile As String = Nothing,
                            Optional sn_threshold As Double = 1,
                            Optional env As Environment = Nothing) As Object

        Dim errors As [Variant](Of Tolerance, Message) = Math.getTolerance(tolerance, env)
        Dim rtRange = ApiArgumentHelpers.GetDoubleRange(peak_width, env, [default]:="3,15")

        If errors Like GetType(Message) Then
            Return errors.TryCast(Of Message)
        ElseIf rtRange Like GetType(Message) Then
            Return rtRange.TryCast(Of Message)
        End If

        ' 1. processing for XIC pool
        If TypeOf ms1 Is XICPool Then
            Dim pool As XICPool = DirectCast(ms1, XICPool)
            Dim features_mz As Double() = CLRVector.asNumeric(feature)

            If features_mz.IsNullOrEmpty Then
                Return Internal.debug.stop("no ion m/z feature was provided!", env)
            Else
                Return pool.xic_deco(features_mz,
                                     errors.TryCast(Of Tolerance),
                                     rtRange.TryCast(Of DoubleRange),
                                     baseline, joint, dtw, parallel)
            End If
        ElseIf TypeOf ms1 Is list Then
            ' 2. processing for a set of the xic data
            Dim ls_xic = DirectCast(ms1, list) _
                .AsGeneric(Of MzGroup)(env) _
                .Select(Function(a) New NamedValue(Of MzGroup)(a.Key, a.Value)) _
                .ToArray

            If Not ls_xic.All(Function(a) a.Value Is Nothing) Then
                If dtw Then
                    ls_xic = XICPool.DtwXIC(rawdata:=ls_xic).ToArray
                End If

                Return xic_deco_task.extractAlignedPeaks(
                    ls_xic,
                    rtRange:=rtRange.TryCast(Of DoubleRange),
                    baseline:=baseline,
                    joint:=joint, xic_align:=True, rt_shifts:=Nothing)
            Else
                GoTo extract_ms1
            End If
        ElseIf TypeOf ms1 Is ChromatogramOverlapList Then
            Return DirectCast(ms1, ChromatogramOverlapList) _
                .GetPeakGroups(rtRange.TryCast(Of DoubleRange), quantile:=baseline, sn_threshold, joint, [single]:=False) _
                .ToArray
        Else
            Dim pull_xic As pipeline = pipeline.TryCreatePipeline(Of MzGroup)(ms1, env, suppress:=True)

            If Not pull_xic.isError Then
                Return pull_xic _
                    .populates(Of MzGroup)(env) _
                    .DecoMzGroups(
                        peakwidth:=rtRange.TryCast(Of DoubleRange),
                        quantile:=baseline,
                        parallel:=parallel,
                        joint:=joint,
                        source:=rawfile,
                        sn:=sn_threshold
                    ) _
                    .ToArray
            End If

extract_ms1:
            Dim source As String = Nothing
            Dim ms1_scans As IEnumerable(Of IMs1Scan) = ms1Scans(ms1, source)

            source = If(rawfile, source)

            If Not source.StringEmpty Then
                Call VBDebugger.EchoLine($"run peak feature finding for rawdata: {source}.")
            End If

            ' usually used for make extract features
            ' for a single sample file
            Return ms1_scans _
                .GetMzGroups(mzdiff:=errors) _
                .DecoMzGroups(
                    peakwidth:=rtRange.TryCast(Of DoubleRange),
                    quantile:=baseline,
                    parallel:=parallel,
                    joint:=joint,
                    source:=source,
                    sn:=sn_threshold
                ) _
                .ToArray
        End If
    End Function

    <ExportAPI("read.rt_shifts")>
    Public Function read_rtshifts(file As String) As RtShift()
        Return file.LoadCsv(Of RtShift)(mute:=True).ToArray
    End Function

    ''' <summary>
    ''' write peak debug data
    ''' </summary>
    ''' <param name="peaks"></param>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <keywords>save data</keywords>
    <ExportAPI("write.peaks")>
    Public Function dumpPeaks(<RRawVectorArgument> peaks As Object, file As Object, Optional env As Environment = Nothing) As Object
        Dim peakSet = pipeline.TryCreatePipeline(Of PeakFeature)(peaks, env)
        Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)

        If peakSet.isError Then
            Return peakSet.getError
        ElseIf buf Like GetType(Message) Then
            Return buf.TryCast(Of Message)
        End If

        Call SaveSample.DumpSample(peakSet.populates(Of PeakFeature)(env), buf.TryCast(Of Stream))

        If TypeOf file Is String Then
            Call buf.TryCast(Of Stream).Dispose()
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' read the peak feature table data
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="readBin">
    ''' does the given data file is in binary format not a csv table file, 
    ''' and this function should be parsed as a binary data file?
    ''' </param>
    ''' <returns></returns>
    ''' <keywords>read data</keywords>
    <ExportAPI("read.peakFeatures")>
    <RApiReturn(GetType(PeakFeature))>
    Public Function readPeakData(file As String, Optional readBin As Boolean = False) As Object
        If readBin Then
            Using buf As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return SaveSample.ReadSample(buf).ToArray
            End Using
        Else
            Return file.LoadCsv(Of PeakFeature)(mute:=True).ToArray
        End If
    End Function

    ''' <summary>
    ''' Do COW peak alignment and export peaktable
    ''' 
    ''' Correlation optimized warping (COW) based on the total ion 
    ''' current (TIC) is a widely used time alignment algorithm 
    ''' (COW-TIC). This approach works successfully on chromatograms 
    ''' containing few compounds and having a well-defined TIC.
    ''' </summary>
    ''' <param name="samples">should be a set of sample file data, which could be extract from the ``mz_deco`` function.</param>
    ''' <param name="mzdiff"></param>
    ''' <param name="norm">do total ion sum normalization after peak alignment and the peaktable object has been exported?</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <example>
    ''' let peaksdata = lapply(files, function(ms1) {
    '''     mz_deco(ms1, tolerance = "da:0.01", peak.width = [3,30]);
    ''' });
    ''' let peaktable = peak_alignment(samples = peaksdata);
    ''' 
    ''' write.csv(peaktable, 
    '''     file = "/path/to/peaktable.csv", 
    '''     row.names = TRUE);
    ''' </example>
    <ExportAPI("peak_alignment")>
    <RApiReturn(GetType(xcms2))>
    Public Function peakAlignment(<RRawVectorArgument>
                                  samples As Object,
                                  Optional mzdiff As Object = "da:0.001",
                                  Optional norm As Boolean = False,
                                  Optional ri_alignment As Boolean = False,
                                  Optional env As Environment = Nothing) As Object

        Dim mzErr = Math.getTolerance(mzdiff, env, [default]:="da:0.001")
        Dim sampleData As NamedCollection(Of PeakFeature)() = Nothing

        If mzErr Like GetType(Message) Then
            Return mzErr.TryCast(Of Message)
        End If

        If TypeOf samples Is list Then
            Dim ls = DirectCast(samples, list).AsGeneric(Of PeakFeature())(env)

            If ls.All(Function(a) a.Value Is Nothing) Then
            Else
                sampleData = ls _
                    .Select(Function(a) New NamedCollection(Of PeakFeature)(a.Key, a.Value)) _
                    .ToArray
            End If
        End If

        If sampleData.IsNullOrEmpty Then
            Dim samplePeaks = pipeline.TryCreatePipeline(Of PeakFeature)(samples, env)

            If samplePeaks.isError Then
                Return samplePeaks.getError
            End If

            sampleData = samplePeaks _
                .populates(Of PeakFeature)(env) _
                .GroupBy(Function(a) a.rawfile) _
                .Select(Function(i)
                            Return New NamedCollection(Of PeakFeature)(i.Key, i.ToArray)
                        End Function) _
                .ToArray
        End If

        Dim peaktable As xcms2()
        Dim rt_shifts As New List(Of RtShift)

        If ri_alignment Then
            peaktable = sampleData _
                .RIAlignment(rt_shifts) _
                .ToArray
        Else
            peaktable = sampleData _
                .CowAlignment() _
                .ToArray
        End If

        Dim id As String() = peaktable.Select(Function(i) i.ID).UniqueNames
        Dim sampleNames As String() = sampleData.Keys.ToArray

        For i As Integer = 0 To id.Length - 1
            Dim peak As xcms2 = peaktable(i)

            peak.ID = id(i)

            For Each sample_id As String In sampleNames
                If Not peak.Properties.ContainsKey(sample_id) Then
                    peak(sample_id) = 0.0
                End If
            Next
        Next

        If norm Then
            For Each name As String In sampleNames
                Dim v = peaktable.Select(Function(i) i(name)).AsVector
                v = v / v.Sum * (10 ^ 8)

                For i As Integer = 0 To peaktable.Length - 1
                    peaktable(i)(name) = v(i)
                Next
            Next
        End If

        Dim vec As New vec(peaktable, RType.GetRSharpType(GetType(xcms2)))
        Call vec.setAttribute("rt.shift", rt_shifts.ToArray)
        Return vec
    End Function

    ''' <summary>
    ''' do ``m/z`` grouping under the given tolerance
    ''' </summary>
    ''' <param name="ms1">
    ''' a LCMS mzpack rawdata object or a collection of the ms1 point data
    ''' </param>
    ''' <param name="mzdiff">the mass tolerance error for extract the XIC from the rawdata set</param>
    ''' <param name="rtwin">the rt tolerance window size for merge data points</param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' create a list of XIC dataset for run downstream deconv operation
    ''' </returns>
    ''' <example>
    ''' let rawdata = open.mzpack(file = "/path/to/rawdata.mzpack");
    ''' let xic = mz.groups(ms1 = rawdata, mzdiff = "ppm:20");
    ''' 
    ''' # export the XIC data as binary data file.
    ''' writeBin(xic, con = "/path/to/xic_data.dat");
    ''' </example>
    ''' <remarks>
    ''' the ion mz value is generated via the max intensity point in each ion 
    ''' feature group, and the xic data has already been re-order via the 
    ''' time asc.
    ''' </remarks>
    <ExportAPI("mz.groups")>
    <RApiReturn(GetType(MzGroup))>
    Public Function mz_groups(<RRawVectorArgument>
                              ms1 As Object,
                              Optional mzdiff As Object = "ppm:20",
                              Optional rtwin As Double = 0.05,
                              Optional env As Environment = Nothing) As Object

        Return ms1Scans(ms1) _
            .GetMzGroups(mzdiff:=Math.getTolerance(mzdiff, env), rtwin:=rtwin) _
            .ToArray
    End Function

    Private Function ms1Scans(ms1 As Object, Optional ByRef source As String = Nothing) As IEnumerable(Of IMs1Scan)
        If ms1 Is Nothing Then
            Return {}
        ElseIf ms1.GetType Is GetType(ms1_scan()) Then
            Return DirectCast(ms1, ms1_scan()).Select(Function(t) DirectCast(t, IMs1Scan))
        ElseIf TypeOf ms1 Is mzPack Then
            source = DirectCast(ms1, mzPack).source
            Return DirectCast(ms1, mzPack) _
                .GetAllScanMs1 _
                .Select(Function(t) DirectCast(t, IMs1Scan))
        Else
            Throw New NotImplementedException
        End If
    End Function

    ''' <summary>
    ''' Load xic sample data files
    ''' </summary>
    ''' <param name="files">a character vector of a collection of the xic data files.</param>
    ''' <returns></returns>
    <ExportAPI("xic_pool")>
    <RApiReturn(GetType(XICPool))>
    Public Function XICpool_func(files As String()) As Object
        Dim pool As New XICPool
        Dim group As MzGroup()

        For Each file As String In files
            Using s As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                group = SaveXIC.ReadSample(s).ToArray
                pool.Add(file.BaseName, group)
            End Using
        Next

        Return pool
    End Function

    ''' <summary>
    ''' extract a collection of xic data for a specific ion feature
    ''' 
    ''' this function is debug used only
    ''' </summary>
    ''' <param name="pool">should be type of <see cref="XICPool"/> or peak collection <see cref="PeakSet"/> object.</param>
    ''' <param name="mz">the ion feature m/z value</param>
    ''' <param name="dtw">this parameter will not working when the data pool type is clr type <see cref="PeakSet"/></param>
    ''' <param name="mzdiff"></param>
    ''' <returns>
    ''' a tuple list object that contains the xic data across
    ''' multiple sample data files for a speicifc ion feature
    ''' m/z.
    ''' </returns>
    ''' <example>
    ''' require(mzkit);
    '''
    ''' imports "mzDeco" from "mz_quantify";
    ''' imports "visual" from "mzplot";
    ''' 
    ''' let files = list.files("/path/to/debug_data_dir/", pattern = "*.xic");
    ''' let pool = xic_pool(files);
    ''' let dtw_xic = pull_xic(pool, mz = 100.0011, dtw = TRUE);
    ''' 
    ''' bitmap(file = "/path/to/save_image.png") {
    '''     raw_snapshot3D(dtw_xic);
    ''' }
    '''
    ''' dtw_xic
    ''' |> mz_deco(joint = TRUE, peak.width = [3,60])
    ''' |> write.csv(file = "/path/to/export_peakstable.csv")
    ''' ;
    ''' </example>
    <ExportAPI("pull_xic")>
    Public Function pull_xic(pool As Object, mz As Double,
                             Optional dtw As Boolean = True,
                             Optional mzdiff As Double = 0.01,
                             Optional strict As Boolean = False,
                             Optional env As Environment = Nothing) As Object
        If pool Is Nothing Then
            Return Message.NullOrStrict(strict, NameOf(pool), env)
        End If

        If TypeOf pool Is XICPool Then
            If dtw Then
                Return DirectCast(pool, XICPool).xic_dtw_list(mz, mzdiff)
            Else
                Return DirectCast(pool, XICPool).xic_matrix_list(mz, mzdiff)
            End If
        ElseIf TypeOf pool Is PeakSet Then
            Return DirectCast(pool, PeakSet) _
                .FilterMz(mz, mzdiff) _
                .OrderBy(Function(i) i.rt) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(XICPool), pool.GetType, env)
        End If
    End Function

    <Extension>
    Private Function xic_dtw_list(pool As XICPool, mz As Double, mzdiff As Double) As list
        Return New list With {
            .slots = pool _
                .DtwXIC(mz, Tolerance.DeltaMass(mzdiff)) _
                .ToDictionary(Function(a) a.Name,
                              Function(a)
                                  Return CObj(a.Value)
                              End Function)
        }
    End Function

    <Extension>
    Private Function xic_matrix_list(pool As XICPool, mz As Double, mzdiff As Double) As list
        Return New list With {
            .slots = pool _
                .GetXICMatrix(mz, Tolerance.DeltaMass(mzdiff)) _
                .ToDictionary(Function(a) a.Name,
                                Function(a)
                                    Return CObj(a.Value)
                                End Function)
        }
    End Function
End Module
