﻿Imports CompMs.Common.Components
Imports CompMs.Common.DataObj.Property
Imports CompMs.Common.Enum
Imports CompMs.Common.FormulaGenerator.DataObj
Imports CompMs.Common.Interfaces
Imports System
Imports System.Collections.Generic
Imports System.Linq

Public Class CLEidSpectrumGenerator
    Implements ILipidSpectrumGenerator
    'CL explain rule -> CL 2 chain(sn1,sn2)/2 chain(sn3,sn4)
    'CL sn1_sn2_sn3_sn4 (follow the rules of alignment) -- MolecularSpeciesLevelChains
    'CL sn1_sn2/sn3_sn4 -- MolecularSpeciesLevelChains <- cannot parsing now
    'CL sn1/sn2/sn3/sn4  -- PositionLevelChains 

    Private Shared ReadOnly C3H6O2 As Double = {CarbonMass * 3, HydrogenMass * 6, OxygenMass * 2}.Sum()

    Private Shared ReadOnly C3H3O2 As Double = {CarbonMass * 3, HydrogenMass * 3, OxygenMass * 2}.Sum()

    Private Shared ReadOnly CH2 As Double = {HydrogenMass * 2, CarbonMass}.Sum()

    Private Shared ReadOnly H2O As Double = {HydrogenMass * 2, OxygenMass}.Sum()

    Public Sub New()
        spectrumGenerator = New SpectrumPeakGenerator()
    End Sub

    Public Sub New(spectrumGenerator As ISpectrumPeakGenerator)
        Me.spectrumGenerator = If(spectrumGenerator, CSharpImpl.__Throw(Of ISpectrumPeakGenerator)(New ArgumentNullException(NameOf(spectrumGenerator))))
    End Sub

    Private ReadOnly spectrumGenerator As ISpectrumPeakGenerator

    Public Function CanGenerate(lipid As ILipid, adduct As AdductIon) As Boolean Implements ILipidSpectrumGenerator.CanGenerate
        If lipid.LipidClass = LbmClass.CL Then
            If Equals(adduct.AdductIonName, "[M+H]+") OrElse Equals(adduct.AdductIonName, "[M+NH4]+") Then
                Return True
            End If
        End If
        Return False
    End Function
    Public Function Generate(lipid As Lipid, adduct As AdductIon, Optional molecule As IMoleculeProperty = Nothing) As IMSScanProperty Implements ILipidSpectrumGenerator.Generate
        Dim nlMass = adduct.AdductIonAccurateMass - ProtonMass
        Dim spectrum = New List(Of SpectrumPeak)()
        spectrum.AddRange(GetCLSpectrum(lipid, adduct))
        'spectrum.AddRange(GetAcylDoubleBondSpectrum(lipid, lipid.Chains.GetTypedChains<AcylChain>().Where(c => c.DoubleBond.UnDecidedCount == 0 && c.Oxidized.UnDecidedCount == 0), adduct));
        Dim c1 As AcylChain = Nothing, c2 As AcylChain = Nothing, c3 As AcylChain = Nothing, c4 As AcylChain = Nothing

        If lipid.Description.Has(LipidDescription.Chain) Then
            spectrum.AddRange(GetAcylLevelSpectrum(lipid, lipid.Chains.GetDeterminedChains(), adduct))

            If CSharpImpl.__Assign(c1, TryCast(lipid.Chains.GetChainByPosition(1), AcylChain)) IsNot Nothing AndAlso CSharpImpl.__Assign(c2, TryCast(lipid.Chains.GetChainByPosition(2), AcylChain)) IsNot Nothing AndAlso CSharpImpl.__Assign(c3, TryCast(lipid.Chains.GetChainByPosition(3), AcylChain)) IsNot Nothing AndAlso CSharpImpl.__Assign(c4, TryCast(lipid.Chains.GetChainByPosition(4), AcylChain)) IsNot Nothing Then
                Dim sn1sn2 = {c1, c2}
                Dim sn3sn4mass = lipid.Mass - (c3.Mass + c4.Mass + C3H3O2 + HydrogenMass)
                spectrum.AddRange(GetAcylDoubleBondSpectrum(lipid, sn1sn2.Where(Function(c) c.DoubleBond.UnDecidedCount = 0 AndAlso c.Oxidized.UnDecidedCount = 0), adduct, sn3sn4mass + nlMass))
                spectrum.AddRange(GetAcylPositionSpectrum(lipid, c1, adduct, sn3sn4mass + nlMass))
                Dim sn3sn4 = {c3, c4}
                Dim sn1sn2mass = lipid.Mass - (c1.Mass + c2.Mass + C3H3O2 + HydrogenMass)
                spectrum.AddRange(GetAcylDoubleBondSpectrum(lipid, sn3sn4.Where(Function(c) c.DoubleBond.UnDecidedCount = 0 AndAlso c.Oxidized.UnDecidedCount = 0), adduct, sn1sn2mass + nlMass))
                spectrum.AddRange(GetAcylPositionSpectrum(lipid, c3, adduct, sn1sn2mass + nlMass))
                spectrum.AddRange(EidSpecificSpectrum(lipid, adduct, nlMass, 200.0R))
            End If
        End If
        spectrum = spectrum.GroupBy(Function(spec) spec, comparer).[Select](Function(specs) New SpectrumPeak(Enumerable.First(specs).Mass, specs.Sum(Function(n) n.Intensity), String.Join(", ", specs.[Select](Function(spec) spec.Comment)), specs.Aggregate(SpectrumComment.none, Function(a, b) a Or b.SpectrumComment))).OrderBy(Function(peak) peak.Mass).ToList()
        Return CreateReference(lipid, adduct, spectrum, molecule)
    End Function

    Private Function CreateReference(lipid As ILipid, adduct As AdductIon, spectrum As List(Of SpectrumPeak), molecule As IMoleculeProperty) As MoleculeMsReference
        Return New MoleculeMsReference With {
.PrecursorMz = adduct.ConvertToMz(lipid.Mass),
.IonMode = adduct.IonMode,
.spectrum = spectrum,
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
    Private Function GetCLSpectrum(lipid As ILipid, adduct As AdductIon) As SpectrumPeak()
        Dim adductmass = If(Equals(adduct.AdductIonName, "[M+NH4]+"), ProtonMass, adduct.AdductIonAccurateMass)
        Dim spectrum = New List(Of SpectrumPeak) From {
New SpectrumPeak(adduct.ConvertToMz(lipid.Mass), 999.0R, "Precursor") With {
    .SpectrumComment = SpectrumComment.precursor
},
New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - H2O), 999.0R, "Precursor -H2O") With {
    .SpectrumComment = SpectrumComment.metaboliteclass,
    .IsAbsolutelyRequiredFragmentForAnnotation = True
},
New SpectrumPeak(adduct.ConvertToMz(lipid.Mass) / 2, 150.0R, "[Precursor]2+") With {
    .SpectrumComment = SpectrumComment.precursor
},
New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - H2O) / 2, 150.0R, "[Precursor -H2O]2+") With {
    .SpectrumComment = SpectrumComment.metaboliteclass,
    .IsAbsolutelyRequiredFragmentForAnnotation = True
}
}
        'if (adduct.AdductIonName == "[M+NH4]+")
        '{
        '    spectrum.Add(
        '        new SpectrumPeak(lipid.Mass + MassDiffDictionary.ProtonMass, 200d, "[M+H]+") { SpectrumComment = SpectrumComment.metaboliteclass }
        '    );
        '}
        Return spectrum.ToArray()
    End Function

    Private Function GetAcylDoubleBondSpectrum(lipid As ILipid, acylChains As IEnumerable(Of AcylChain), adduct As AdductIon, Optional nlMass As Double = 0.0) As IEnumerable(Of SpectrumPeak)
        Return acylChains.SelectMany(Function(acylChain) spectrumGenerator.GetAcylDoubleBondSpectrum(lipid, acylChain, adduct, nlMass - HydrogenMass, 10.0R))
    End Function

    Private Function GetAcylLevelSpectrum(lipid As ILipid, acylChains As IEnumerable(Of IChain), adduct As AdductIon) As IEnumerable(Of SpectrumPeak)
        Dim lipidMass = lipid.Mass
        Dim adductmass = If(Equals(adduct.AdductIonName, "[M+NH4]+"), ProtonMass, adduct.AdductIonAccurateMass)
        Dim acylChainsArr = acylChains.ToArray()
        Dim sn1sn2mass = acylChainsArr(0).Mass + acylChainsArr(1).Mass + C3H3O2 + HydrogenMass
        Dim sn3sn4mass = acylChainsArr(2).Mass + acylChainsArr(3).Mass + C3H3O2 + HydrogenMass

        Dim spectrum = New List(Of SpectrumPeak) From {
New SpectrumPeak(lipidMass - sn1sn2mass + ProtonMass, 30.0R, $"[M-Sn1-Sn2-C3H3O2+H]+") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(lipidMass - sn3sn4mass + ProtonMass, 30.0R, $"[M-Sn3-Sn4-C3H3O2+H]+") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(lipidMass - sn1sn2mass + ProtonMass - H2O, 30.0R, $"[M-Sn1-Sn2-C3H3O2-H2O+H]+") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(lipidMass - sn3sn4mass + ProtonMass - H2O, 30.0R, $"[M-Sn3-Sn4-C3H3O2-H2O+H]+") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(sn1sn2mass + ProtonMass, 500.0R, $"[Sn1+Sn2+C3H3O2+H]+") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(sn3sn4mass + ProtonMass, 500.0R, $"[Sn3+Sn4+C3H3O2+H]+") With {
    .SpectrumComment = SpectrumComment.acylchain
}

}
            spectrum.AddRange(acylChains.SelectMany(Function(acylChain) GetAcylLevelSpectrum(lipid, acylChain, adduct)))

        Return spectrum.ToArray()
    End Function

    Private Function GetAcylLevelSpectrum(lipid As ILipid, acylChain As IChain, adduct As AdductIon) As SpectrumPeak()
        Dim lipidMass = lipid.Mass
        Dim chainMass = acylChain.Mass - HydrogenMass

        Dim spectrum = New List(Of SpectrumPeak) From {
New SpectrumPeak(chainMass + ProtonMass, 50.0R, $"{acylChain} acyl+") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(chainMass + C3H6O2 + ProtonMass, 100.0R, $"{acylChain} +C3H6O2") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(chainMass + C3H6O2 - OxygenMass + ProtonMass, 50.0R, $"{acylChain} +C3H6O") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - adduct.AdductIonAccurateMass + ProtonMass), 20.0R, $"-{acylChain}") With {
    .SpectrumComment = SpectrumComment.acylchain
},
New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - chainMass - adduct.AdductIonAccurateMass + ProtonMass - H2O), 20.0R, $"-{acylChain}- H2O") With {
    .SpectrumComment = SpectrumComment.acylchain
}
}

        Return spectrum.ToArray()
    End Function


    Private Function GetAcylPositionSpectrum(lipid As ILipid, acylChain As IChain, adduct As AdductIon, nlMass As Double) As SpectrumPeak()
        Dim lipidMass = lipid.Mass + adduct.AdductIonAccurateMass
        Dim chainMass = acylChain.Mass - HydrogenMass
        Dim spectrum = New List(Of SpectrumPeak) From {
New SpectrumPeak(lipidMass - chainMass - nlMass - CH2, 20.0R, "-CH2(Sn1)") With {
    .SpectrumComment = SpectrumComment.snposition
}
}
        Return spectrum.ToArray()
    End Function

    Private Shared Function EidSpecificSpectrum(lipid As Lipid, adduct As AdductIon, nlMass As Double, intensity As Double) As SpectrumPeak()
        Dim spectrum = New List(Of SpectrumPeak)()
        Dim c1 As IChain = Nothing, c2 As IChain = Nothing, c3 As IChain = Nothing, c4 As IChain = Nothing

        If CSharpImpl.__Assign(c1, TryCast(lipid.Chains.GetChainByPosition(1), IChain)) IsNot Nothing AndAlso CSharpImpl.__Assign(c2, TryCast(lipid.Chains.GetChainByPosition(2), IChain)) IsNot Nothing AndAlso CSharpImpl.__Assign(c3, TryCast(lipid.Chains.GetChainByPosition(3), IChain)) IsNot Nothing AndAlso CSharpImpl.__Assign(c4, TryCast(lipid.Chains.GetChainByPosition(4), IChain)) IsNot Nothing Then
            Dim sn1sn2mass = lipid.Mass - (c1.Mass + c2.Mass + C3H3O2 + HydrogenMass)
            Dim sn3sn4mass = lipid.Mass - (c3.Mass + c4.Mass + C3H3O2 + HydrogenMass)
            Dim sn1sn2 = {c1, c2}
            Dim sn3sn4 = {c3, c4}
            For Each chain In sn1sn2
                If chain.DoubleBond.Count = 0 OrElse chain.DoubleBond.UnDecidedCount > 0 Then Continue For
                If chain.DoubleBond.Count <= 3 Then
                    intensity = intensity * 0.5
                End If
                spectrum.AddRange(EidSpecificSpectrumGenerator.EidSpecificSpectrumGen(lipid, chain, adduct, sn3sn4mass + nlMass, intensity))
            Next
            For Each chain In sn3sn4
                If chain.DoubleBond.Count = 0 OrElse chain.DoubleBond.UnDecidedCount > 0 Then Continue For
                If chain.DoubleBond.Count <= 3 Then
                    intensity = intensity * 0.5
                End If
                spectrum.AddRange(EidSpecificSpectrumGenerator.EidSpecificSpectrumGen(lipid, chain, adduct, sn1sn2mass + nlMass, intensity))
            Next
        End If
        Return spectrum.ToArray()
    End Function

    Private Shared ReadOnly comparer As IEqualityComparer(Of SpectrumPeak) = New SpectrumEqualityComparer()

End Class