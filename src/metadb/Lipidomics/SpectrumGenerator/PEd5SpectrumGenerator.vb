﻿Imports CompMs.Common.Components
Imports CompMs.Common.DataObj.Property
Imports CompMs.Common.Enum
Imports CompMs.Common.FormulaGenerator.DataObj
Imports CompMs.Common.Interfaces
Imports System
Imports System.Collections.Generic
Imports System.Linq


Public Class PEd5SpectrumGenerator
        Implements ILipidSpectrumGenerator

        Private Shared ReadOnly C2H8NO4P As Double = {CarbonMass * 2, HydrogenMass * 8, NitrogenMass, OxygenMass * 4, PhosphorusMass}.Sum()

        Private Shared ReadOnly CH5N As Double = {CarbonMass * 1, HydrogenMass * 5, NitrogenMass}.Sum()

        Private Shared ReadOnly Gly_C As Double = {CarbonMass * 5, HydrogenMass * 7, NitrogenMass, OxygenMass * 4, PhosphorusMass, Hydrogen2Mass * 5}.Sum()

        Private Shared ReadOnly Gly_O As Double = {CarbonMass * 4, HydrogenMass * 7, NitrogenMass, OxygenMass * 5, PhosphorusMass, Hydrogen2Mass * 3}.Sum()

        Private Shared ReadOnly CH2 As Double = {HydrogenMass * 2, CarbonMass}.Sum()

        Private Shared ReadOnly CD2 As Double = {Hydrogen2Mass * 2, CarbonMass}.Sum()

        Private Shared ReadOnly H2O As Double = {HydrogenMass * 2, OxygenMass}.Sum()

        Public Sub New()
            spectrumGenerator = New SpectrumPeakGenerator()
        End Sub

        Public Sub New(spectrumGenerator As ISpectrumPeakGenerator)
            Me.spectrumGenerator = If(spectrumGenerator, CSharpImpl.__Throw(Of ISpectrumPeakGenerator)(New ArgumentNullException(NameOf(spectrumGenerator))))
        End Sub

        Private ReadOnly spectrumGenerator As ISpectrumPeakGenerator

        Public Function CanGenerate(lipid As ILipid, adduct As AdductIon) As Boolean Implements ILipidSpectrumGenerator.CanGenerate
            If lipid.LipidClass = LbmClass.PE_d5 Then
                If Equals(adduct.AdductIonName, "[M+H]+") OrElse Equals(adduct.AdductIonName, "[M+Na]+") Then
                    Return True
                End If
            End If
            Return False
        End Function

        Public Function Generate(lipid As Lipid, adduct As AdductIon, Optional molecule As IMoleculeProperty = Nothing) As IMSScanProperty Implements ILipidSpectrumGenerator.Generate
            Dim spectrum = New List(Of SpectrumPeak)()
            spectrum.AddRange(GetPESpectrum(lipid, adduct))
            If lipid.Description.Has(LipidDescription.Chain) Then
                spectrum.AddRange(GetAcylLevelSpectrum(lipid, lipid.Chains.GetDeterminedChains(), adduct))
                lipid.Chains.ApplyToChain(1, Sub(chain) spectrum.AddRange(GetAcylPositionSpectrum(lipid, chain, adduct)))
                spectrum.AddRange(GetAcylDoubleBondSpectrum(lipid, lipid.Chains.GetTypedChains(Of AcylChain)(), adduct))
                spectrum.AddRange(GetAcylDoubleBondSpectrum(lipid, lipid.Chains.GetTypedChains(Of AcylChain)(), adduct, nlMass:=C2H8NO4P))
            End If
            spectrum = spectrum.GroupBy(Function(spec) spec, comparer).[Select](Function(specs) New SpectrumPeak(Enumerable.First(specs).Mass, specs.Sum(Function(n) n.Intensity), String.Join(", ", specs.[Select](Function(spec) spec.Comment)), specs.Aggregate(SpectrumComment.none, Function(a, b) a Or b.SpectrumComment))).OrderBy(Function(peak) peak.Mass).ToList()
            Return CreateReference(lipid, adduct, spectrum, molecule)
        End Function

        Private Function CreateReference(lipid As ILipid, adduct As AdductIon, spectrum As List(Of SpectrumPeak), molecule As IMoleculeProperty) As MoleculeMsReference
            Return New MoleculeMsReference With {
    .PrecursorMz = adduct.ConvertToMz(lipid.Mass),
    .IonMode = adduct.IonMode,
    .Spectrum = spectrum,
    .Name = lipid.Name,
    .Formula = molecule?.Formula,
    .Ontology = molecule?.Ontology,
    .SMILES = molecule?.SMILES,
    .InChIKey = molecule?.InChIKey,
    .AdductType = adduct,
    .CompoundClass = lipid.LipidClass.ToString(),
    .Charge = adduct.ChargeNumber
}
        End Function

        Private Function GetPESpectrum(lipid As ILipid, adduct As AdductIon) As SpectrumPeak()
            Dim spectrum = New List(Of SpectrumPeak) From {
    New SpectrumPeak(adduct.ConvertToMz(lipid.Mass), 999R, "Precursor") With {
        .SpectrumComment = SpectrumComment.precursor
    },
    New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - C2H8NO4P), 500R, "Precursor -C2H8NO4P") With {
        .SpectrumComment = SpectrumComment.metaboliteclass,
        .IsAbsolutelyRequiredFragmentForAnnotation = True
    },
    New SpectrumPeak(adduct.ConvertToMz(C2H8NO4P), 100R, "Header") With {
        .SpectrumComment = SpectrumComment.metaboliteclass
    },
    New SpectrumPeak(adduct.ConvertToMz(Gly_C), 400R, "Gly-C") With {
        .SpectrumComment = SpectrumComment.metaboliteclass
    },
    New SpectrumPeak(adduct.ConvertToMz(Gly_O), 300R, "Gly-O") With {
        .SpectrumComment = SpectrumComment.metaboliteclass
    },
    New SpectrumPeak(adduct.ConvertToMz(lipid.Mass) / 2, 100R, "[Precursor]2+") With {
        .SpectrumComment = SpectrumComment.metaboliteclass
    }
}
            If Equals(adduct.AdductIonName, "[M+Na]+") Then
                spectrum.AddRange({New SpectrumPeak(lipid.Mass - C2H8NO4P + ProtonMass, 250R, "Precursor -C2H8NO4P") With {
.SpectrumComment = SpectrumComment.metaboliteclass
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - CH5N + HydrogenMass), 100R, "Precursor -CH4N") With {
.SpectrumComment = SpectrumComment.metaboliteclass
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - CH5N - CarbonMass), 300R, "Precursor -C2H5N") With {
.SpectrumComment = SpectrumComment.metaboliteclass
}})
            End If

            Return spectrum.ToArray()
        End Function

        Private Function GetAcylDoubleBondSpectrum(lipid As ILipid, acylChains As IEnumerable(Of AcylChain), adduct As AdductIon, Optional nlMass As Double = 0.0) As IEnumerable(Of SpectrumPeak)
            Return acylChains.SelectMany(Function(acylChain) spectrumGenerator.GetAcylDoubleBondSpectrum(lipid, acylChain, adduct, nlMass, 30R))
        End Function

        Private Function GetAcylLevelSpectrum(lipid As ILipid, acylChains As IEnumerable(Of IChain), adduct As AdductIon) As IEnumerable(Of SpectrumPeak)
            Return acylChains.SelectMany(Function(acylChain) GetAcylLevelSpectrum(lipid, acylChain, adduct))
        End Function

        Private Function GetAcylLevelSpectrum(lipid As ILipid, acylChain As IChain, adduct As AdductIon) As SpectrumPeak()
            Dim chainMass = acylChain.Mass - HydrogenMass
            Dim spectrum = New List(Of SpectrumPeak)()
            If Equals(adduct.AdductIonName, "[M+H]+") Then
                spectrum.AddRange({New SpectrumPeak(chainMass + ProtonMass, 100R, $"{acylChain} acyl+") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass), 100R, $"-{acylChain}") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - C2H8NO4P), 100R, $"-Header -{acylChain}") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - OxygenMass - HydrogenMass), 100R, $"-{acylChain}-O") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - C2H8NO4P - H2O), 100R, $"-Header -{acylChain}-O") With {
.SpectrumComment = SpectrumComment.acylchain
}})
            ElseIf Equals(adduct.AdductIonName, "[M+Na]+") Then
                spectrum.AddRange({New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - HydrogenMass * 2), 100R, $"-{acylChain}") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - H2O), 100R, $"-{acylChain}-O") With {
.SpectrumComment = SpectrumComment.acylchain
}})
            End If
            Return spectrum.ToArray()
        End Function

        Private Function GetAcylPositionSpectrum(lipid As ILipid, acylChain As IChain, adduct As AdductIon) As SpectrumPeak()
            Dim chainMass = acylChain.Mass
            Dim spectrum = New List(Of SpectrumPeak) From {
    New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - OxygenMass - CD2), 100R, "-CD2(Sn1)") With {
        .SpectrumComment = SpectrumComment.snposition
    }
}
            If Equals(adduct.AdductIonName, "[M+H]+") Then
                spectrum.AddRange({New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - C2H8NO4P - OxygenMass - CD2), 100R, "-Header -CD2(Sn1)") With {
.SpectrumComment = SpectrumComment.snposition
}})
            End If
            Return spectrum.ToArray()
        End Function


        Private Shared ReadOnly comparer As IEqualityComparer(Of SpectrumPeak) = New SpectrumEqualityComparer()

End Class
