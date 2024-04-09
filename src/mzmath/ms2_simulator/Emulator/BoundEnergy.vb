﻿#Region "Microsoft.VisualBasic::872137fc774456423e107a4de107584e, mzkit\src\mzmath\ms2_simulator\Emulator\BoundEnergy.vb"

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

'   Total Lines: 39
'    Code Lines: 28
' Comment Lines: 5
'   Blank Lines: 6
'     File Size: 1.17 KB


' Class BoundEnergy
' 
'     Properties: atom1, atom2, bound, bounds, comments
'                 H0
' 
'     Function: GetEnergyTable, ToString
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Public Class BoundEnergy

    ' Bonds,H0*,bounds.n,atom1,atom2,Comments

    <Column("Bonds")>
    Public Property bound As String
    ''' <summary>
    ''' Energy consumption
    ''' </summary>
    ''' <returns></returns>
    <Column("H0*")>
    Public Property H0 As Double
    <Column("bounds.n")>
    Public Property bounds As Integer
    Public Property atom1 As String
    Public Property atom2 As String
    Public Property comments As String

    Public Overrides Function ToString() As String
        If comments.StringEmpty Then
            Return $"Dim {bound} = {H0}"
        Else
            Return $"Dim {bound} = {H0}  ' ({comments})"
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function GetEnergyTable() As BoundEnergy()
        Return My.Resources.Standard_bond_energies _
            .LineTokens _
            .AsDataSource(Of BoundEnergy)(strict:=False) _
            .ToArray
    End Function

End Class
