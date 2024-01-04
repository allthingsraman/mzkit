﻿#Region "Microsoft.VisualBasic::5be677e82e504cdff8024ea81b7c364d, mzkit\src\metadb\Massbank\MetaLib\Models\ICompoundClass.vb"

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

    '   Total Lines: 15
    '    Code Lines: 9
    ' Comment Lines: 3
    '   Blank Lines: 3
    '     File Size: 406 B


    '     Interface ICompoundClass
    ' 
    '         Properties: [class], kingdom, molecular_framework, sub_class, super_class
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace MetaLib.Models

    ''' <summary>
    ''' 主要是取自HMDB数据库之中的代谢物分类信息
    ''' </summary>
    Public Interface ICompoundClass

        Property kingdom As String
        Property super_class As String
        Property [class] As String
        Property sub_class As String
        Property molecular_framework As String

    End Interface

    Public MustInherit Class ClassReader

        Public MustOverride Function GetClass(id As String) As CompoundClass

        ''' <summary>
        ''' try to enumerate all the reference id inside current 
        ''' class data index pool
        ''' </summary>
        ''' <returns>
        ''' a collection of the metabolite reference id, which could be used 
        ''' for get the compound class data via the <see cref="GetClass(String)"/> method.
        ''' </returns>
        Public MustOverride Function EnumerateId() As IEnumerable(Of String)

    End Class
End Namespace
