﻿#Region "Microsoft.VisualBasic::6353b5e1970844729c8ab9db6637e084, metadb\Massbank\MetaLib\Models\ICompoundClass.vb"

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
    '    Code Lines: 34 (52.31%)
    ' Comment Lines: 22 (33.85%)
    '    - Xml Docs: 95.45%
    ' 
    '   Blank Lines: 9 (13.85%)
    '     File Size: 2.26 KB


    '     Class ClassReader
    ' 
    '         Function: GetLastNode, ToSet
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.BioDeep.Chemoinformatics

Namespace MetaLib.Models

    ''' <summary>
    ''' a helper for get <see cref="ICompoundClass"/> data
    ''' </summary>
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

        ''' <summary>
        ''' populate the class structure data in string array:
        ''' 
        ''' 1. <see cref="ICompoundClass.kingdom"/>
        ''' 2. <see cref="ICompoundClass.super_class"/>
        ''' 3. <see cref="ICompoundClass.class"/>
        ''' 4. <see cref="ICompoundClass.sub_class"/>
        ''' 5. <see cref="ICompoundClass.molecular_framework"/>
        ''' </summary>
        ''' <param name="c"></param>
        ''' <returns></returns>
        Public Shared Iterator Function ToSet(c As ICompoundClass) As IEnumerable(Of String)
            If Not c Is Nothing Then
                Yield c.kingdom
                Yield c.super_class
                Yield c.class
                Yield c.sub_class
                Yield c.molecular_framework
            End If
        End Function

        Public Shared Function GetLastNode(c As ICompoundClass) As String
            If c Is Nothing Then
                Return Nothing
            End If

            If Not c.molecular_framework.StringEmpty Then
                Return c.molecular_framework
            End If
            If Not c.sub_class.StringEmpty Then
                Return c.sub_class
            End If
            If Not c.class.StringEmpty Then
                Return c.class
            End If
            If Not c.super_class.StringEmpty Then
                Return c.super_class
            End If

            Return c.kingdom
        End Function

    End Class
End Namespace
