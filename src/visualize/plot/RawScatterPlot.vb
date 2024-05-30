﻿#Region "Microsoft.VisualBasic::51ce5c839ed529d8ffa6c59fa3bb64a0, visualize\plot\RawScatterPlot.vb"

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

    '   Total Lines: 127
    '    Code Lines: 100 (78.74%)
    ' Comment Lines: 14 (11.02%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 13 (10.24%)
    '     File Size: 5.59 KB


    ' Class RawScatterPlot
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Plot
    ' 
    '     Sub: PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

''' <summary>
''' 横坐标为rt，纵坐标为m/z的散点图绘制
''' </summary>
Public Class RawScatterPlot : Inherits Plot

    ReadOnly samples As ms1_scan()
    ReadOnly mapLevels As Integer
    ReadOnly rawfile$

    Public Sub New(samples As IEnumerable(Of ms1_scan), mapLevels As Integer, rawfile$, theme As Theme)
        MyBase.New(theme)

        Me.samples = samples.ToArray
        Me.rawfile = rawfile
        Me.mapLevels = mapLevels
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, region As GraphicsRegion)
        ' 先转换为散点图的数据系列
        Dim colors As String() = Designer _
            .GetColors(theme.colorSet, mapLevels) _
            .Select(Function(c) c.ToHtmlColor) _
            .ToArray

        Dim points As PointData() = samples _
           .Where(Function(a) a.intensity > 0) _
           .Select(Function(compound)
                       Return New PointData() With {
                           .pt = New PointF(compound.scan_time, compound.mz),
                           .value = std.Log(compound.intensity)
                       }
                   End Function) _
           .ToArray
        Dim serials As New SerialData With {
            .title = rawfile,
            .pts = points,
            .pointSize = theme.pointSize,
            .color = Nothing
        }
        Dim intensityRange As New DoubleRange(points.Select(Function(a) a.value).ToArray)
        Dim indexRange As New DoubleRange(0, colors.Length - 1)

        For i As Integer = 0 To points.Length - 1
            points(i).color = colors(CInt(intensityRange.ScaleMapping(points(i).value, indexRange)))
        Next

        Dim brushes = colors.Select(Function(colorStr) New SolidBrush(colorStr.TranslateColor)).ToArray
        Dim ticks = points.Select(Function(a) a.value ^ std.E).CreateAxisTicks
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim tickStyle As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
        Dim legendTitleStyle As Font = css.GetFont(CSSFont.TryParse(theme.legendTitleCSS))
        Dim tickAxisStroke As Pen = Stroke.TryParse(theme.axisStroke).GDIObject
        Dim scatter As New Scatter2D({serials}, theme, scatterReorder:=True, fillPie:=True) With {
            .xlabel = "scan_time in seconds",
            .ylabel = "m/z ratio"
        }

        ' 绘制标尺
        Dim canvas = region.PlotRegion
        Dim width = canvas.Width * 0.125
        Dim legendLayout As New Rectangle(region.Width - width - region.Padding.Right / 3, canvas.Top, width, canvas.Height * 0.3)

        Call scatter.Plot(g, region)
        Call g.ColorMapLegend(
            layout:=legendLayout,
            designer:=brushes,
            ticks:=ticks,
            titleFont:=legendTitleStyle,
            title:="Intensity Scale",
            tickFont:=tickStyle,
            tickAxisStroke:=tickAxisStroke
        )
    End Sub

    ''' <summary>
    ''' The scatter plots of the samples ``m/z`` and ``rt``.
    ''' </summary>
    ''' <param name="samples"></param>
    ''' <param name="size$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="margin$"></param>
    ''' <param name="ptSize!"></param>
    ''' <returns></returns>
    Public Overloads Shared Function Plot(samples As IEnumerable(Of ms1_scan),
                                          Optional size$ = "6000,4500",
                                          Optional bg$ = "white",
                                          Optional margin$ = "padding:200px 800px 500px 600px;",
                                          Optional rawfile$ = "n/a",
                                          Optional ptSize! = 24,
                                          Optional sampleColors$ = "darkblue,blue,skyblue,green,orange,red,darkred",
                                          Optional mapLevels As Integer = 25,
                                          Optional legendTitleCSS$ = CSSFont.PlotSubTitle,
                                          Optional tickCSS$ = CSSFont.Win7LittleLarge,
                                          Optional axisStroke$ = Stroke.AxisStroke,
                                          Optional axisLabelFont$ = CSSFont.Win7VeryLarge,
                                          Optional ppi As Integer = 300) As GraphicsData

        Dim theme As New Theme With {
            .background = bg,
            .colorSet = sampleColors,
            .legendTitleCSS = legendTitleCSS,
            .axisTickCSS = tickCSS,
            .axisStroke = axisStroke,
            .axisLabelCSS = axisLabelFont,
            .pointSize = ptSize,
            .padding = margin,
            .drawLegend = False
        }
        Dim app As New RawScatterPlot(samples, mapLevels, rawfile, theme)

        Return app.Plot(size, ppi:=ppi)
    End Function
End Class
