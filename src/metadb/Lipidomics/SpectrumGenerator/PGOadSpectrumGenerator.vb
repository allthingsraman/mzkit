﻿Imports CompMs.Common.Components
Imports CompMs.Common.DataObj.Property
Imports CompMs.Common.Enum
Imports CompMs.Common.FormulaGenerator.DataObj
Imports CompMs.Common.Interfaces
Imports System
Imports System.Collections.Generic
Imports System.Linq


Public Class PGOadSpectrumGenerator
        Implements ILipidSpectrumGenerator
        Private Shared ReadOnly C3H9O6P As Double = {CarbonMass * 3, HydrogenMass * 9, OxygenMass * 6, PhosphorusMass}.Sum() ' PG Header
        Private Shared ReadOnly NH3 As Double = {HydrogenMass * 3, NitrogenMass}.Sum()
        Private Shared ReadOnly C3H6O5P As Double = {CarbonMass * 3, HydrogenMass * 6, OxygenMass * 5, PhosphorusMass}.Sum()
        Private Shared ReadOnly H2O As Double = {HydrogenMass * 2, OxygenMass}.Sum()
        Private Shared ReadOnly Electron As Double = 0.00054858026

        Private ReadOnly spectrumGenerator As IOadSpectrumPeakGenerator
        Public Sub New()
            spectrumGenerator = New OadSpectrumPeakGenerator()
        End Sub

        Public Sub New(spectrumGenerator As IOadSpectrumPeakGenerator)
            Me.spectrumGenerator = If(spectrumGenerator, CSharpImpl.__Throw(Of IOadSpectrumPeakGenerator)(New ArgumentNullException(NameOf(spectrumGenerator))))
        End Sub

        Public Function CanGenerate(lipid As ILipid, adduct As AdductIon) As Boolean Implements ILipidSpectrumGenerator.CanGenerate
            If Equals(adduct.AdductIonName, "[M+NH4]+") OrElse Equals(adduct.AdductIonName, "[M-H]-") Then
                Return True
            End If
            Return False
        End Function

        Public Function Generate(lipid As Lipid, adduct As AdductIon, Optional molecule As IMoleculeProperty = Nothing) As IMSScanProperty Implements ILipidSpectrumGenerator.Generate
            Dim abundance = 30
            Dim nlMass = If(adduct.IonMode = IonModes.Positive, C3H9O6P + NH3, 0.0)
            Dim spectrum = New List(Of SpectrumPeak)()
            spectrum.AddRange(GetPGOadSpectrum(lipid, adduct, nlMass))
            '"OAD02+O",
            '"OAD05",
            '"OAD06",
            '"OAD07",
            '"OAD08",
            '"OAD09",
            '"OAD10",
            '"OAD11",
            '"OAD12",
            '"OAD13",
            '"OAD15+O",
            '"OAD12+O+2H",
            '"OAD02+O",
            '"OAD05",
            '"OAD06",
            '"OAD07",
            '"OAD08",
            '"OAD09",
            '"OAD10",
            '"OAD11",
            '"OAD12",
            '"OAD13",
            '"OAD15+O",
            '"OAD16",
            '"OAD17",
            '"OAD01+H"
            Dim oadId = If(adduct.IonMode = IonModes.Positive, New String() {"OAD01", "OAD02", "OAD03", "OAD04", "OAD14", "OAD15", "OAD16", "OAD17", "OAD12+O", "OAD12+O+H", "OAD01+H"}, New String() {"OAD01", "OAD02", "OAD03", "OAD04", "OAD14", "OAD15", "OAD12+O", "OAD12+O+H", "OAD12+O+2H"})

            Dim plChains As PositionLevelChains = Nothing

            If CSharpImpl.__Assign(plChains, TryCast(lipid.Chains, PositionLevelChains)) IsNot Nothing Then
                For Each chain As AcylChain In lipid.Chains.GetDeterminedChains()
                    spectrum.AddRange(spectrumGenerator.GetAcylDoubleBondSpectrum(lipid, chain, adduct, nlMass, abundance, oadId))
                Next
            End If
            spectrum = spectrum.GroupBy(Function(spec) spec, comparer).[Select](Function(specs) New SpectrumPeak(Enumerable.First(specs).Mass, specs.Sum(Function(n) n.Intensity), String.Join(", ", specs.[Select](Function(spec) spec.Comment)), specs.Aggregate(SpectrumComment.none, Function(a, b) a Or b.SpectrumComment))).OrderBy(Function(peak) peak.Mass).ToList()
            Return CreateReference(lipid, adduct, spectrum, molecule)
        End Function

        Private Function GetPGOadSpectrum(lipid As Lipid, adduct As AdductIon, nlMass As Double) As SpectrumPeak()
            Dim spectrum = New List(Of SpectrumPeak)()
            Dim Chains As SeparatedChains = Nothing, Chains As SeparatedChains = Nothing

            If Equals(adduct.AdductIonName, "[M+NH4]+") Then
                spectrum.AddRange({New SpectrumPeak(adduct.ConvertToMz(lipid.Mass), 100R, "Precursor") With {
.SpectrumComment = SpectrumComment.precursor
}, New SpectrumPeak(lipid.Mass - C3H9O6P + ProtonMass, 999R, "Precursor -C3H9O6P") With {
.SpectrumComment = SpectrumComment.metaboliteclass,
.IsAbsolutelyRequiredFragmentForAnnotation = True
}})

                If CSharpImpl.__Assign(Chains, TryCast(lipid.Chains, SeparatedChains)) IsNot Nothing Then
                    For Each chain As AcylChain In lipid.Chains.GetDeterminedChains()
                        spectrum.AddRange({New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - nlMass - chain.Mass + HydrogenMass), 50R, $"-{chain}") With {
.SpectrumComment = SpectrumComment.acylchain
'new SpectrumPeak(adduct.ConvertToMz(chain.Mass - MassDiffDictionary.HydrogenMass), 20d, $"{chain} Acyl+") { SpectrumComment = SpectrumComment.acylchain },
'new SpectrumPeak(adduct.ConvertToMz(chain.Mass ), 5d, $"{chain} Acyl+ +H") { SpectrumComment = SpectrumComment.acylchain },
}})
                    Next
                End If
            ElseIf Equals(adduct.AdductIonName, "[M-H]-") Then
                spectrum.AddRange({New SpectrumPeak(adduct.ConvertToMz(lipid.Mass), 999R, "Precursor") With {
.SpectrumComment = SpectrumComment.precursor
}, New SpectrumPeak(C3H6O5P + Electron, 30R, "Header-") With {
.SpectrumComment = SpectrumComment.metaboliteclass
}})

                If CSharpImpl.__Assign(Chains, TryCast(lipid.Chains, SeparatedChains)) IsNot Nothing Then
                    For Each chain As AcylChain In lipid.Chains.GetDeterminedChains()
                        spectrum.AddRange({New SpectrumPeak(chain.Mass + OxygenMass + Electron, 50R, $"{chain} FA") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(lipid.Mass - chain.Mass + Electron, 20R, $"-{chain}") With {
.SpectrumComment = SpectrumComment.acylchain
}})
                    Next
                End If
            Else
                spectrum.AddRange({New SpectrumPeak(adduct.ConvertToMz(lipid.Mass), 999R, "Precursor") With {
.SpectrumComment = SpectrumComment.precursor
}})
            End If
            Return spectrum.ToArray()
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

        Private Shared ReadOnly comparer As IEqualityComparer(Of SpectrumPeak) = New SpectrumEqualityComparer()

End Class
