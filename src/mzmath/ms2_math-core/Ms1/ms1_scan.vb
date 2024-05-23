﻿#Region "Microsoft.VisualBasic::72f270337d822bcacbf27738ab6ca5d4, mzmath\ms2_math-core\Ms1\ms1_scan.vb"

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

    '   Total Lines: 65
    '    Code Lines: 42 (64.62%)
    ' Comment Lines: 14 (21.54%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (13.85%)
    '     File Size: 2.59 KB


    ' Class ms1_scan
    ' 
    '     Properties: intensity, mz, scan_time
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: GroupByMz, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Math
Imports std = System.Math

''' <summary>
''' A TICPoint [mz, rt, intensity]
''' </summary>
Public Class ms1_scan : Implements IMs1, IMs1Scan, INumericKey, ITimeSignal

    <XmlAttribute> Public Property mz As Double Implements IMs1.mz, INumericKey.key
    <XmlAttribute> Public Property scan_time As Double Implements IMs1.rt, ITimeSignal.time
    <XmlAttribute> Public Property intensity As Double Implements IMs1Scan.intensity, ITimeSignal.intensity

    Sub New()
    End Sub

    Sub New(mz As Double, rt As Double, intensity As Double)
        _mz = mz
        _scan_time = rt
        _intensity = intensity
    End Sub

    Sub New(scan As IMs1Scan)
        Call Me.New(scan.mz, scan.rt, scan.intensity)
    End Sub

    Public Overrides Function ToString() As String
        Return $"{mz.ToString("F4")}@{std.Round(scan_time)} ({intensity})"
    End Function

    ''' <summary>
    ''' 按照``m/z``分组合并取出<see cref="intensity"/>最大值作为合并之后的结果
    ''' 这个函数是忽略掉<see cref="scan_time"/>的，即认为这些数据都是获取自同一个
    ''' <see cref="scan_time"/>下的结果
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="tolerance">默认是误差在0.3个道尔顿以内</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 类似于centroid
    ''' </remarks>
    Public Shared Function GroupByMz(data As IEnumerable(Of ms1_scan), Optional tolerance As Tolerance = Nothing) As ms1_scan()
        Dim mzGroups As NamedCollection(Of ms1_scan)() = data _
            .GroupBy(Function(scan) scan.mz, tolerance Or tolerance.DefaultTolerance) _
            .ToArray
        Dim scans As ms1_scan() = mzGroups _
            .Select(Function(group)
                        Dim mz = group.Select(Function(d) d.mz).Average
                        Dim into = group.Select(Function(d) d.intensity).Max
                        Dim time = group.Select(Function(d) d.scan_time).Average

                        Return New ms1_scan With {
                            .mz = mz,
                            .intensity = into,
                            .scan_time = time
                        }
                    End Function) _
            .ToArray

        Return scans
    End Function
End Class
