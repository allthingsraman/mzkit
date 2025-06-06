﻿#Region "Microsoft.VisualBasic::fa10dd665bd00f8c96dee3de3f79e5d3, mzmath\Oligonucleotide_MS\MS_Peak\Match.vb"

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

    '   Total Lines: 99
    '    Code Lines: 48 (48.48%)
    ' Comment Lines: 44 (44.44%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (7.07%)
    '     File Size: 3.15 KB


    ' Class Match
    ' 
    '     Properties: Adduct, End3, End5, Ends, ErrorPpm
    '                 f1StOccurance, Frequency, Length, Name, ObservedMass
    '                 Sequence, Start, TheoreticalMass
    ' 
    '     Function: ToString
    ' 
    '     Sub: Print
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq

Public Class Match

    ''' <summary>
    ''' 1
    ''' </summary>
    <Column("Observed Mass")> Public Property ObservedMass As Double
    ''' <summary>
    ''' 2
    ''' </summary>
    <Column("Sequence")> Public Property Sequence As String
    ''' <summary>
    ''' 3
    ''' </summary>
    <Column("Start")> Public Property Start As Integer
    ''' <summary>
    ''' 4
    ''' </summary>
    <Column("End")> Public Property Ends As Integer
    ''' <summary>
    ''' 5
    ''' </summary>
    <Column("Length")> Public Property Length As Integer
    ''' <summary>
    ''' 6
    ''' </summary>
    <Column("5' End")> Public Property End5 As String
    ''' <summary>
    ''' 7
    ''' </summary>
    <Column("3' End")> Public Property End3 As String
    ''' <summary>
    ''' 8
    ''' </summary>
    <Column("Adduct")> Public Property Adduct As String
    ''' <summary>
    ''' 9
    ''' </summary>
    <Column("Theoretical Mass")> Public Property TheoreticalMass As Double
    ''' <summary>
    ''' 10
    ''' </summary>
    <Column("Error (ppm)")> Public Property ErrorPpm As Double
    ''' <summary>
    ''' 11
    ''' </summary>
    <Column("Name")> Public Property Name As String
    ''' <summary>
    ''' 12
    ''' </summary>
    <Column("Frequency")> Public Property Frequency As Double
    ''' <summary>
    ''' 13
    ''' </summary>
    <Column("1st Occurance")> Public Property f1StOccurance As String

    Public Overrides Function ToString() As String
        Return {
            ObservedMass, Sequence, Start, Ends, Length, End5, End3,
            Adduct, TheoreticalMass, ErrorPpm,
            Name, Frequency, f1StOccurance
        }.JoinBy(", ")
    End Function

    ''' <summary>
    ''' print table
    ''' </summary>
    ''' <param name="outputs"></param>
    ''' <param name="dev"></param>
    Public Shared Sub Print(outputs As IEnumerable(Of Match), dev As TextWriter)
        Dim content As ConsoleTableBaseData = ConsoleTableBaseData.FromColumnHeaders(
            "Observed Mass", "Sequence", "Start", "End", "Length", "5' End", "3' End",
            "Adduct", "Theoretical Mass", "Error (ppm)",
            "Name", "Frequency", "1st Occurance"
        )

        For Each hit As Match In outputs
            Call content.AppendLine(
                hit.ObservedMass, hit.Sequence, hit.Start, hit.Ends, hit.Length, hit.End5, hit.End3,
                hit.Adduct, hit.TheoreticalMass, hit.ErrorPpm,
                hit.Name, hit.Frequency, hit.f1StOccurance
            )
        Next

        Call ConsoleTableBuilder _
            .From(content) _
            .WithFormat(ConsoleTableBuilderFormat.Minimal) _
            .Export _
            .ToString() _
            .DoCall(AddressOf dev.WriteLine)
        Call dev.Flush()
    End Sub

End Class
