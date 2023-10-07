﻿#Region "Microsoft.VisualBasic::1b65057ea431884728ff1a2710ab8aae, mzkit\Rscript\Library\mzkit.quantify\Linears.vb"

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

'   Total Lines: 261
'    Code Lines: 164
' Comment Lines: 70
'   Blank Lines: 27
'     File Size: 10.26 KB


' Module Linears
' 
'     Function: CreateMRMDataSet, getIonPeakTable, GetLinearPoints, GetQuantifyResult, GetRawX
'               printIS, printLineModel, printStandards, SampleQuantify, StandardCurveDataSet
'               writeMRMpeaktable, writeStandardCurve
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.ASCII.MSL
Imports BioNovoGene.Analytical.MassSpectrometry.Math.GCMS
Imports BioNovoGene.Analytical.MassSpectrometry.Math.LinearQuantitative
Imports BioNovoGene.Analytical.MassSpectrometry.Math.LinearQuantitative.Data
Imports BioNovoGene.Analytical.MassSpectrometry.Math.LinearQuantitative.Linear
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MRM.Models
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports Rlist = SMRUCC.Rsharp.Runtime.Internal.Object.list
Imports stdNum = System.Math

<Package("Linears")>
Module Linears

    Sub Main()
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of StandardCurve)(AddressOf printLineModel)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of Standards())(AddressOf printStandards)
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of [IS]())(AddressOf printIS)

        ' create linear regression report
        REnv.Internal.htmlPrinter.AttachHtmlFormatter(Of StandardCurve())(AddressOf LinearReport.CreateHtml)
        REnv.Internal.htmlPrinter.AttachHtmlFormatter(Of LinearDataSet)(AddressOf LinearReport.CreateHtml)
    End Sub

    Private Function printLineModel(line As Object) As String
        If line Is Nothing Then
            Return "NULL"
        Else
            With DirectCast(line, StandardCurve)
                Return $"{ .name}: { .linear.ToString}"
            End With
        End If
    End Function

    Private Function printStandards(obj As Object) As String
        Dim csv = DirectCast(obj, Standards()).ToCsvDoc.ToMatrix.RowIterator.ToArray
        Dim printContent = csv.Print(addBorder:=False)

        Return printContent
    End Function

    Private Function printIS(obj As Object) As String
        Dim csv = DirectCast(obj, [IS]()).ToCsvDoc.ToMatrix.RowIterator.ToArray
        Dim printContent = csv.Print(addBorder:=False)

        Return printContent
    End Function

    ''' <summary>
    ''' export the table data of the reference linears
    ''' </summary>
    ''' <param name="lines"></param>
    ''' <returns></returns>
    <ExportAPI("lines.table")>
    Public Function StandardCurveDataSet(lines As StandardCurve()) As EntityObject()
        Return lines _
            .Select(Function(line)
                        Dim namePoint As ReferencePoint = line.points.ElementAtOrNull(Scan0)

                        Return New EntityObject With {
                            .ID = line.name,
                            .Properties = New Dictionary(Of String, String) From {
                                {"name", If(namePoint Is Nothing, line.name, namePoint.Name)},
                                {"equation", "f(x)=" & line.linear.Polynomial.ToString("G5", False)},
                                {"R2", stdNum.Sqrt(line.linear.R2)},
                                {"is.weighted", line.isWeighted},
                                {"IS.calibration", line.requireISCalibration},
                                {"IS", line.IS.name}
                            }
                        }
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' save reference point data into a given table file
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="file$"></param>
    ''' <returns></returns>
    <ExportAPI("write.points")>
    Public Function writeStandardCurve(points As ReferencePoint(), file$) As Boolean
        Return points.SaveTo(file, silent:=True)
    End Function

    ''' <summary>
    ''' Get reference input points
    ''' </summary>
    ''' <param name="linears"></param>
    ''' <param name="nameRef">The metabolite id</param>
    ''' <returns></returns>
    <ExportAPI("points")>
    <RApiReturn(GetType(ReferencePoint))>
    Public Function GetLinearPoints(linears As StandardCurve(),
                                    nameRef As Object,
                                    Optional env As Environment = Nothing) As Object
        Dim name As String

        If nameRef Is Nothing Then
            Return Internal.debug.stop({
                $"the required feature name reference can not be nothing!",
                $"parameter: {nameRef}"
            }, env)
        ElseIf TypeOf nameRef Is String Then
            name = nameRef
        ElseIf TypeOf nameRef Is MSLIon Then
            name = DirectCast(nameRef, MSLIon).Name
        ElseIf TypeOf nameRef Is IonPair Then
            name = DirectCast(nameRef, IonPair).accession
        ElseIf TypeOf nameRef Is QuantifyIon Then
            name = DirectCast(nameRef, QuantifyIon).id
        Else
            Return Message.InCompatibleType(GetType(String), nameRef.GetType, env)
        End If

        Dim line As StandardCurve = linears _
            .Where(Function(l)
                       Return l.name = name
                   End Function) _
            .FirstOrDefault

        If line Is Nothing Then
            Call env.AddMessage({$"linear data of '{name}' is not found...", $"nameref: {name}"}, MSG_TYPES.WRN)
            Return Nothing
        Else
            Return line.points
        End If
    End Function

    ''' <summary>
    ''' Write peak data which is extract from the raw file with 
    ''' the given ion pairs or quantify ion data
    ''' </summary>
    ''' <param name="ionPeaks"></param>
    ''' <param name="file">The output csv file path</param>
    ''' <returns></returns>
    <ExportAPI("write.ionPeaks")>
    Public Function writeMRMpeaktable(ionPeaks As IonPeakTableRow(), file$) As Boolean
        Return ionPeaks.SaveTo(file, silent:=True)
    End Function

    ''' <summary>
    ''' get ions peaks
    ''' </summary>
    ''' <param name="samples"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("ionPeaks")>
    <RApiReturn(GetType(IonPeakTableRow))>
    Public Function getIonPeakTable(<RRawVectorArgument>
                                    samples As Object,
                                    Optional env As Environment = Nothing) As Object

        Dim peaks As pipeline = pipeline.TryCreatePipeline(Of QuantifyScan)(samples, env)

        If peaks.isError Then
            Return peaks.getError
        End If

        Return peaks _
            .populates(Of QuantifyScan)(env) _
            .Select(Function(sample) sample.ionPeaks) _
            .IteratesALL _
            .ToArray
    End Function

    ''' <summary>
    ''' Run sample data quantify
    ''' </summary>
    ''' <param name="models"></param>
    ''' <param name="ions"></param>
    ''' <returns></returns>
    <ExportAPI("quantify")>
    Public Function SampleQuantify(models As StandardCurve(),
                                   ions As TargetPeakPoint(),
                                   Optional integrator As PeakAreaMethods = PeakAreaMethods.NetPeakSum,
                                   Optional names As Rlist = Nothing,
                                   Optional baselineQuantile As Double = 0.6,
                                   Optional fileName As String = "NA",
                                   Optional env As Environment = Nothing) As QuantifyScan

        Dim nameIndex As Dictionary(Of String, String)

        If names Is Nothing Then
            nameIndex = ions.ToDictionary(Function(ion) ion.Name, Function(ion) ion.Name)
        Else
            nameIndex = names.AsGeneric(Of String)(env)
        End If

        Return models.SampleQuantify(ions, integrator, nameIndex, baselineQuantile, fileName)
    End Function

    ''' <summary>
    ''' Get quantify result
    ''' </summary>
    ''' <param name="fileScans"></param>
    ''' <returns></returns>
    <ExportAPI("result")>
    Public Function GetQuantifyResult(fileScans As QuantifyScan()) As DataSet()
        Return fileScans.Select(Function(file) file.quantify).ToArray
    End Function

    ''' <summary>
    ''' Get result of ``AIS/At``
    ''' </summary>
    ''' <param name="fileScans"></param>
    ''' <returns></returns>
    <ExportAPI("scans.X")>
    Public Function GetRawX(fileScans As QuantifyScan()) As DataSet()
        Return fileScans.Select(Function(file) file.rawX).ToArray
    End Function

    <ExportAPI("read.linearPack")>
    Public Function readLinearPack(file As String) As LinearPack
        Return LinearPack.OpenFile(file)
    End Function

    ''' <summary>
    ''' Create targeted linear dataset object for do linear quantification data report.
    ''' </summary>
    ''' <param name="standardCurve"></param>
    ''' <param name="samples"></param>
    ''' <param name="QC_dataset">
    ''' Regular expression pattern string for match QC sample files
    ''' </param>
    ''' <param name="ionsRaw">
    ''' A list of ions ChromatogramTick data for run ion data plots.
    ''' the list data structure in format looks like:
    ''' 
    ''' ```
    ''' { 
    '''    ion_id1 {
    '''        sample1: ChromatogramTick[],
    '''        sample2: ChromatogramTick[],
    '''        ...
    '''    }
    ''' }
    ''' ```
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("report.dataset")>
    <RApiReturn(GetType(LinearDataSet), GetType(QCData))>
    Public Function CreateMRMDataSet(standardCurve As StandardCurve(), samples As QuantifyScan(),
                                     Optional QC_dataset$ = Nothing,
                                     Optional ionsRaw As Rlist = Nothing) As Object

        If Not QC_dataset.StringEmpty Then
            Return New QCData With {
                .model = standardCurve,
                .result = samples,
                .matchQC = QC_dataset
            }
        Else
            Return New LinearDataSet With {
                .StandardCurve = standardCurve,
                .Samples = samples,
                .IonsRaw = ionsRaw
            }
        End If
    End Function
End Module
