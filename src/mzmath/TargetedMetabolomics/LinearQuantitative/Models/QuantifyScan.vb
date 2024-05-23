﻿#Region "Microsoft.VisualBasic::8e923a8f5b43461fa59f96fe0ac35a63, mzmath\TargetedMetabolomics\LinearQuantitative\Models\QuantifyScan.vb"

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

    '   Total Lines: 25
    '    Code Lines: 11 (44.00%)
    ' Comment Lines: 8 (32.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (24.00%)
    '     File Size: 658 B


    '     Class QuantifyScan
    ' 
    '         Properties: ionPeaks, quantify, rawX
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO

Namespace LinearQuantitative

    Public Class QuantifyScan

        Public Property ionPeaks As IonPeakTableRow()

        ''' <summary>
        ''' 定量结果
        ''' </summary>
        ''' <returns></returns>
        Public Property quantify As DataSet

        ''' <summary>
        ''' 原始的峰面积数据
        ''' </summary>
        ''' <returns></returns>
        Public Property rawX As DataSet

        Public Overrides Function ToString() As String
            Return $"[{quantify.ID}] {ionPeaks.Length} ions detected"
        End Function
    End Class
End Namespace
