﻿#Region "Microsoft.VisualBasic::3eb3338bedd08649e6fa1c47e53c3161, mzkit\src\metadb\Chemoinformatics\Formula\FormulaSearch.vb"

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

    '   Total Lines: 232
    '    Code Lines: 154
    ' Comment Lines: 36
    '   Blank Lines: 42
    '     File Size: 9.85 KB


    '     Class FormulaSearch
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ConstructAndVerifyCompoundWork, PPM, reorderCandidateElements, (+2 Overloads) SearchByExactMass, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace Formula

    Public Class FormulaSearch

        ReadOnly opts As SearchOption
        ReadOnly elements As Dictionary(Of String, Element)
        ReadOnly progressReport As Action(Of String)
        ReadOnly candidateElements As ElementSearchCandiate()
        ReadOnly enableHCRatioCheck As Boolean = True

        Sub New(opts As SearchOption, Optional progress As Action(Of String) = Nothing)
            Me.opts = opts
            Me.progressReport = progress
            Me.elements = Element.MemoryLoadElements
            Me.candidateElements = reorderCandidateElements(enableHCRatioCheck)
        End Sub

        ''' <summary>
        ''' 为了方便计算HC比例以及加速计算，在这里总是将C放在第一位，H放在第二位
        ''' </summary>
        ''' <returns></returns>
        Private Function reorderCandidateElements(ByRef enableHCRatioCheck As Boolean) As ElementSearchCandiate()
            Dim list As ElementSearchCandiate() = opts.candidateElements.ToArray
            Dim C As Integer = which(list.Select(Function(e) e.Element = "C")).DefaultFirst(-1)
            Dim H As Integer = which(list.Select(Function(e) e.Element = "H")).DefaultFirst(-1)
            Dim reorders As New List(Of ElementSearchCandiate)

            If C > -1 Then
                reorders.Add(list(C))
            End If
            If H > -1 Then
                reorders.Add(list(H))
            End If

            enableHCRatioCheck = (C > -1) And (H > -1)

            For i As Integer = 0 To list.Length - 1
                If i <> C AndAlso i <> H Then
                    reorders.Add(list(i))
                End If
            Next

            Return reorders.ToArray
        End Function

        Public Iterator Function SearchByExactMass(exact_mass As Double,
                                                   Optional doVerify As Boolean = True,
                                                   Optional cancel As Value(Of Boolean) = Nothing) As IEnumerable(Of FormulaComposition)

            Dim elements As New Stack(Of ElementSearchCandiate)(candidateElements.AsEnumerable.Reverse)
            Dim seed As New FormulaComposition(New Dictionary(Of String, Integer), "")

            If cancel Is Nothing Then
                cancel = False
            End If

            Dim chargeMin As Double = opts.chargeRange.Min
            Dim chargeMax As Double = opts.chargeRange.Max

            For Each formula As FormulaComposition In SearchByExactMass(exact_mass, seed, elements, cancel, doVerify:=doVerify)
                If doVerify Then
                    Dim counts As New ElementNumType(formula)
                    Dim checked As Boolean = False

                    If ConstructAndVerifyCompoundWork(counts) Then
                        ' formula.charge = FormalCharge.CorrectChargeEmpirical(formula.charge, counts)
                        formula.charge = FormalCharge.EvaluateCharge(formula)

                        If formula.charge >= chargeMin AndAlso formula.charge <= chargeMax Then
                            checked = True
                        End If
                    End If

                    If Not checked Then
                        Continue For
                    ElseIf Not SevenGoldenRulesCheck.Check(formula, True, CoverRange.CommonRange, True) Then
                    End If
                End If

                If Not progressReport Is Nothing Then
                    Call progressReport($"find {formula} with tolerance error {formula.ppm} ppm!")
                End If

                formula.massdiff = std.Abs(formula.ExactMass - exact_mass)

                Yield formula
            Next
        End Function

        Private Shared Function ConstructAndVerifyCompoundWork(elementNum As ElementNumType) As Boolean
            Dim maxH As Integer = 0

            ' Compute maximum number of hydrogens
            If elementNum.Si = 0 AndAlso
                elementNum.C = 0 AndAlso
                elementNum.N = 0 AndAlso
                elementNum.P = 0 AndAlso
                elementNum.Other = 0 AndAlso
                (elementNum.O > 0 OrElse elementNum.S > 0) Then
                ' Only O and S
                maxH = 3
            Else
                ' Formula is: [#C*2 + 3 - (2 if N or P present)] + [#N + 3 - (1 if C or Si present)] + [#other elements * 4 + 3], where we assume other elements can have a coordination Number of up to 7
                If elementNum.C > 0 Or elementNum.Si > 0 Then
                    maxH += (elementNum.C + elementNum.Si) * 2 + 3
                    ' If udtElementNum.N > 0 Or udtElementNum.P > 0 Then maxh = maxh - 2
                End If

                If elementNum.N > 0 Or elementNum.P > 0 Then
                    maxH += (elementNum.N + elementNum.P) + 3
                    ' If udtElementNum.C > 0 Or udtElementNum.Si > 0 Then maxh = maxh - 1
                End If

                ' Correction for carbon contribution
                'If (udtElementNum.C > 0 Or udtElementNum.Si > 0) And (udtElementNum.N > 0 Or udtElementNum.P > 0) Then udtElementNum.H = udtElementNum.H - 2

                ' Correction for nitrogen contribution
                'If (udtElementNum.N > 0 Or udtElementNum.P > 0) And (udtElementNum.C > 0 Or udtElementNum.Si > 0) Then udtElementNum.H = udtElementNum.H - 1

                ' Combine the above two commented out if's to obtain:
                If (elementNum.N > 0 Or elementNum.P > 0) And (elementNum.C > 0 Or elementNum.Si > 0) Then
                    maxH = maxH - 3
                End If

                If elementNum.Other > 0 Then
                    maxH += elementNum.Other * 4 + 3
                End If
            End If

            ' correct for if H only
            If maxH < 3 Then
                maxH = 3
            End If

            ' correct for halogens
            maxH = maxH -
                elementNum.F -
                elementNum.Cl -
                elementNum.Br -
                elementNum.I

            ' correct for negative udtElementNum.H
            If maxH < 0 Then
                maxH = 0
            End If

            ' Verify H's
            Dim blnHOK = (elementNum.H <= maxH)

            ' Only proceed if hydrogens check out
            Return blnHOK
        End Function

        Private Iterator Function SearchByExactMass(exact_mass As Double,
                                                    parent As FormulaComposition,
                                                    candidates As Stack(Of ElementSearchCandiate),
                                                    cancel As Value(Of Boolean),
                                                    doVerify As Boolean) As IEnumerable(Of FormulaComposition)
            If candidates.Count = 0 Then
                Return
            End If

            Dim current As ElementSearchCandiate = candidates.Pop
            Dim isto As Double = elements(current.Element).isotopic

            For n As Integer = current.MinCount To current.MaxCount
                Dim formula As FormulaComposition = parent.AppendElement(current.Element, n)
                Dim ppm As Double = FormulaSearch.PPM(formula.ExactMass, exact_mass)

                If cancel.Value Then
                    Exit For
                End If

                If ppm <= opts.ppm Then
                    formula.ppm = ppm
                    ' populate current formula that match exact mass ppm condition
                    Yield formula
                ElseIf formula.ExactMass < exact_mass Then
                    If candidates.Count > 0 Then
                        ' 还可以再增加分子质量
                        ' stack必须要在这里进行重新初始化
                        ' 否则会被其他的循环所修改产生bug
                        For Each subtree In SearchByExactMass(
                            exact_mass:=exact_mass,
                            parent:=formula,
                            candidates:=New Stack(Of ElementSearchCandiate)(candidates.AsEnumerable.Reverse),
                            cancel:=cancel,
                            doVerify:=doVerify
                        )
                            Yield subtree
                        Next
                    End If
                End If
            Next
        End Function

        Public Overrides Function ToString() As String
            Return opts.ToString
        End Function

        ''' <summary>
        ''' 分子量差值
        ''' </summary>
        ''' <param name="measured#"></param>
        ''' <param name="actualValue#"></param>
        ''' <returns></returns>
        Public Overloads Shared Function PPM(measured#, actualValue#) As Double
            ' （测量值-实际分子量）/ 实际分子量
            ' |(实验数据 - 数据库结果)| / 实验数据 * 1000000
            Dim ppmd# = (std.Abs(measured - actualValue) / actualValue) * 1000000

            If ppmd < 0 Then
                ' 计算溢出了
                Return 10000000000
            End If

            Return ppmd
        End Function
    End Class
End Namespace
