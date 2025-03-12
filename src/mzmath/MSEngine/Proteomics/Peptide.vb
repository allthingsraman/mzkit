﻿#Region "Microsoft.VisualBasic::6eb43d602da9ffb951f50ac088683f58, mzmath\MSEngine\Proteomics\Peptide.vb"

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

    '   Total Lines: 85
    '    Code Lines: 60 (70.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 25 (29.41%)
    '     File Size: 3.39 KB


    ' Class Peptide
    ' 
    '     Properties: DatabaseOrigin, DatabaseOriginID, ExactMass, Formula, IsDecoy
    '                 IsProteinCterminal, IsProteinNterminal, MissedCleavages, ModifiedSequence, Position
    '                 ResidueCodeIndexToModificationIndex, SamePeptideNumberInSearchedProteins, Sequence, SequenceObj
    ' 
    '     Function: CountModifiedAminoAcids, GetSequenceObj
    ' 
    '     Sub: GenerateSequenceObj
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.BioDeep.Chemoinformatics.Formula
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports std = System.Math


Public Class Peptide

    Public Property DatabaseOrigin As String

    Public Property DatabaseOriginID As Integer

    Public ReadOnly Property Sequence As String

        Get
            If cacheSequence Is Nothing Then
                cacheSequence = If(SequenceObj.IsNullOrEmpty, String.Empty, String.Join("", SequenceObj.[Select](Function(n) n.OneLetter.ToString())))
            End If
            Return cacheSequence
        End Get
    End Property ' original amino acid sequence

    Private cacheSequence As String = Nothing


    Public ReadOnly Property ModifiedSequence As String
        Get
            Return If(SequenceObj.IsNullOrEmpty, String.Empty, String.Join("", SequenceObj.[Select](Function(n) n.Code())))
        End Get
    End Property

    Public Property Position As intRange

    Public Property ExactMass As Double

    Public ReadOnly Property Formula As Formula
        Get
            Return If(SequenceObj.IsNullOrEmpty, Nothing, PeptideCalc.CalculatePeptideFormula(SequenceObj))
        End Get
    End Property


    Public Property IsProteinNterminal As Boolean

    Public Property IsProteinCterminal As Boolean

    Public Property SequenceObj As List(Of AminoAcid) ' N -> C, including modified amino acid information


    Public Property IsDecoy As Boolean = False

    Public Property MissedCleavages As Integer = 0

    Public Property SamePeptideNumberInSearchedProteins As Integer = 0

    Public Property ResidueCodeIndexToModificationIndex As Dictionary(Of Integer, Integer) = New Dictionary(Of Integer, Integer)()

    Public Function CountModifiedAminoAcids() As Integer
        If SequenceObj Is Nothing Then Return 0
        Return SequenceObj.Where(Function(n) n.IsModified()).Count
    End Function

    Public Sub GenerateSequenceObj(proteinSeq As String, start As Integer, [end] As Integer, ResidueCodeIndexToModificationIndex As Dictionary(Of Integer, Integer), ID2Code As Dictionary(Of Integer, String), Code2AminoAcidObj As Dictionary(Of String, AminoAcid))
        SequenceObj = GetSequenceObj(proteinSeq, start, [end], ResidueCodeIndexToModificationIndex, ID2Code, Code2AminoAcidObj)
    End Sub

    Private Function GetSequenceObj(proteinSeq As String, start As Integer, [end] As Integer, ResidueCodeIndexToModificationIndex As Dictionary(Of Integer, Integer), iD2Code As Dictionary(Of Integer, String), code2AminoAcidObj As Dictionary(Of String, AminoAcid)) As List(Of AminoAcid)
        Dim sequence = New List(Of AminoAcid)()
        If std.Max(start, [end]) > proteinSeq.Length - 1 Then Return Nothing
        For i = start To [end]
            Dim oneleter = proteinSeq(i)
            If ResidueCodeIndexToModificationIndex.ContainsKey(i) Then
                Dim residueID = ResidueCodeIndexToModificationIndex(i)
                Dim residueCode = iD2Code(residueID)
                Dim aa = code2AminoAcidObj(residueCode)
                sequence.Add(aa)
            Else
                Dim residueCode = oneleter
                Dim aa = code2AminoAcidObj(residueCode.ToString())
                sequence.Add(aa)
            End If
        Next
        Return sequence
    End Function
End Class


