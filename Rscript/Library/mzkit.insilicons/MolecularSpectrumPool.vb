﻿
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking.PoolData
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.Repository
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("spectrumPool")>
Public Module MolecularSpectrumPool

    Const unknown As String = NameOf(unknown)

    <ExportAPI("openPool")>
    Public Function openPool(dir As String, Optional level As Double = 0.8) As SpectrumPool
        Return SpectrumPool.OpenDirectory(dir, level, split:=6)
    End Function

    <ExportAPI("closePool")>
    Public Function closePool(pool As SpectrumPool) As Object
        Call pool.Dispose()
        Return Nothing
    End Function

    ''' <summary>
    ''' generates the guid for the spectrum with unknown annotation
    ''' </summary>
    ''' <param name="spectral"></param>
    ''' <returns></returns>
    <ExportAPI("conservedGuid")>
    Public Function conservedGuid(spectral As PeakMs2) As String
        Dim peaks As String() = spectral.mzInto _
            .OrderByDescending(Function(mzi) mzi.intensity) _
            .Take(6) _
            .Select(Function(m) m.mz.ToString("F1")) _
            .ToArray
        Dim mz1 As String = spectral.mz.ToString("F1")
        Dim meta As String() = {
            spectral.meta.TryGetValue("biosample", unknown),
            spectral.meta.TryGetValue("organism", unknown)
        }
        Dim hashcode As Integer = FNV1a.GetHashCode(peaks.JoinIterates(mz1).JoinIterates(meta))

        If hashcode < 0 Then
            hashcode = -hashcode + 7
        End If

        Return $"MSMS_{hashcode.ToString.FormatZero("00000000000")}"
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="pool"></param>
    ''' <param name="x">
    ''' the spectrum data collection
    ''' </param>
    ''' <param name="biosample"></param>
    ''' <param name="organism"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' the spectrum data for run clustering should be 
    ''' processed into centroid mode at first!
    ''' </remarks>
    <ExportAPI("addPool")>
    Public Function add(pool As SpectrumPool, <RRawVectorArgument> x As Object,
                        Optional biosample As String = "unknown",
                        Optional organism As String = "unkown",
                        Optional env As Environment = Nothing) As Object

        Dim data As pipeline = pipeline.TryCreatePipeline(Of PeakMs2)(x, env)

        If data.isError Then
            Return data.getError
        End If

        For Each peak As PeakMs2 In data.populates(Of PeakMs2)(env)
            peak.meta.Add("biosample", biosample)
            peak.meta.Add("organism", organism)
            peak.lib_guid = conservedGuid(peak)

            Call pool.Add(peak)
        Next

        Return Nothing
    End Function

    <ExportAPI("commit")>
    Public Function commit(pool As SpectrumPool) As Object
        Call pool.Commit()
        Return pool
    End Function
End Module