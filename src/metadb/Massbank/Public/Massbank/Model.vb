﻿#Region "Microsoft.VisualBasic::6dbd7fe1cd74134d59f495414a87d1aa, E:/mzkit/src/metadb/Massbank//Public/Massbank/Model.vb"

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

    '   Total Lines: 113
    '    Code Lines: 102
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 5.23 KB


    '     Module Model
    ' 
    '         Function: __getValue, __internalTable, CHROMATOGRAPHY, DBLinks, FOCUSED_ION
    '                   MASS_SPECTROMETRY
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.BioDeep.Chemistry.Massbank.DATA
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Massbank

    Public Module Model

        <Extension>
        Public Function DBLinks(record As Record) As Dictionary(Of String, String())
            If record.CH Is Nothing OrElse record.CH.LINK.IsNullOrEmpty Then
                Return New Dictionary(Of String, String())
            Else
                Dim links$() = record.CH.LINK
                Dim table = links.__internalTable
                Return table
            End If
        End Function

        <Extension>
        Public Function FOCUSED_ION(record As Record) As FOCUSED_ION
            If record.MS Is Nothing OrElse record.MS.FOCUSED_ION.IsNullOrEmpty Then
                Return New FOCUSED_ION
            Else
                Dim values$() = record.MS.FOCUSED_ION
                Dim table = values.__internalTable
                Dim getValue = table.__getValue
                Dim ion As New FOCUSED_ION With {
                .DERIVATIVE_FORM = getValue(NameOf(.DERIVATIVE_FORM)).FirstOrDefault,
                .DERIVATIVE_MASS = getValue(NameOf(.DERIVATIVE_MASS)).FirstOrDefault,
                .DERIVATIVE_TYPE = getValue(NameOf(.DERIVATIVE_TYPE)).FirstOrDefault,
                .PRECURSOR_TYPE = getValue(NameOf(.PRECURSOR_TYPE)).FirstOrDefault,
                .PRECURSOR_MZ = getValue("PRECURSOR_M/Z").FirstOrDefault
            }

                Return ion
            End If
        End Function

        <Extension>
        Public Function CHROMATOGRAPHY(record As Record) As CHROMATOGRAPHY
            If record.AC Is Nothing OrElse record.AC.CHROMATOGRAPHY.IsNullOrEmpty Then
                Return New CHROMATOGRAPHY
            Else
                Dim values$() = record.AC.CHROMATOGRAPHY
                Dim table = values.__internalTable
                Dim getValue = table.__getValue
                Dim chr As New CHROMATOGRAPHY With {
                .COLUMN_NAME = getValue(NameOf(.COLUMN_NAME)).FirstOrDefault,
                .COLUMN_TEMPERATURE = getValue(NameOf(.COLUMN_TEMPERATURE)).FirstOrDefault,
                .FLOW_GRADIENT = getValue(NameOf(.FLOW_GRADIENT)).FirstOrDefault,
                .FLOW_RATE = getValue(NameOf(.FLOW_RATE)).FirstOrDefault,
                .RETENTION_TIME = getValue(NameOf(.RETENTION_TIME)).FirstOrDefault,
                .SAMPLING_CONE = getValue(NameOf(.SAMPLING_CONE)).FirstOrDefault,
                .SOLVENT = getValue(NameOf(.SOLVENT))
            }

                Return chr
            End If
        End Function

        <Extension>
        Public Function MASS_SPECTROMETRY(record As Record) As MASS_SPECTROMETRY
            If record.AC Is Nothing OrElse record.AC.MASS_SPECTROMETRY.IsNullOrEmpty Then
                Return New MASS_SPECTROMETRY
            Else
                Dim values$() = record.AC.MASS_SPECTROMETRY
                Dim table = values.__internalTable
                Dim getValue = table.__getValue
                Dim ms As New MASS_SPECTROMETRY With {
                .COLLISION_ENERGY = getValue(NameOf(.COLLISION_ENERGY)).FirstOrDefault,
                .DATAFORMAT = getValue(NameOf(.DATAFORMAT)).FirstOrDefault,
                .DESOLVATION_GAS_FLOW = getValue(NameOf(.DESOLVATION_GAS_FLOW)).FirstOrDefault,
                .DESOLVATION_TEMPERATURE = getValue(NameOf(.DESOLVATION_TEMPERATURE)).FirstOrDefault,
                .FRAGMENTATION_METHOD = getValue(NameOf(.FRAGMENTATION_METHOD)).FirstOrDefault,
                .IONIZATION = getValue(NameOf(.IONIZATION)).FirstOrDefault,
                .ION_MODE = getValue(NameOf(.ION_MODE)).FirstOrDefault,
                .MS_TYPE = getValue(NameOf(.MS_TYPE)).FirstOrDefault,
                .SCANNING = getValue(NameOf(.SCANNING)).FirstOrDefault,
                .SOURCE_TEMPERATURE = getValue(NameOf(.SOURCE_TEMPERATURE)).FirstOrDefault
            }

                Return ms
            End If
        End Function

        <Extension>
        Private Function __getValue(table As Dictionary(Of String, String())) As Func(Of String, String())
            Return Function(key$)
                       If table.ContainsKey(key) Then
                           Return table(key)
                       Else
                           Return {}
                       End If
                   End Function
        End Function

        <Extension>
        Private Function __internalTable(values$()) As Dictionary(Of String, String())
            Dim data As NamedValue(Of String)() = values _
                .Select(Function(l) l.GetTagValue(" ", trim:=True)) _
                .ToArray
            Dim table As Dictionary(Of String, String()) = data _
                .GroupBy(Function(k) k.Name) _
                .ToDictionary(Function(l) l.Key,
                              Function(l)
                                  Return l.Values
                              End Function)
            Return table
        End Function
    End Module
End Namespace
