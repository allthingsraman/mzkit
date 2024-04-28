﻿#Region "Microsoft.VisualBasic::ce5c391ae4cafdb004a0a24f6226d5bc, E:/mzkit/src/mzmath/TargetedMetabolomics//LinearQuantitative/LinearPack/CDFWriter.vb"

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

    '   Total Lines: 197
    '    Code Lines: 163
    ' Comment Lines: 4
    '   Blank Lines: 30
    '     File Size: 9.60 KB


    '     Module CDFWriter
    ' 
    '         Function: Write, writeSampleNames
    ' 
    '         Sub: peakLinearNames, Write, writeGlobals, writeIS, writeLinear
    '              writeLinears, writePeak, writePeakNames, writePeakSamples, writeSampleLevel
    '              writeSampleLevels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Content
Imports BioNovoGene.Analytical.MassSpectrometry.Math.LinearQuantitative.Linear
Imports Microsoft.VisualBasic.DataStorage
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearQuantitative.Data

    Module CDFWriter

        <Extension>
        Public Function Write(pack As LinearPack, file As Stream) As Boolean
            Using cdffile As New netCDF.CDFWriter(file)
                Call pack.Write(cdffile)
            End Using

            Return True
        End Function

        <Extension>
        Private Sub Write(pack As LinearPack, file As netCDF.CDFWriter)
            Call pack.writeGlobals(file)
            Call pack.peakLinearNames(file)
            Call pack.writeLinears(file)
            Call pack.writePeakNames(file)
            Call pack.writePeakSamples(file)
            Call pack.writeSampleLevels(file)
            Call pack.writeIS(file)
        End Sub

        <Extension>
        Private Sub writeIS(pack As LinearPack, file As netCDF.CDFWriter)
            Dim bstr As chars = pack.IS.ToBEncodeString
            Dim strLen As New Dimension With {.name = "bstrLen", .size = bstr.Length}

            file.AddVariable("IS", bstr, strLen)
        End Sub

        <Extension>
        Private Sub writeSampleLevels(pack As LinearPack, file As netCDF.CDFWriter)
            Dim allSampleNames As String() = pack.writeSampleNames(file)
            Dim size As New Dimension With {.name = "samplePoints", .size = allSampleNames.Length}

            For Each level In pack.reference
                Call level.Value.writeSampleLevel(level.Key, allSampleNames, size, file)
            Next
        End Sub

        <Extension>
        Private Sub writeSampleLevel(levels As SampleContentLevels,
                                     ionName As String,
                                     allSampleNames As String(),
                                     size As Dimension,
                                     file As netCDF.CDFWriter)

            Dim data As doubles = allSampleNames.Select(Function(lv) levels(lv)).ToArray
            Dim attrs As attribute() = {
                New attribute With {.name = "directMap", .type = CDFDataTypes.BOOLEAN, .value = levels.directMap}
            }

            file.AddVariable($"levels\{ionName}", data, size, attrs)
        End Sub

        <Extension>
        Private Function writeSampleNames(pack As LinearPack, file As netCDF.CDFWriter) As String()
            Dim allSampleNames As String() = pack.peakSamples _
                .SafeQuery _
                .Select(Function(p) p.SampleName) _
                .Distinct _
                .ToArray

            If allSampleNames.IsNullOrEmpty Then
                ' the peak samples in the linearpack object is empty
                ' if this pack data object is created from a excel table
                '
                ' so extract the names from the reference names
                allSampleNames = pack.reference.Values _
                    .Select(Function(a) a.levels.Keys) _
                    .IteratesALL _
                    .OrderBy(Function(fname) fname) _
                    .Distinct _
                    .ToArray
            End If

            Dim data As chars = allSampleNames.GetJson
            Dim size As New Dimension With {.name = "sizeofSamples", .size = data.Length}
            Dim attrs As attribute() = {
                New attribute With {.name = "size", .type = CDFDataTypes.NC_INT, .value = allSampleNames.Length}
            }

            file.AddVariable("sampleNames", data, size, attrs)

            Return allSampleNames
        End Function

        <Extension>
        Private Sub writePeakSamples(pack As LinearPack, file As netCDF.CDFWriter)
            For Each peak As TargetPeakPoint In pack.peakSamples.SafeQuery
                Call peak.writePeak(file)
            Next
        End Sub

        <Extension>
        Private Sub writePeak(peak As TargetPeakPoint, file As netCDF.CDFWriter)
            Dim time As Double() = peak.Peak.ticks.Select(Function(t) t.Time).ToArray
            Dim into As Double() = peak.Peak.ticks.Select(Function(t) t.Intensity).ToArray
            Dim data As doubles = time.JoinIterates(into).ToArray
            Dim size As New Dimension With {.name = $"sizeof_{peak.SampleName}\{peak.Name}", .size = data.Length}
            Dim attrs As attribute() = {
                New attribute With {.name = "name", .type = CDFDataTypes.NC_CHAR, .value = peak.Name},
                New attribute With {.name = "sample_name", .type = CDFDataTypes.NC_CHAR, .value = peak.SampleName},
                New attribute With {
                    .name = "summary",
                    .type = CDFDataTypes.NC_CHAR,
                    .value = peak.ChromatogramSummary _
                        .Select(Function(q)
                                    Return $"{q.Percentage}:{q.Quantile}"
                                End Function) _
                        .JoinBy("|")
                },
                New attribute With {.name = "rtmin", .type = CDFDataTypes.NC_CHAR, .value = peak.Peak.window.Min},
                New attribute With {.name = "rtmax", .type = CDFDataTypes.NC_CHAR, .value = peak.Peak.window.Max},
                New attribute With {.name = "maxinto", .type = CDFDataTypes.NC_CHAR, .value = peak.Peak.peakHeight},
                New attribute With {.name = "base", .type = CDFDataTypes.NC_CHAR, .value = peak.Peak.base}
            }

            Call file.AddVariable($"{peak.SampleName}\{peak.Name}", data, size, attrs)
        End Sub

        <Extension>
        Private Sub writePeakNames(pack As LinearPack, file As netCDF.CDFWriter)
            Dim data As chars = pack.peakSamples.SafeQuery.Select(Function(p) $"{p.SampleName}\{p.Name}").GetJson
            Dim size As New Dimension With {.name = "sizeofPeaks", .size = If(pack.peakSamples.IsNullOrEmpty, 0, pack.peakSamples.Length)}
            Dim attrs As attribute() = {
                New attribute With {.name = "peaks", .type = CDFDataTypes.NC_INT, .value = If(pack.linears.IsNullOrEmpty, 0, pack.linears.Length)}
            }

            file.AddVariable("peaks", data, size, attrs)
        End Sub

        <Extension>
        Private Sub peakLinearNames(pack As LinearPack, file As netCDF.CDFWriter)
            Dim data As chars = pack.linears.SafeQuery.Select(Function(l) l.name).GetJson
            Dim size As New Dimension With {.name = "sizeofLinears", .size = data.Length}
            Dim attrs As attribute() = {
                New attribute With {
                    .name = "linears",
                    .type = CDFDataTypes.NC_INT,
                    .value = If(pack.linears.IsNullOrEmpty, 0, pack.linears.Length)
                }
            }

            file.AddVariable("linears", data, size, attrs)
        End Sub

        <Extension>
        Private Sub writeLinears(pack As LinearPack, file As netCDF.CDFWriter)
            For Each line As StandardCurve In pack.linears.SafeQuery
                Call line.writeLinear(file)
            Next
        End Sub

        <Extension>
        Private Sub writeLinear(linear As StandardCurve, file As netCDF.CDFWriter)
            Dim attrs As attribute() = {
                New attribute With {.name = "name", .type = CDFDataTypes.NC_CHAR, .value = linear.name},
                New attribute With {.name = "points", .type = CDFDataTypes.NC_INT, .value = linear.points.TryCount},
                New attribute With {.name = "R2", .type = CDFDataTypes.NC_DOUBLE, .value = If(linear.linear Is Nothing, 0.0, linear.linear.R2)}
            }

            Using buffer As MemoryStream = StandardCurveCDF.WriteCDF(linear)
                Dim bytes As bytes = buffer.ToArray
                Dim chunkSize As New Dimension With {.name = $"chunkof_{linear.name}", .size = bytes.Length}

                file.AddVariable(linear.name, bytes, chunkSize, attrs)
            End Using
        End Sub

        <Extension>
        Private Sub writeGlobals(pack As LinearPack, file As netCDF.CDFWriter)
            Dim title As New attribute With {.name = "title", .type = CDFDataTypes.NC_CHAR, .value = pack.title}
            Dim time As New attribute With {.name = "time", .type = CDFDataTypes.NC_CHAR, .value = pack.time.ToString}
            Dim github As New attribute With {.name = "github", .type = CDFDataTypes.NC_CHAR, .value = "https://github.com/xieguigang/mzkit"}
            Dim linears As New attribute With {.name = "linears", .type = CDFDataTypes.NC_INT, .value = If(pack.linears.IsNullOrEmpty, 0, pack.linears.Length)}
            Dim peaks As New attribute With {.name = "peaks", .type = CDFDataTypes.NC_INT, .value = If(pack.peakSamples.IsNullOrEmpty, 0, pack.peakSamples.Length)}
            Dim type As New attribute With {.name = "targetted", .type = CDFDataTypes.NC_CHAR, .value = pack.targetted.ToString}

            Call file.GlobalAttributes(title, time, github, linears, peaks, type)
        End Sub
    End Module
End Namespace
