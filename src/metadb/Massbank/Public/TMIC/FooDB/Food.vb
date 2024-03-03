﻿#Region "Microsoft.VisualBasic::8e83428c6064d2caedd7f04aa17eaacf, mzkit\src\metadb\FooDB\FooDB\Food.vb"

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

    '   Total Lines: 20
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 695 B


    ' Class Food
    ' 
    '     Properties: category, description, food_group, food_subgroup, food_type
    '                 id, itis_id, legacy_id, name, name_scientific
    '                 ncbi_taxonomy_id, public_id, wikipedia_id
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Public Class Food
    Public Property id As String
    Public Property name As String
    Public Property name_scientific As String
    Public Property description As String
    Public Property itis_id As String
    Public Property wikipedia_id As String
    Public Property legacy_id As String
    Public Property food_group As String
    Public Property food_subgroup As String
    Public Property food_type As String
    Public Property category As String
    Public Property ncbi_taxonomy_id As String
    Public Property public_id As String

    Public Overrides Function ToString() As String
        Return $"[{id}] {name}({name_scientific})"
    End Function

End Class
