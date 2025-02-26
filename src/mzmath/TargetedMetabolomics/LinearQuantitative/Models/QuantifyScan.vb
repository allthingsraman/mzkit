﻿#Region "Microsoft.VisualBasic::e0d2e3edbfe139bc74e97065e68989db, mzmath\TargetedMetabolomics\LinearQuantitative\Models\QuantifyScan.vb"

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

    '   Total Lines: 26
    '    Code Lines: 10 (38.46%)
    ' Comment Lines: 11 (42.31%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (19.23%)
    '     File Size: 703 B


    '     Class QuantifyScan
    ' 
    '         Properties: quantify, rawX
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Framework.IO

Namespace LinearQuantitative

    ''' <summary>
    ''' peak data of a single sample file
    ''' </summary>
    Public Class QuantifyScan : Inherits DataFile

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
