﻿#Region "Microsoft.VisualBasic::2aaf4a362c848f1a8f55f9159784352e, mzkit\Rscript\Library\mzkit\comprehensive\SingleCells.vb"

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

    '   Total Lines: 92
    '    Code Lines: 63
    ' Comment Lines: 18
    '   Blank Lines: 11
    '     File Size: 3.94 KB


    ' Module SingleCells
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: cellMatrix, cellStatsTable, singleCellsIons
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.SingleCells
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.genomics.Analysis.HTS.DataFrame
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports HTSMatrix = SMRUCC.genomics.Analysis.HTS.DataFrame.Matrix
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.[Object].dataframe
Imports SingleCellMath = BioNovoGene.Analytical.MassSpectrometry.SingleCells.Deconvolute.Math
Imports SingleCellMatrix = BioNovoGene.Analytical.MassSpectrometry.SingleCells.Deconvolute.PeakMatrix

''' <summary>
''' Single cells metabolomics data processor
''' </summary>
''' 
<Package("SingleCells")>
Module SingleCells

    Sub New()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(SingleCellIonStat()), AddressOf cellStatsTable)
    End Sub

    Private Function cellStatsTable(ions As SingleCellIonStat(), args As list, env As Environment) As Rdataframe
        Dim table As New Rdataframe With {
           .columns = New Dictionary(Of String, Array),
           .rownames = ions _
               .Select(Function(i) i.mz.ToString("F4")) _
               .ToArray
        }

        Call table.add(NameOf(SingleCellIonStat.mz), ions.Select(Function(i) i.mz))
        Call table.add(NameOf(SingleCellIonStat.cells), ions.Select(Function(i) i.cells))
        Call table.add(NameOf(SingleCellIonStat.maxIntensity), ions.Select(Function(i) i.maxIntensity))
        Call table.add(NameOf(SingleCellIonStat.baseCell), ions.Select(Function(i) i.baseCell))
        Call table.add(NameOf(SingleCellIonStat.Q1Intensity), ions.Select(Function(i) i.Q1Intensity))
        Call table.add(NameOf(SingleCellIonStat.Q2Intensity), ions.Select(Function(i) i.Q2Intensity))
        Call table.add(NameOf(SingleCellIonStat.Q3Intensity), ions.Select(Function(i) i.Q3Intensity))
        Call table.add(NameOf(SingleCellIonStat.RSD), ions.Select(Function(i) i.RSD))

        Return table
    End Function

    ''' <summary>
    ''' export single cell expression matrix from the raw data scans
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="mzdiff"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("cell_matrix")>
    Public Function cellMatrix(raw As mzPack,
                               Optional mzdiff As Double = 0.005,
                               Optional freq As Double = 0.001,
                               Optional env As Environment = Nothing) As Object

        Dim singleCells As New List(Of DataFrameRow)
        Dim mzSet As Double() = SingleCellMath.GetMzIndex(raw:=raw, mzdiff:=mzdiff, freq:=freq)

        For Each cell_scan As DataFrameRow In SingleCellMatrix.ExportScans(Of DataFrameRow)(raw, mzSet)
            cell_scan.geneID = cell_scan.geneID _
                .Replace("[MS1]", "") _
                .Trim
            singleCells.Add(cell_scan)
        Next

        Return New HTSMatrix With {
            .expression = singleCells.ToArray,
            .sampleID = mzSet _
                .Select(Function(mzi) mzi.ToString("F4")) _
                .ToArray,
            .tag = raw.source
        }
    End Function

    ''' <summary>
    ''' do stats of the single cell metabolomics ions
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="da"></param>
    ''' <param name="parallel"></param>
    ''' <returns></returns>
    <ExportAPI("SCM_ionStat")>
    <RApiReturn(GetType(SingleCellIonStat))>
    Public Function singleCellsIons(raw As mzPack,
                                    Optional da As Double = 0.01,
                                    Optional parallel As Boolean = True) As Object

        Return SingleCellIonStat.DoIonStats(raw, da, parallel).ToArray
    End Function
End Module
