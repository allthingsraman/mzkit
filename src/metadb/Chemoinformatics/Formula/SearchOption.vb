﻿#Region "Microsoft.VisualBasic::7c5d6a383eb3b149e3a55dabd2400869, mzkit\src\metadb\Chemoinformatics\Formula\SearchOption.vb"

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

'   Total Lines: 130
'    Code Lines: 110
' Comment Lines: 0
'   Blank Lines: 20
'     File Size: 5.03 KB


'     Class SearchOption
' 
'         Properties: candidateElements, chargeRange, ppm
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: AddElement, AdjustPpm, DefaultMetaboliteProfile, GeneralFlavone, NaturalProduct
'                   SmallMolecule, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Formula

    ''' <summary>
    ''' options for evaluate a formula for matches a given experiment mass
    ''' </summary>
    Public Class SearchOption

        Public ReadOnly Property candidateElements As ElementSearchCandiate()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _candidateElements.SafeQuery.ToArray
            End Get
        End Property

        ''' <summary>
        ''' the mass tolerance between the formula evaluated mass and the experiment mass
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ppm As Double
        Public ReadOnly Property chargeRange As IntRange

        ''' <summary>
        ''' number of the element types in current search option
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property candidateSize As Integer
            Get
                Return _candidateElements.TryCount
            End Get
        End Property

        ''' <summary>
        ''' a collection of the candidate elements
        ''' </summary>
        ReadOnly _candidateElements As List(Of ElementSearchCandiate)

        Sub New(minCharge As Integer, maxCharge As Integer, Optional ppm As Double = 30)
            Me._candidateElements = New List(Of ElementSearchCandiate)
            Me.ppm = ppm
            Me.chargeRange = New IntRange(minCharge, maxCharge)
        End Sub

        Sub New()
        End Sub

        Public Function AddElement(element As String, min As Integer, max As Integer) As SearchOption
            Call New ElementSearchCandiate With {
                .Element = element,
                .MaxCount = max,
                .MinCount = min
            }.DoCall(AddressOf _candidateElements.Add)

            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AddElement(element As String, range As Integer()) As SearchOption
            Return AddElement(element, range.Min, range.Max)
        End Function

        Public Function AdjustPpm(ppm As Double) As SearchOption
            _ppm = ppm
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function DefaultMetaboliteProfile() As SearchOption
            Return New SearchOption(-999999999, 999999999, ppm:=1) _
                .AddElement("C", 1, 30) _
                .AddElement("H", 0, 300) _
                .AddElement("N", 0, 30) _
                .AddElement("O", 0, 30) _
                .AddElement("P", 0, 30) _
                .AddElement("S", 0, 30)
        End Function

        Public Shared Function NaturalProduct(type As DNPOrWileyType, common As Boolean) As SearchOption
            If type = DNPOrWileyType.DNP Then
                Dim opts As New SearchOption(-999999999, 999999999, ppm:=1)
                opts.AddElement("C", 0, 66) _
                    .AddElement("H", 0, 126) _
                    .AddElement("N", 0, 25) _
                    .AddElement("O", 0, 27) _
                    .AddElement("P", 0, 6) _
                    .AddElement("S", 0, 8)

                If Not common Then
                    Return opts.AddElement("F", 0, 16) _
                        .AddElement("Cl", 0, 11) _
                        .AddElement("Br", 0, 8) _
                        .AddElement("Si", 0, 0)
                End If

                Return opts
            Else
                Dim opts As New SearchOption(-999999999, 999999999, ppm:=1)
                opts.AddElement("C", 0, 78) _
                    .AddElement("H", 0, 126) _
                    .AddElement("N", 0, 20) _
                    .AddElement("O", 0, 27) _
                    .AddElement("P", 0, 9) _
                    .AddElement("S", 0, 14)

                If Not common Then
                    Return opts.AddElement("F", 0, 34) _
                    .AddElement("Cl", 0, 12) _
                    .AddElement("Br", 0, 8) _
                    .AddElement("Si", 0, 14)
                End If

                Return opts
            End If
        End Function

        Public Shared Function GeneralFlavone() As SearchOption
            Return New SearchOption(-999999, 999999, ppm:=1).AddElement("C", 9, 50).AddElement("H", 4, 100).AddElement("O", 1, 30)
        End Function

        Public Shared Function SmallMolecule(type As DNPOrWileyType, common As Boolean) As SearchOption
            If type = DNPOrWileyType.DNP Then
                Dim opts As New SearchOption(-999999999, 999999999, ppm:=1)
                opts.AddElement("C", 0, 29) _
                    .AddElement("H", 0, 72) _
                    .AddElement("N", 0, 10) _
                    .AddElement("O", 0, 18) _
                    .AddElement("P", 0, 4) _
                    .AddElement("S", 0, 7)

                If Not common Then
                    Return opts _
                    .AddElement("F", 0, 15) _
                    .AddElement("Cl", 0, 8) _
                    .AddElement("Br", 0, 5) _
                    .AddElement("Si", 0, 0)
                End If

                Return opts
            Else
                Dim opts As New SearchOption(-999999999, 999999999, ppm:=1)
                opts.AddElement("C", 0, 39) _
                    .AddElement("H", 0, 72) _
                    .AddElement("N", 0, 20) _
                    .AddElement("O", 0, 20) _
                    .AddElement("P", 0, 9) _
                    .AddElement("S", 0, 10)

                If Not common Then
                    Return opts _
                    .AddElement("F", 0, 16) _
                    .AddElement("Cl", 0, 10) _
                    .AddElement("Br", 0, 4) _
                    .AddElement("Si", 0, 8)
                End If

                Return opts
            End If
        End Function
    End Class
End Namespace
