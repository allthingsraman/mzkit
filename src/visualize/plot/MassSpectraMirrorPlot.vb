﻿#Region "Microsoft.VisualBasic::a5fe3a003cf00381c6468017cce585c7, visualize\plot\MassSpectraMirrorPlot.vb"

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

    '   Total Lines: 115
    '    Code Lines: 98 (85.22%)
    ' Comment Lines: 9 (7.83%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 8 (6.96%)
    '     File Size: 5.18 KB


    ' Module MassSpectra
    ' 
    '     Function: AlignMirrorPlot, MirrorPlot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS

Public Module MassSpectra

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="library"></param>
    ''' <param name="size$"></param>
    ''' <param name="margin$"></param>
    ''' <param name="intoCutoff#"></param>
    ''' <param name="titles">[query, reference]</param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function MirrorPlot(library As LibraryMatrix,
                               Optional size$ = "1200,800",
                               Optional margin$ = "padding: 100px 30px 50px 100px;",
                               Optional bg$ = "white",
                               Optional intoCutoff# = 0.05,
                               Optional titles$() = Nothing,
                               Optional plotTitle$ = "BioDeep™ MS/MS alignment Viewer",
                               Optional labelDisplayIntensity# = 0.3,
                               Optional drawLegend As Boolean = True,
                               Optional xlab$ = "M/Z ratio",
                               Optional ylab$ = "Relative Intensity(%)",
                               Optional drawGrid As Boolean = True,
                               Optional tagXFormat$ = "F2",
                               Optional bw As Single = 8,
                               Optional legendLayout As String = "top-right",
                               Optional color1 As String = AlignmentPlot.DefaultColor1,
                               Optional color2 As String = AlignmentPlot.DefaultColor2,
                               Optional DrawGridX As Boolean = False,
                               Optional gridStrokeX As String = PlotAlignmentGroup.DefaultGridXStroke,
                               Optional gridStrokeY As String = PlotAlignmentGroup.DefaultGridYStroke,
                               Optional driver As Drivers = Drivers.Default) As GraphicsData

        Dim a As New LibraryMatrix With {.ms2 = library.ms2, .name = titles.ElementAtOrDefault(0, library.name)}
        Dim b As New LibraryMatrix With {.ms2 = library.ms2, .name = titles.ElementAtOrDefault(1, library.name)}

        Return AlignMirrorPlot(
            a, b,
            size:=size,
            intoCutoff:=intoCutoff,
            margin:=margin,
            title:=plotTitle,
            labelDisplayIntensity:=labelDisplayIntensity,
            drawLegend:=drawLegend,
            bg:=bg,
            xlab:=xlab,
            ylab:=ylab,
            drawGrid:=drawGrid,
            tagXFormat:=tagXFormat,
            driver:=driver,
            bw:=bw,
            legendLayout:=legendLayout,
            color1:=color1,
            color2:=color2,
            drawGridX:=DrawGridX,
            gridStrokeX:=gridStrokeX,
            gridStrokeY:=gridStrokeY
        )
    End Function

    Public Function AlignMirrorPlot(query As LibraryMatrix, ref As LibraryMatrix,
                                    Optional size$ = "1200,800",
                                    Optional margin$ = "padding: 100px 30px 50px 100px;",
                                    Optional bg$ = "white",
                                    Optional intoCutoff# = 0.05,
                                    Optional title$ = "BioDeep™ MS/MS alignment Viewer",
                                    Optional labelDisplayIntensity# = 0.3,
                                    Optional drawLegend As Boolean = True,
                                    Optional xlab$ = "M/Z ratio",
                                    Optional ylab$ = "Relative Intensity(%)",
                                    Optional tagXFormat$ = "F2",
                                    Optional drawGrid As Boolean = True,
                                    Optional drawGridX As Boolean = False,
                                    Optional bw As Single = 8,
                                    Optional legendLayout As String = "top-right",
                                    Optional color1 As String = DefaultColor1,
                                    Optional color2 As String = DefaultColor2,
                                    Optional gridStrokeX As String = PlotAlignmentGroup.DefaultGridXStroke,
                                    Optional gridStrokeY As String = PlotAlignmentGroup.DefaultGridYStroke,
                                    Optional highlights As Double() = Nothing,
                                    Optional driver As Drivers = Drivers.Default) As GraphicsData

        Dim mz As Double() = query _
            .Trim(intoCutoff) _
            .Join(ref.Trim(intoCutoff)) _
            .Select(Function(mass) mass.mz) _
            .ToArray
        Dim mzRange As DoubleRange = mz.Range
        Dim qMatrix As (x#, into#)() = query.Select(Function(q) (q.mz, q.intensity)).ToArray
        Dim sMatrix As (x#, into#)() = ref.Select(Function(s) (s.mz, s.intensity)).ToArray

        mzRange = {
            mzRange.Min - (mzRange.Min * 0.125),
            mzRange.Max + (mzRange.Max * 0.125)
        }

        Return AlignmentPlot.PlotAlignment(
            qMatrix, sMatrix,
            queryName:=query.name,
            subjectName:=ref.name,
            xrange:=mzRange,
            yrange:=New Double() {0, 100},
            size:=size, padding:=margin,
            xlab:=xlab,
            ylab:=ylab,
            title:=title,
            titleCSS:=CSSFont.Win7Large,
            format:="F0",
            yAxislabelPosition:=YlabelPosition.LeftCenter,
            labelPlotStrength:=labelDisplayIntensity,
            drawLegend:=drawLegend,
            bg:=bg,
            drawGrid:=drawGrid,
            tagXFormat:=tagXFormat,
            driver:=driver,
            bw:=bw,
            legendLayout:=legendLayout,
            cla:=color1,
            clb:=color2,
            drawGridX:=drawGridX,
            gridStrokeX:=gridStrokeX,
            gridStrokeY:=gridStrokeY,
            highlights:=highlights
        )
    End Function
End Module
