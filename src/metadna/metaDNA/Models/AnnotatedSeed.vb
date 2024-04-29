﻿#Region "Microsoft.VisualBasic::5648683cf739ff453f1781f53dcfd504, E:/mzkit/src/metadna/metaDNA//Models/AnnotatedSeed.vb"

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

    '   Total Lines: 34
    '    Code Lines: 14
    ' Comment Lines: 14
    '   Blank Lines: 6
    '     File Size: 1022 B


    ' Class AnnotatedSeed
    ' 
    '     Properties: id, inferSize, kegg_id, parent, parentTrace
    '                 products
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

''' <summary>
''' 已经成功进行注释的代谢物信息(作为MetaDNA推断的种子)
''' </summary>
''' <remarks>
''' 
''' </remarks>
Public Class AnnotatedSeed : Implements INamedValue

    Public Property kegg_id As String
    Public Property id As String Implements INamedValue.Key

    ''' <summary>
    ''' current feature Ms1 of <see cref="id"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property parent As ms1_scan
    Public Property parentTrace As Double
    Public Property inferSize As Integer

    ''' <summary>
    ''' ``[lib_guid => spectrum]``
    ''' </summary>
    ''' <returns></returns>
    Public Property products As Dictionary(Of String, LibraryMatrix)

    Public Overrides Function ToString() As String
        Return $"{id}: {kegg_id}"
    End Function

End Class
