﻿#Region "Microsoft.VisualBasic::aef5dcd0994acd988fd154e162c77d1c, metadb\Lipidomics\Annotation\LipidSearchMapper.vb"

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

    '   Total Lines: 128
    '    Code Lines: 79 (61.72%)
    ' Comment Lines: 25 (19.53%)
    '    - Xml Docs: 84.00%
    ' 
    '   Blank Lines: 24 (18.75%)
    '     File Size: 4.47 KB


    ' Class LipidSearchMapper
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: compares, emptyTree, (+2 Overloads) FindLipidReference
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1.Annotations
Imports BioNovoGene.BioDeep.Chemoinformatics.Lipidomics
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.TagData

''' <summary>
''' a helper module for mapping the lipidsearch name to lipidmaps id 
''' </summary>
Public Class LipidSearchMapper(Of T As {IExactMassProvider, IReadOnlyId, ICompoundNameProvider, IFormulaProvider})

    ReadOnly classes As New Dictionary(Of String, AVLClusterTree(Of LipidName))
    ReadOnly formula As New Dictionary(Of String, Dictionary(Of String, List(Of T)))

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="lipidmaps"></param>
    ''' <param name="getLipidName">ABBREVIATION</param>
    Sub New(lipidmaps As IEnumerable(Of T), getLipidName As Func(Of T, String))
        For Each lipid As T In lipidmaps
            Dim name_str As String = getLipidName(lipid)
            Dim name As LipidName = If(name_str.StringEmpty, Nothing, LipidName.ParseLipidName(name_str))

            If name Is Nothing Then
                Continue For
            Else
                name.id = lipid.Identity
            End If

            Dim class$ = name.className.ToLower
            Dim abbreviation As String = name.ToOverviewName

            If Not classes.ContainsKey([class]) Then
                classes.Add([class], emptyTree)
            End If
            If Not formula.ContainsKey([class]) Then
                formula.Add([class], New Dictionary(Of String, List(Of T)))
            End If

            If Not formula([class]).ContainsKey(abbreviation) Then
                formula([class]).Add(abbreviation, New List(Of T))
            End If

            Call classes([class]).Add(name)
            Call formula([class])(abbreviation).Add(lipid)
        Next
    End Sub

    Private Shared Function emptyTree() As AVLClusterTree(Of LipidName)
        Return New AVLClusterTree(Of LipidName)(AddressOf compares, Function(n) n.ToString)
    End Function

    Private Shared Function compares(a As LipidName, b As LipidName) As Integer
        If a.chains.Length > b.chains.Length Then
            Return 1
        ElseIf a.chains.Length < b.chains.Length Then
            Return -1
        End If

        Dim total1 = Aggregate c In a.chains Into Sum(c.carbons)
        Dim total2 = Aggregate c In b.chains Into Sum(c.carbons)

        If total1 > total2 Then
            Return 1
        ElseIf total1 < total2 Then
            Return -1
        End If

        Dim dbd1 = Aggregate c In a.chains Into Sum(c.doubleBonds)
        Dim dbd2 = Aggregate c In b.chains Into Sum(c.doubleBonds)

        If dbd1 > dbd2 Then
            Return 1
        ElseIf dbd1 < dbd2 Then
            Return -1
        End If

        Return 0
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns>
    ''' 1 for search by exact structure matches
    ''' 0 for lipid abbreviation name matches
    ''' </returns>
    Public Iterator Function FindLipidReference(name As LipidName) As IEnumerable(Of IntegerTagged(Of String))
        Dim class$ = name.className.ToLower

        [class] = [class].ToLower

        If Not classes.ContainsKey([class]) Then
            Return
        End If

        ' search by exact structure matches
        Dim query = classes([class]).Search(name).ToArray
        Dim abbreviation As String = name.ToOverviewName

        For Each lipid As LipidName In query
            Yield (1, lipid.id)
        Next

        Dim classNames = formula([class])

        If classNames.ContainsKey(abbreviation) Then
            For Each lipid As T In classNames(abbreviation)
                Yield (0, lipid.Identity)
            Next
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="class">the lipidsearch class name</param>
    ''' <param name="fattyAcid"></param>
    ''' <returns>
    ''' 1 for search by exact structure matches
    ''' 0 for lipid abbreviation name matches
    ''' </returns>
    Public Function FindLipidReference(class$, fattyAcid As String) As IEnumerable(Of IntegerTagged(Of String))
        Return FindLipidReference(LipidName.ParseLipidName([class] & fattyAcid))
    End Function

End Class
