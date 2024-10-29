﻿#Region "Microsoft.VisualBasic::cc3d61ce5f1cbdf04a11c282b7c4f262, mzmath\ms2_math-core\Extensions.vb"

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

    '   Total Lines: 150
    '    Code Lines: 88 (58.67%)
    ' Comment Lines: 46 (30.67%)
    '    - Xml Docs: 97.83%
    ' 
    '   Blank Lines: 16 (10.67%)
    '     File Size: 5.67 KB


    ' Module Extensions
    ' 
    '     Function: CreateLibraryMatrix, (+2 Overloads) CreateMzIndex, GroupByMz, KovatsRI, (+2 Overloads) RetentionIndex
    '               Trim, TrimBaseline
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.ComponentModel.Algorithm
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

<HideModuleName> Public Module Extensions

    <Extension>
    Public Function CreateMzIndex(mzSet As Double(),
                                  Optional win_size As Double = 1,
                                  Optional verbose As Boolean = True) As BlockSearchFunction(Of MzIndex)
        If verbose Then
            Call VBDebugger.EchoLine($"tolerance window size: {win_size}")
        End If

        Return New BlockSearchFunction(Of MzIndex)(
            data:=mzSet.Select(Function(mzi, i) New MzIndex(mzi, i)),
            eval:=Function(i) i.mz,
            tolerance:=win_size,
            fuzzy:=True
        )
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateMzIndex(spectrum As IEnumerable(Of ms2), Optional win_size As Double = 1) As BlockSearchFunction(Of MzIndex)
        Return spectrum _
            .Select(Function(mzi) mzi.mz) _
            .ToArray _
            .CreateMzIndex(win_size)
    End Function

    ''' <summary>
    ''' 将响应强度低于一定值的碎片进行删除
    ''' </summary>
    ''' <param name="library"></param>
    ''' <param name="intoCutoff">相对相应强度的删除阈值, 值范围为``[0, 1]``</param>
    ''' <returns></returns>
    <Extension>
    Public Function Trim(ByRef library As LibraryMatrix, intoCutoff#) As LibraryMatrix
        library = library / library.Max
        If intoCutoff > 0 Then
            library = library(library!intensity >= intoCutoff)
        End If
        library = library * 100

        Return library
    End Function

    ''' <summary>
    ''' 主要是针对AB5600设备的数据, 将相同的信号强度的杂峰碎片都删除
    ''' </summary>
    ''' <param name="ms2"></param>
    ''' <returns></returns>
    <Extension>
    Public Function TrimBaseline(ms2 As IEnumerable(Of ms2)) As ms2()
        Dim intoGroups = ms2.GroupBy(Function(m) m.intensity, Function(a, b) std.Abs(a - b) <= 0.00001)
        Dim result As ms2() = intoGroups _
            .Where(Function(i) i.Length = 1) _
            .Select(Function(g) g.First) _
            .ToArray

        Return result
    End Function

    ''' <summary>
    ''' 根据保留时间来计算出保留指数
    ''' </summary>
    ''' <param name="rt"></param>
    ''' <param name="A"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    <Extension>
    Public Function RetentionIndex(rt As IRetentionTime, A As (rt#, ri#), B As (rt#, ri#)) As Double
        Dim rtScale = (rt.rt - A.rt) / (B.rt - A.rt)
        Dim riScale = (B.ri - A.ri) * rtScale
        Dim ri = A.ri + riScale

        Return ri
    End Function

    ''' <summary>
    ''' Kovats retention index
    ''' </summary>
    ''' <param name="small_n"></param>
    ''' <param name="large_N"></param>
    ''' <param name="ti"></param>
    ''' <param name="t_smalln"></param>
    ''' <param name="t_largeN"></param>
    ''' <returns></returns>
    Public Function KovatsRI(small_n As Integer, large_N As Integer, ti As Double, t_smalln As Double, t_largeN As Double) As Double
        Return 100 * (small_n + (large_N - small_n) * (std.Log(ti) - std.Log(t_smalln)) / (std.Log(t_largeN) - std.Log(t_smalln)))
    End Function

    ''' <summary>
    ''' 根据保留时间来计算出保留指数
    ''' </summary>
    ''' <param name="rt"></param>
    ''' <param name="A"></param>
    ''' <param name="B"></param>
    ''' <returns></returns>
    <Extension>
    Public Function RetentionIndex(Of RI As {IRetentionTime, IRetentionIndex})(rt As IRetentionTime, A As RI, B As RI) As Double
        Dim rtScale = (rt.rt - A.rt) / (B.rt - A.rt)
        Dim riScale = (B.RI - A.RI) * rtScale
        Dim retention_index = A.RI + riScale

        Return retention_index
    End Function

    ''' <summary>
    ''' 在一定的误差范围内按照m/z对碎片进行分组操作，并取出该分组内的信号响应值最大值作为该分组的信号响应
    ''' </summary>
    ''' <param name="mz"></param>
    ''' <param name="tolerance"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GroupByMz(mz As IEnumerable(Of ms1_scan), Optional tolerance As Tolerance = Nothing) As ms1_scan()
        Return ms1_scan.GroupByMz(mz, tolerance)
    End Function

    ''' <summary>
    ''' 这个拓展适用于``GC/MS``的标准品图谱，当然也适用于LC/MS的时间窗口采样结果
    ''' </summary>
    ''' <param name="fragments"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateLibraryMatrix(fragments As IEnumerable(Of ms1_scan), Optional name$ = "GC/MS Mass Scan") As LibraryMatrix
        Dim ms2 = fragments _
            .SafeQuery _
            .Select(Function(scan)
                        Return New ms2 With {
                            .mz = scan.mz,
                            .intensity = scan.intensity
                        }
                    End Function) _
            .ToArray

        Return New LibraryMatrix With {
            .ms2 = ms2,
            .name = name
        }
    End Function
End Module
