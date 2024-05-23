﻿#Region "Microsoft.VisualBasic::b8bb546f12ca564e37eee2b8b2cf1740, mzmath\TargetedMetabolomics\GCMS\Raw.vb"

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

    '   Total Lines: 97
    '    Code Lines: 49 (50.52%)
    ' Comment Lines: 36 (37.11%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 12 (12.37%)
    '     File Size: 3.64 KB


    '     Class Raw
    ' 
    '         Properties: attributes, fileName, ms, mz, tic
    '                     times, title
    ' 
    '         Function: (+2 Overloads) GetMsScan, GetScanIndex, GetTIC, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace GCMS

    ''' <summary>
    ''' 从CDF文件之中所读取出来的最原始的数据
    ''' </summary>
    Public Class Raw

        Public Property title As String
        ''' <summary>
        ''' the global attribute in netcdf file
        ''' </summary>
        ''' <returns></returns>
        Public Property attributes As Dictionary(Of String, String)

#Region "Gas Chromatography"
        ''' <summary>
        ''' [气相色谱数据] 按照从开始到结束升序排序过了的
        ''' </summary>
        ''' <returns></returns>
        Public Property times As Double()
        ''' <summary>
        ''' [气相色谱数据] 相应强度是和<see cref="times"/>一一对应的
        ''' </summary>
        ''' <returns></returns>
        Public Property tic As Double()
#End Region

        ''' <summary>
        ''' 质谱扫描的结果数据
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 以扫描的时间为主线的
        ''' </remarks>
        Public Property ms As ms1_scan()()
        Public Property mz As Double()

        Public Property fileName As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetTIC() As NamedCollection(Of ChromatogramTick)
            Return New NamedCollection(Of ChromatogramTick) With {
                .name = title,
                .value = ChromatogramTick.Zip(times, tic).ToArray
            }
        End Function

        Dim index As New Lazy(Of IndexSelector)(Function() IndexSelector.FromSortSequence(times))

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="min">the min value of the rt range</param>
        ''' <param name="max">the max value of the rt range</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetScanIndex(min#, max#) As IEnumerable(Of Integer)
            Return index.Value.SelectByRange(min, max)
        End Function

        ''' <summary>
        ''' 将某一个时间范围内的<see cref="ms"/>扫描结果都拿出来
        ''' </summary>
        ''' <param name="ROI"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMsScan(ROI As DoubleRange) As ms1_scan()
            Return GetMsScan(ROI.Min, ROI.Max)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMsScan(rtmin#, rtmax#) As ms1_scan()
            ' 先找到下标的集合
            ' 然后再取出scan_index对应的ms scan数据
            Return GetScanIndex(rtmin, rtmax) _
                .Select(Function(scanIndex)
                            Return ms(scanIndex)
                        End Function) _
                .IteratesALL _
                .OrderBy(Function(scan) scan.mz) _
                .ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"[{fileName}] {title Or "NA".AsDefault}, scan_time [{times.Min.ToString("F1")} ~ {times.Max.ToString("F1")}s]"
        End Function
    End Class
End Namespace
