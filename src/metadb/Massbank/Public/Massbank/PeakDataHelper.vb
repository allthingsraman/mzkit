﻿#Region "Microsoft.VisualBasic::8f8b3f5aced15c2943db99204bc48d80, G:/mzkit/src/metadb/Massbank//Public/Massbank/PeakDataHelper.vb"

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

    '   Total Lines: 67
    '    Code Lines: 38
    ' Comment Lines: 21
    '   Blank Lines: 8
    '     File Size: 2.73 KB


    '     Module PeakDataHelper
    ' 
    '         Function: Join
    '         Enum NormalizationMethods
    ' 
    '             RelativeMax, RelativeSum
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: Normalize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.BioDeep.Chemistry.Massbank.DATA
Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace Massbank

    Public Module PeakDataHelper

        ''' <summary>
        ''' ``x,y x,y x,y .....``
        ''' </summary>
        ''' <param name="peakData"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Join(peakData As IEnumerable(Of DoubleTagged(Of Double))) As String
            Return peakData _
                .Select(Function(pk) $"{pk.Tag},{pk.Value}") _
                .JoinBy(" ")
        End Function

        ''' <summary>
        ''' 归一化方法列表
        ''' </summary>
        Public Enum NormalizationMethods
            RelativeSum
            RelativeMax
        End Enum

        ''' <summary>
        ''' ###### 对峰的信号量进行归一化处理，即将信号量转换为0到1之间的百分比
        ''' 
        ''' + <see cref="NormalizationMethods.RelativeSum"/>
        ''' 这些``rel.int.``是用来鉴定单个物质的碎片响应值就可以按上面那样 算
        ''' 那就是单个碎片的相对峰值除以该物质的所有碎片的相对峰值之和，获得
        ''' 就是将``rel.int.``都加起来，然后``rel.int.``列里面的每一个值都除以这个和就行了么？
        ''' 
        ''' + <see cref="NormalizationMethods.RelativeMax"/>
        ''' 直接使用相对信号强度除以最大的信号强度得到百分比值进行归一化
        ''' </summary>
        ''' <param name="record">``MS/MS``信号峰数据</param>
        ''' <returns></returns>
        <Extension>
        Public Function Normalize(record As Record, Optional method As NormalizationMethods = NormalizationMethods.RelativeMax) As DoubleTagged(Of Double)()
            Dim base#

            Select Case method
                Case NormalizationMethods.RelativeMax
                    base = record.PK.PEAK.Max(Function(pk) pk.relint)
                Case NormalizationMethods.RelativeSum
                    base = record.PK.PEAK.Sum(Function(pk) pk.relint)
                Case Else
                    Throw New NotImplementedException
            End Select

            Dim out As DoubleTagged(Of Double)() = record.PK.PEAK _
                .Select(Function(pk)
                            Return New DoubleTagged(Of Double) With {
                                .Tag = pk.mz,
                                .Value = pk.relint / base
                            }
                        End Function) _
                .ToArray

            Return out
        End Function
    End Module
End Namespace
