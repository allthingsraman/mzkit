﻿#Region "Microsoft.VisualBasic::a7cb6779754808366d1f91786395ed66, mzkit\src\visualize\plot\ChromatogramPlot\ChromatogramPeakPlot.vb"

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

'   Total Lines: 209
'    Code Lines: 158
' Comment Lines: 25
'   Blank Lines: 26
'     File Size: 9.87 KB


' Module ChromatogramPeakPlot
' 
'     Function: Plot
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' time -> into
''' </summary>
Public Class ChromatogramPeakPlot : Inherits Plot

    Public Const DefaultPadding$ = "padding: 200px 80px 150px 150px"

    ''' <summary>
    ''' 绘制一个单独的峰
    ''' </summary>
    ''' <param name="chromatogram"></param>
    ''' <param name="size$"></param>
    ''' <param name="padding$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="title$"></param>
    ''' <param name="curveStyle$"></param>
    ''' <param name="titleFontCSS$"></param>
    ''' <param name="showMRMRegion">Debug usage</param>
    ''' <param name="ROI_styleCSS$"></param>
    ''' <param name="baselineQuantile">
    ''' 如果基线过高的话，显示累加线会在基线处出现斜边的情况，会需要这个参数来计算基线然后减去基线值来修正累加曲线
    ''' </param>
    ''' <param name="showAccumulateLine">
    ''' 这个类似于对峰面积积分的结果
    ''' </param>
    ''' <returns></returns>
    Public Overloads Function Plot(chromatogram As ChromatogramTick(),
                                   Optional size$ = "2100,1600",
                                   Optional padding$ = DefaultPadding,
                                   Optional bg$ = "white",
                                   Optional title$ = "NULL",
                                   Optional curveStyle$ = Stroke.ScatterLineStroke,
                                   Optional titleFontCSS$ = CSSFont.Win7VeryLarge,
                                   Optional legendFontCSS$ = CSSFont.Win7LargerNormal,
                                   Optional showMRMRegion As Boolean = False,
                                   Optional ROI_styleCSS$ = "stroke: red; stroke-width: 4px; stroke-dash: dash;",
                                   Optional baseLine_styleCSS$ = "stroke: green; stroke-width: 4px; stroke-dash: dash;",
                                   Optional accumulateLineStyleCss$ = "stroke: blue; stroke-width: 4px; stroke-dash: dash;",
                                   Optional showAccumulateLine As Boolean = False,
                                   Optional baselineQuantile# = 0.65,
                                   Optional angleThreshold# = 8,
                                   Optional peakwidth As DoubleRange = Nothing,
                                   Optional isMRM As Boolean = True,
                                   Optional sn_threshold As Double = 3,
                                   Optional ppi As Integer = 100) As GraphicsData

        Dim timeTicks#() = chromatogram.TimeArray.CreateAxisTicks
        Dim intoTicks#() = chromatogram.IntensityArray.CreateAxisTicks
        Dim accumulate# = 0
        Dim base = chromatogram.Baseline(baselineQuantile)
        Dim sumAll = (chromatogram.IntensityArray.AsVector - base) _
            .Where(Function(x) x > 0) _
            .Sum()
        Dim maxInto = intoTicks.Max - base
        Dim ay = Function(into As Double) As Double
                     into = into - base
                     accumulate += If(into < 0, 0, into)
                     Return (accumulate / sumAll) * maxInto
                 End Function
        Dim curvePen As Pen = Stroke.TryParse(curveStyle).GDIObject
        Dim titleFont As Font = CSSFont.TryParse(titleFontCSS).GDIObject(ppi)
        Dim legendFont As Font = CSSFont.TryParse(legendFontCSS).GDIObject(ppi)
        Dim ROIpen As Pen = Stroke.TryParse(ROI_styleCSS).GDIObject
        Dim baselinePen As Pen = Stroke.TryParse(baseLine_styleCSS).GDIObject
        Dim accumulateLine As Pen = Stroke.TryParse(accumulateLineStyleCss).GDIObject
        Dim legends As New List(Of NamedValue(Of Pen))
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim rect As Rectangle = region.PlotRegion
                Dim X = d3js.scale.linear.domain(values:=timeTicks).range(integers:={rect.Left, rect.Right})
                Dim Y = d3js.scale.linear.domain(values:=intoTicks).range(integers:={rect.Top, rect.Bottom})
                Dim scaler As New DataScaler With {
                    .X = X,
                    .Y = Y,
                    .region = rect,
                    .AxisTicks = (timeTicks, intoTicks)
                }
                Dim ZERO = scaler.TranslateY(0)

                Call g.DrawAxis(
                    region, scaler, showGrid:=False,
                    xlabel:="Time (s)",
                    ylabel:="Intensity",
                    htmlLabel:=False,
                    XtickFormat:="F0",
                    YtickFormat:="G3"
                )

                Dim A, B As PointF
                ' 累加线
                Dim ac1 As New PointF
                Dim ac2 As PointF

                ' 在这里绘制色谱曲线
                For Each signal As SlideWindow(Of PointF) In chromatogram _
                    .Select(Function(c)
                                Return New PointF(c.Time, c.Intensity)
                            End Function) _
                    .SlideWindows(winSize:=2)

                    A = scaler.Translate(signal.First)
                    B = scaler.Translate(signal.Last)

                    Call g.DrawLine(curvePen, A, B)

                    If showAccumulateLine Then
                        ac2 = New PointF(signal.First.X, ay(signal.First.Y))
                        g.DrawLine(accumulateLine, scaler.Translate(ac1), scaler.Translate(ac2))
                        ac1 = ac2
                    End If
                Next

                If showMRMRegion Then

                    ' 取出最大的ROI就是MRM色谱峰的保留时间范围
                    Dim MRM_ROIs As ROI() = chromatogram _
                        .Shadows _
                        .PopulateROI(
                            angleThreshold:=angleThreshold,
                            peakwidth:=peakwidth,
                            snThreshold:=sn_threshold
                        ) _
                        .ToArray
                    Dim maxIntensity# = intoTicks.Max
                    Dim canvas As IGraphics = g
                    Dim drawLine = Sub(x1 As PointF, x2 As PointF, isBaseline As Boolean)
                                       x1 = scaler.Translate(x1)
                                       x2 = scaler.Translate(x2)

                                       If isBaseline Then
                                           Call canvas.DrawLine(baselinePen, x1, x2)
                                       Else
                                           Call canvas.DrawLine(ROIpen, x1, x2)
                                       End If
                                   End Sub

                    For Each MRM_ROI As ROI In MRM_ROIs
                        A = New PointF(MRM_ROI.time.Min, 0)
                        B = New PointF(MRM_ROI.time.Min, maxIntensity)
                        drawLine(A, B, False)

                        A = New PointF(MRM_ROI.time.Max, 0)
                        B = New PointF(MRM_ROI.time.Max, maxIntensity)
                        drawLine(A, B, False)

                        A = New PointF(timeTicks.Min, base)
                        B = New PointF(timeTicks.Max, base)

                        drawLine(A, B, True)
                    Next
                End If

                Dim left = rect.Left + (rect.Width - g.MeasureString(title, titleFont).Width) / 2
                Dim top = (rect.Top - titleFont.Height) / 2 - 10

                Call g.DrawString(title, titleFont, Brushes.Black, left, top)

                If showAccumulateLine Then
                    legends += New NamedValue(Of Pen) With {.Name = "Area Integration", .Value = accumulateLine}
                End If
                If showMRMRegion Then
                    legends += New NamedValue(Of Pen) With {.Name = "Chromatography ROI", .Value = Stroke.TryParse(ROI_styleCSS).GDIObject}
                    legends += New NamedValue(Of Pen) With {.Name = "Baseline", .Value = baselinePen}
                End If

                If legends > 0 Then
                    legends += New NamedValue(Of Pen) With {.Name = "Chromatogram", .Value = curvePen}

                    Dim lineWidth% = 100
                    Dim maxLegend As SizeF = g.MeasureString(legends.Keys.MaxLengthString, legendFont)
                    Dim offset = maxLegend.Height / 2

                    left = rect.Right - lineWidth * 1.25 - maxLegend.Width
                    top = rect.Top + 10

                    For Each legend As NamedValue(Of Pen) In legends
                        Call g.DrawString(legend.Name, legendFont, Brushes.Black, New PointF(left, top))
                        Call g.DrawLine(legend.Value, New PointF(left + maxLegend.Width, top + offset), New PointF(rect.Right - 20.0!, top + offset))

                        top += maxLegend.Height + 5
                    Next
                End If
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            padding,
            bg,
            plotInternal,
            dpi:=$"{ppi},{ppi}")
    End Function
End Class
