﻿Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.Xml
Imports Microsoft.VisualBasic.Math.Information
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

Namespace Spectra

    ''' <summary>
    ''' https://github.com/YuanyueLi/SpectralEntropy/blob/master/spectral_entropy/math_distance.py
    ''' </summary>
    Public Module SpectralEntropy

        ''' <summary>
        ''' Unweighted entropy distance:
        ''' 
        ''' ```
        ''' -\frac{2\times S_{PQ}-S_P-S_Q} {ln(4)}, S_I=\sum_{i} {I_i ln(I_i)}
        ''' ```
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="q"></param>
        ''' <returns></returns>
        Public Function unweighted_entropy_distance(p As Vector, q As Vector) As Double
            Dim merged As Vector = p + q
            Dim entropy_increase = 2 * merged.ShannonEntropy - p.ShannonEntropy - q.ShannonEntropy

            Return entropy_increase
        End Function

        ''' <summary>
        ''' Entropy distance:
        ''' 
        ''' ```
        ''' -\frac{2\times S_{PQ}^{'}-S_P^{'}-S_Q^{'}} {ln(4)}, S_I^{'}=\sum_{i} {I_i^{'} ln(I_i^{'})}, I^{'}=I^{w}, with\ w=0.25+S\times 0.5\ (S&lt;1.5)
        ''' ```
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="q"></param>
        ''' <returns></returns>
        Public Function entropy_distance(p As Vector, q As Vector) As Double
            p = WeightIntensityByEntropy(p)
            q = WeightIntensityByEntropy(q)

            Return unweighted_entropy_distance(p, q)
        End Function

        Public Function WeightIntensityByEntropy(x As Vector,
                                                 Optional WEIGHT_START As Double = 0.25,
                                                 Optional ENTROPY_CUTOFF As Double = 3) As Vector

            Dim weight_slope = (1 - WEIGHT_START) / ENTROPY_CUTOFF

            If x.Sum > 0 Then
                Dim entropy_x = x.ShannonEntropy

                If entropy_x < ENTROPY_CUTOFF Then
                    Dim weight = WEIGHT_START + weight_slope * entropy_x
                    x = x ^ weight
                    Dim x_sum = x.Sum
                    x = x / x_sum
                End If
            End If

            Return x
        End Function

        ''' <summary>
        ''' 因为计算熵需要概率向量总合为1，所以在这里应该是使用总离子做归一化，而非使用最大值做归一化
        ''' </summary>
        ''' <param name="ms"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function StandardizeSpectrum(ms As LibraryMatrix) As LibraryMatrix
            Return ms / ms.intensity.Sum
        End Function

        <Extension>
        Public Function calculate_entropy_similarity(alignment As SSM2MatrixFragment()) As Double
            Dim p As New Vector(From mzi In alignment Select mzi.query)
            Dim q As New Vector(From mzi In alignment Select mzi.ref)

            Return _entropy_similarity(p, q)
        End Function

        Public Function calculate_entropy_similarity(spectrum_a As ms2(), spectrum_b As ms2(), tolerance As Tolerance) As Double
            Return GlobalAlignment _
                .CreateAlignment(
                    query:=StandardizeSpectrum(New LibraryMatrix(spectrum_a)).ms2,
                    ref:=StandardizeSpectrum(New LibraryMatrix(spectrum_b)).ms2,
                    tolerance:=tolerance
                ) _
                .ToArray _
                .calculate_entropy_similarity
        End Function

        Public Function calculate_entropy_similarity(spectrum_a As ms2(), spectrum_b As ms2(), Optional ms2_da As Double = 0.3) As Double
            Return GlobalAlignment _
                .CreateAlignment(
                    query:=StandardizeSpectrum(New LibraryMatrix(spectrum_a)).ms2,
                    ref:=StandardizeSpectrum(New LibraryMatrix(spectrum_b)).ms2,
                    tolerance:=Tolerance.DeltaMass(ms2_da)
                ) _
                .ToArray _
                .calculate_entropy_similarity
        End Function

        Private Function _entropy_similarity(a As Vector, b As Vector) As Double
            Dim ia = _get_entropy_and_weighted_intensity(a)
            Dim ib = _get_entropy_and_weighted_intensity(b)
            Dim entropy_merged = (ia.intensity + ib.intensity).ShannonEntropy

            Return 1 - (2 * entropy_merged - ia.spectral_entropy - ib.spectral_entropy) / stdNum.Log(4)
        End Function

        Private Function _get_entropy_and_weighted_intensity(intensity As Vector) As (spectral_entropy As Double, intensity As Vector)
            Dim spectral_entropy = intensity.ShannonEntropy

            If spectral_entropy < 3 Then
                Dim weight = 0.25 + 0.25 * spectral_entropy
                Dim weighted_intensity = intensity ^ weight
                Dim intensity_sum = weighted_intensity.Sum

                weighted_intensity /= intensity_sum
                spectral_entropy = weighted_intensity.ShannonEntropy

                Return (spectral_entropy, weighted_intensity)
            Else
                Return (spectral_entropy, intensity)
            End If
        End Function
    End Module
End Namespace