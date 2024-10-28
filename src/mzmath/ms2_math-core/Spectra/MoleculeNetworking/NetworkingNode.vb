﻿#Region "Microsoft.VisualBasic::2804167243a4e5e3dfdb0144f672c0b0, mzmath\ms2_math-core\Spectra\MoleculeNetworking\NetworkingNode.vb"

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

    '   Total Lines: 102
    '    Code Lines: 64 (62.75%)
    ' Comment Lines: 22 (21.57%)
    '    - Xml Docs: 86.36%
    ' 
    '   Blank Lines: 16 (15.69%)
    '     File Size: 3.94 KB


    '     Class NetworkingNode
    ' 
    '         Properties: members, mz, referenceId, representation, size
    ' 
    '         Function: (+2 Overloads) Create, CreateReferenceId, ToString, UnionRepresentative
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.Linq

Namespace Spectra.MoleculeNetworking

    Public Class NetworkingNode

        Public Property representation As LibraryMatrix

        Public Property members As PeakMs2()
        Public Property mz As Double

        ''' <summary>
        ''' get collection size of the <see cref="members"/> in current network node
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
            Get
                Return members.TryCount
            End Get
        End Property

        ''' <summary>
        ''' get the reference <see cref="LibraryMatrix.name"/> from the <see cref="representation"/>
        ''' spectrum object as the reference id of current network node
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property referenceId As String
            Get
                Return representation.name
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return referenceId
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="tolerance">ms2 tolerance</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Create(parentIon As Double, raw As SpectrumCluster, tolerance As Tolerance, cutoff As LowAbundanceTrimming) As NetworkingNode
            Return Create(parentIon, raw.cluster, tolerance, cutoff)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="tolerance">ms2 tolerance</param>
        ''' <returns></returns>
        Public Shared Function Create(parentIon As Double, raw As PeakMs2(), tolerance As Tolerance, cutoff As LowAbundanceTrimming) As NetworkingNode
            Dim ions As PeakMs2() = raw _
                .Select(Function(a)
                            Dim maxInto = a.mzInto.Select(Function(x) x.intensity).Max

                            For i As Integer = 0 To a.mzInto.Length - 1
                                a.mzInto(i).intensity = a.mzInto(i).intensity / maxInto
                            Next

                            Return a
                        End Function) _
                .ToArray
            Dim nodeData As LibraryMatrix = unionRepresentative(ions, tolerance, cutoff)

            Return New NetworkingNode With {
                .mz = parentIon,
                .members = ions,
                .representation = nodeData
            }
        End Function

        Public Shared Function UnionRepresentative(ions As PeakMs2(), tolerance As Tolerance, cutoff As LowAbundanceTrimming) As LibraryMatrix
            Dim represent As LibraryMatrix = ions.SpectrumSum(centroid:=tolerance.GetErrorDalton)
            Dim matrix As ms2() = cutoff.Trim(represent.Array)

            Return New LibraryMatrix With {
                .centroid = True,
                .ms2 = matrix,
                .name = CreateReferenceId(matrix, topN:=3)
            }
        End Function

        Private Shared Function CreateReferenceId(matrix As ms2(), Optional topN As Integer = 3) As String
            Dim products As ms2() = matrix _
                .OrderByDescending(Function(a) a.intensity) _
                .Take(topN) _
                .ToArray
            Dim uid As String = products _
                .Select(Function(i) $"{i.mz.ToString("F3")}:{(i.intensity * 100).ToString("F0")}") _
                .JoinBy("/")

            Return uid
        End Function
    End Class
End Namespace
