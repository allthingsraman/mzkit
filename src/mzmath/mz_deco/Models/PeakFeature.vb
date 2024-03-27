﻿#Region "Microsoft.VisualBasic::fb6350ceb9235236b7ab430b6adb1e20, mzkit\src\mzmath\mz_deco\Models\PeakFeature.vb"

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

'   Total Lines: 71
'    Code Lines: 25
' Comment Lines: 35
'   Blank Lines: 11
'     File Size: 2.07 KB


' Class PeakFeature
' 
'     Properties: area, baseline, integration, maxInto, mz
'                 noise, nticks, rt, rtmax, rtmin
'                 snRatio, xcms_id
' 
'     Function: ToString
' 
' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Math.SignalProcessing.COW
Imports std = System.Math

''' <summary>
''' a result peak feature
''' </summary>
Public Class PeakFeature
    Implements IRetentionTime
    Implements IROI
    Implements IPeak2D
    Implements IMs1Scan
    Implements INamedValue

    ''' <summary>
    ''' the unique reference id of the current peak object
    ''' </summary>
    ''' <returns></returns>
    Public Property xcms_id As String Implements IPeak2D.ID, INamedValue.Key

    ''' <summary>
    ''' the xic m/z
    ''' </summary>
    ''' <returns></returns>
    Public Property mz As Double Implements IMs1Scan.mz, IPeak2D.Dimension1

    ''' <summary>
    ''' 出峰达到峰高最大值<see cref="maxInto"/>的时间点
    ''' </summary>
    ''' <returns></returns>
    Public Property rt As Double Implements IRetentionTime.rt, IPeak2D.Dimension2
    Public Property rtmin As Double Implements IROI.rtmin
    Public Property rtmax As Double Implements IROI.rtmax

    ''' <summary>
    ''' the retention index of the corresponding <see cref="rt"/> value.
    ''' </summary>
    ''' <returns></returns>
    Public Property RI As Double

    ''' <summary>
    ''' 这个区域的最大峰高度
    ''' </summary>
    ''' <returns></returns>
    Public Property maxInto As Double

    ''' <summary>
    ''' 所计算出来的基线的响应强度
    ''' </summary>
    ''' <returns></returns>
    Public Property baseline As Double
    ''' <summary>
    ''' 当前的这个ROI的峰面积积分值
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 为当前的ROI峰面积占整个TIC峰面积的百分比，一个实验所导出来的所有的ROI的
    ''' 积分值加起来应该是约等于100的
    ''' </remarks>
    Public Property integration As Double
    ''' <summary>
    ''' 计算得到的峰面积
    ''' </summary>
    ''' <returns></returns>
    Public Property area As Double Implements IMs1Scan.intensity, IPeak2D.Intensity

    ''' <summary>
    ''' 噪声的面积积分百分比
    ''' </summary>
    ''' <returns></returns>
    Public Property noise As Double

    Public Property nticks As Integer

    ''' <summary>
    ''' the sample file name reference
    ''' </summary>
    ''' <returns></returns>
    Public Property rawfile As String

    ''' <summary>
    ''' 信噪比
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property snRatio As Double
        Get
            Return std.Log(integration / noise)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{xcms_id}] {mz.ToString("F4")}@[{rtmin.ToString("F1")}, {rtmax.ToString("F1")}] = {area.ToString("G3")}"
    End Function

End Class

''' <summary>
''' the retention index reference
''' </summary>
Public Class RIRefer : Implements INamedValue, IReadOnlyId, IRetentionIndex, IRetentionTime, IMs1

    Public Property name As String Implements INamedValue.Key, IReadOnlyId.Identity
    Public Property mz As Double Implements IMs1.mz
    Public Property rt As Double Implements IMs1.rt

    ''' <summary>
    ''' the reference retention index value
    ''' </summary>
    ''' <returns></returns>
    Public Property RI As Double Implements IRetentionIndex.RI

    Public Overrides Function ToString() As String
        Return name
    End Function

End Class