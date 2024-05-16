﻿#Region "Microsoft.VisualBasic::18a75eaeaedc33e421b5ff81e18c0c62, metadb\Massbank\Public\TMIC\HMDB\MetaReference\BioSamples.vb"

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

    '   Total Lines: 80
    '    Code Lines: 57
    ' Comment Lines: 13
    '   Blank Lines: 10
    '     File Size: 3.02 KB


    '     Enum BioSamples
    ' 
    '         Feces, Urine
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module BioSampleExtensions
    ' 
    '         Function: GetSampleLocations, ParseSampleType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace TMIC.HMDB.Repository

    Public Enum BioSamples
        All = 0
        Bloods = 2
        MetabolicSystems = 4
        OtherFluids = 8
        Biomass = 16
        Feces
        Urine
    End Enum

    Public Module BioSampleExtensions

        ''' <summary>
        ''' enum integer to factor strings
        ''' </summary>
        ReadOnly samples As New Dictionary(Of BioSamples, String()) From {
            {BioSamples.Biomass, {"Feces"}},
            {BioSamples.Feces, {"Feces"}},
            {BioSamples.Bloods, {"Blood", "serum", "plasma", "blood plasma"}},
            {BioSamples.MetabolicSystems, {"Saliva", "Urine", "Sweat"}},
            {BioSamples.Urine, {"Urine"}},
            {BioSamples.OtherFluids, {"Cerebrospinal Fluid (CSF)", "Breast Milk"}}
        }

        ''' <summary>
        ''' 这个拓展函数接受使用``|``分隔的token来进行or运算
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ParseSampleType(type As String) As BioSamples
            Dim list = type.Split("|"c)
            Dim biosample As BioSamples = BioSamples.All
            Dim types As BioSamples() = Enums(Of BioSamples)()

            For Each typeName As String In list
                For Each key As BioSamples In types
                    If key.Description.TextEquals(typeName) Then
                        biosample = biosample Or key
                    End If
                Next
            Next

            Return biosample
        End Function

        ''' <summary>
        ''' convert the <see cref="BioSamples"/> flag integer to the factor strings
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetSampleLocations(type As BioSamples) As String()
            If type = BioSamples.All Then
                Return Enums(Of BioSamples) _
                    .Where(Function(key)
                               Return samples.ContainsKey(key)
                           End Function) _
                    .Select(Function(key) samples(key)) _
                    .IteratesALL _
                    .ToArray
            Else
                Dim list As New List(Of String)

                If type.HasFlag(BioSamples.Bloods) Then list += samples(BioSamples.Bloods)
                If type.HasFlag(BioSamples.Biomass) Then list += samples(BioSamples.Biomass)
                If type.HasFlag(BioSamples.MetabolicSystems) Then list += samples(BioSamples.MetabolicSystems)
                If type.HasFlag(BioSamples.OtherFluids) Then list += samples(BioSamples.OtherFluids)

                Return list
            End If
        End Function
    End Module
End Namespace
