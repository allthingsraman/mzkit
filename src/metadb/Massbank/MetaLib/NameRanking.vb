﻿Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges

Namespace MetaLib

    Public Module NameRanking

        ReadOnly empty_symbols As Index(Of String) = {".", "_", "?"}
        ReadOnly symbols As Char() = {"-", "/", "\", ":", "<", ">", "?", "(", ")", "[", "]", "{", "}", "|", ";", ",", "'", """"c, "."}

        Public Function Score(name As String) As Double
            If name.StringEmpty(testEmptyFactor:=True) OrElse name Like empty_symbols Then
                Return -1
            End If

            Dim eval As Double

            If name.Length < 3 Then
                eval = 1
            ElseIf name.Length < 32 Then
                eval = 10
            Else
                eval = 3
            End If

            Dim count As Integer = Aggregate c As Char
                                   In symbols
                                   Into Sum(name.Count(c))
            eval /= count

            Return eval
        End Function

        Public Function Ranking(names As IEnumerable(Of String)) As IEnumerable(Of NumericTagged(Of String))
            Return From name As String
                   In names
                   Let score As Double = NameRanking.Score(name)
                   Select out = New NumericTagged(Of String)(score, name)
                   Order By out.tag Descending
        End Function

    End Module
End Namespace