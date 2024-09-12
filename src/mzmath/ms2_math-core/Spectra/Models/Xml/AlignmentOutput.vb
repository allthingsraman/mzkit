﻿#Region "Microsoft.VisualBasic::c1ee83641b825ca2b4b533a2d1378790, mzmath\ms2_math-core\Spectra\Models\Xml\AlignmentOutput.vb"

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

    '   Total Lines: 91
    '    Code Lines: 67 (73.63%)
    ' Comment Lines: 8 (8.79%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (17.58%)
    '     File Size: 3.20 KB


    '     Class AlignmentOutput
    ' 
    '         Properties: alignments, cosine, entropy, forward, jaccard
    '                     mean, mirror, nhits, query, reference
    '                     reverse
    ' 
    '         Function: CreateLinearMatrix, GetAlignmentMirror, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace Spectra.Xml

    Public Class AlignmentOutput

        <XmlAttribute> Public Property forward As Double
        <XmlAttribute> Public Property reverse As Double
        <XmlAttribute> Public Property jaccard As Double
        <XmlAttribute> Public Property entropy As Double

        Public ReadOnly Property mirror As Double
            Get
                If _alignments.IsNullOrEmpty Then
                    Return 0.0
                Else
                    Dim nq As Integer = alignments.Where(Function(x) x.query > 0).Count
                    Dim nr As Integer = alignments.Where(Function(x) x.ref > 0).Count

                    Return jaccard * (std.Min(nq, nr) / std.Max(nq, nr))
                End If
            End Get
        End Property

        ''' <summary>
        ''' the min value of <see cref="forward"/> and <see cref="reverse"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property cosine As Double
            Get
                Return std.Min(forward, reverse)
            End Get
        End Property

        ''' <summary>
        ''' get score mean result
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property mean As Double
            Get
                Return {forward, reverse, jaccard, entropy}.Average
            End Get
        End Property

        Public Property query As Meta
        Public Property reference As Meta

        <XmlArray("alignments")>
        Public Property alignments As SSM2MatrixFragment()

        Public ReadOnly Property nhits As Integer
            Get
                If alignments Is Nothing OrElse alignments.Length = 0 Then
                    Return 0
                End If

                Return alignments _
                    .Where(Function(a)
                               If a.da = "NA" Then
                                   Return False
                               Else
                                   Return Not Double.Parse(a.da).IsNaNImaginary
                               End If
                           End Function) _
                    .Count
            End Get
        End Property

        Public ReadOnly Property alignment_str As String
            Get
                Return CreateLinearMatrix(alignments).JoinBy(" ")
            End Get
        End Property

        Public Iterator Function GetHitsMzPeaks() As IEnumerable(Of Double)
            For Each hit As SSM2MatrixFragment In alignments
                If hit.query > 0 AndAlso hit.ref > 0 Then
                    Yield hit.mz
                End If
            Next
        End Function

        ''' <summary>
        ''' construct a tuple of the spectrum data as the mirror alignment
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' the name of the generated <see cref="LibraryMatrix"/> object is generates from the metadata of query and reference.
        ''' </remarks>
        Public Function GetAlignmentMirror() As (query As LibraryMatrix, ref As LibraryMatrix)
            With New Ms2AlignMatrix(alignments)
                Dim q = .GetQueryMatrix.With(Sub(a) a.name = query?.id)
                Dim r = .GetReferenceMatrix.With(Sub(a) a.name = reference?.id)

                Return (q, r)
            End With
        End Function

        Public Overrides Function ToString() As String
            Return $"{query} vs {reference}"
        End Function

        Public Shared Iterator Function CreateLinearMatrix(matrix As IEnumerable(Of SSM2MatrixFragment)) As IEnumerable(Of String)
            For Each line As SSM2MatrixFragment In matrix
                Yield $"{line.mz.ToString("F4")}_{line.query.ToString("G4")}_{line.ref.ToString("G4")}"
            Next
        End Function

        Public Shared Iterator Function ParseAlignmentLinearMatrix(str As String) As IEnumerable(Of SSM2MatrixFragment)
            If Not str Is Nothing Then
                Dim peaks As String() = str.Split

                For Each ti As String In peaks
                    Dim tokens = ti.Split("_"c)
                    Dim mz = Val(tokens(0))
                    Dim query = Val(tokens(1))
                    Dim refer = Val(tokens(2))

                    Yield New SSM2MatrixFragment With {
                        .mz = mz,
                        .query = query,
                        .ref = refer
                    }
                Next
            End If
        End Function

        Public Shared Function ParseAlignment(str As String) As AlignmentOutput
            Return New AlignmentOutput With {
                .alignments = ParseAlignmentLinearMatrix(str).ToArray
            }
        End Function

    End Class
End Namespace
