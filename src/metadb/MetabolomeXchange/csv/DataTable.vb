﻿#Region "Microsoft.VisualBasic::cc029b0d2eadd837792ad5174b14cc2a, mzkit\src\metadb\MetabolomeXchange\csv\DataTable.vb"

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

    '   Total Lines: 37
    '    Code Lines: 34
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 1.43 KB


    ' Class DataTable
    ' 
    '     Properties: [date], analysis, metabolites1, metabolites10, metabolites11
    '                 metabolites12, metabolites13, metabolites14, metabolites15, metabolites16
    '                 metabolites17, metabolites18, metabolites19, metabolites2, metabolites20
    '                 metabolites3, metabolites4, metabolites5, metabolites6, metabolites7
    '                 metabolites8, metabolites9, organism, organism_parts, platform
    '                 publications, submitter, title
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO

Public Class DataTable : Inherits EntityObject

    Public Property title As String
    Public Property [date] As Date
    Public Property submitter As String()
    Public Property publications As String()
    Public Property analysis As String
    Public Property platform As String
    Public Property organism As String()
    Public Property organism_parts As String()
    Public Property metabolites1 As String()
    Public Property metabolites2 As String()
    Public Property metabolites3 As String()
    Public Property metabolites4 As String()
    Public Property metabolites5 As String()
    Public Property metabolites6 As String()
    Public Property metabolites7 As String()
    Public Property metabolites8 As String()
    Public Property metabolites9 As String()
    Public Property metabolites10 As String()
    Public Property metabolites11 As String()
    Public Property metabolites12 As String()
    Public Property metabolites13 As String()
    Public Property metabolites14 As String()
    Public Property metabolites15 As String()
    Public Property metabolites16 As String()
    Public Property metabolites17 As String()
    Public Property metabolites18 As String()
    Public Property metabolites19 As String()
    Public Property metabolites20 As String()

    Public Overrides Function ToString() As String
        Return title
    End Function
End Class
