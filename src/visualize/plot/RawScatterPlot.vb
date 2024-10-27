﻿#Region "Microsoft.VisualBasic::e148efa275809dcaf9f33945f2f9a94b, visualize\plot\RawScatterPlot.vb"

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

    '   Total Lines: 129
    '    Code Lines: 102 (79.07%)
    ' Comment Lines: 14 (10.85%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 13 (10.08%)
    '     File Size: 5.68 KB


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

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

''' <summary>
''' 横坐标为rt，纵坐标为m/z的散点图绘制
''' </summary>
Public Class RawScatterPlot : Inherits Plot

    ReadOnly samples As ms1_scan()
    ReadOnly mapLevels As Integer
    ReadOnly rawfile$

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="samples"></param>
    ''' <param name="mapLevels"></param>
    ''' <param name="rawfile">the serial data legend name</param>
    ''' <param name="theme"></param>
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

        theme.drawLegend = False

        Dim brushes = colors.Select(Function(colorStr) New SolidBrush(colorStr.TranslateColor)).ToArray
        Dim ticks = points.Select(Function(a) a.value ^ std.E).CreateAxisTicks
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim tickStyle As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
        Dim legendTitleStyle As Font = css.GetFont(CSSFont.TryParse(theme.legendTitleCSS))
        Dim tickAxisStroke As Pen = css.GetPen(Stroke.TryParse(theme.axisStroke))
        Dim scatter As New Scatter2D({serials}, theme, scatterReorder:=True, fillPie:=True) With {
            .xlabel = "scan_time in seconds",
            .ylabel = "M/Z Ratio"
        }

        ' 绘制标尺
        Dim canvas = region.PlotRegion(css)
        Dim width = css.GetWidth(region.Padding.Right) * (4 / 5)
        Dim legendLayout As New Rectangle(canvas.Right, canvas.Top, width, canvas.Height * (5 / 6))

        Call scatter.Plot(g, region)
        Call g.ColorMapLegend(
            layout:=legendLayout,
            designer:=brushes,
            ticks:=ticks,
            titleFont:=legendTitleStyle,
            title:=legendTitle,
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
                                          Optional legendtitle As String = "Intensity",
                                          Optional driver As Drivers = Drivers.Default,
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
        Dim app As New RawScatterPlot(samples, mapLevels, rawfile, theme) With {
            .legendTitle = legendtitle
        }

        Return app.Plot(size, ppi:=ppi, driver:=driver)
    End Function
End Class
