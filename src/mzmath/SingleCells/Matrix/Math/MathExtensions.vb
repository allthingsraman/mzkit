﻿Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.SingleCells.Deconvolute
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.SIMD

Namespace MatrixMath

    Public Module MathExtensions

        ''' <summary>
        ''' normalize of each spot/cell by total peak sum.
        ''' </summary>
        ''' <param name="m"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TotalPeakSumNormalization(m As MzMatrix, Optional scale As Double = 1000000.0) As MzMatrix
            Dim norm As New List(Of PixelData)

            For Each spot As PixelData In m.matrix
                Dim sum_val As Double = Aggregate into As Double In spot.intensity Into Sum(into)
                Dim norm_vec As Double() = Multiply.f64_scalar_op_multiply_f64(scale, Divide.f64_op_divide_f64_scalar(spot.intensity, sum_val))
                Dim norm_spot As New PixelData With {
                    .intensity = norm_vec,
                    .label = spot.label,
                    .X = spot.X,
                    .Y = spot.Y,
                    .Z = spot.Z
                }

                Call norm.Add(norm_spot)
            Next

            Return New MzMatrix With {
                .matrix = norm.ToArray,
                .matrixType = m.matrixType,
                .mz = m.mz,
                .mzmax = m.mzmax,
                .mzmin = m.mzmin,
                .tolerance = m.tolerance
            }
        End Function

        <Extension>
        Public Function RSD(m As MzMatrix) As Double()
            Dim rsd_vec As Double() = New Double(m.featureSize - 1) {}

            For i As Integer = 0 To rsd_vec.Length - 1
                Dim offset As Integer = i
                Dim col As Double() = (From cell As PixelData In m.matrix Select cell(offset)).ToArray

                rsd_vec(i) = col.RSD
            Next

            Return rsd_vec
        End Function
    End Module
End Namespace