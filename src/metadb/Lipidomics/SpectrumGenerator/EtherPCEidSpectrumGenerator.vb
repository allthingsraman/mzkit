﻿#Region "Microsoft.VisualBasic::111c4b9708c8bedbb70f7758cd22aef4, metadb\Lipidomics\SpectrumGenerator\EtherPCEidSpectrumGenerator.vb"

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

    '   Total Lines: 143
    '    Code Lines: 122 (85.31%)
    ' Comment Lines: 2 (1.40%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (13.29%)
    '     File Size: 7.72 KB


    ' Class EtherPCEidSpectrumGenerator
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: CanGenerate, CreateReference, EidSpecificSpectrum, Generate, GetEtherPCOSpectrum
    '               GetEtherPCPSpectrum, GetEtherPCSpectrum, GetSn1PositionSpectrum
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.BioDeep.MSEngine
Imports BioNovoGene.BioDeep.Chemoinformatics.Formula.ElementsExactMass
Imports BioNovoGene.BioDeep.Chemoinformatics.Formula.MS

Public Class EtherPCEidSpectrumGenerator
    Implements ILipidSpectrumGenerator
    Private Shared ReadOnly C5H14NO4P As Double = {CarbonMass * 5, HydrogenMass * 14, NitrogenMass, OxygenMass * 4, PhosphorusMass}.Sum()

    Private Shared ReadOnly C3H9N As Double = {CarbonMass * 3, HydrogenMass * 9, NitrogenMass}.Sum()

    Private Shared ReadOnly C5H11N As Double = {CarbonMass * 5, HydrogenMass * 11, NitrogenMass}.Sum()

    Private Shared ReadOnly Gly_C As Double = {CarbonMass * 8, HydrogenMass * 18, NitrogenMass, OxygenMass * 4, PhosphorusMass}.Sum()

    Private Shared ReadOnly Gly_O As Double = {CarbonMass * 7, HydrogenMass * 16, NitrogenMass, OxygenMass * 5, PhosphorusMass}.Sum()

    Private Shared ReadOnly H2O As Double = {HydrogenMass * 2, OxygenMass}.Sum()

    Private Shared ReadOnly CH2 As Double = {HydrogenMass * 2, CarbonMass}.Sum()

    Public Sub New()
        spectrumGenerator = New SpectrumPeakGenerator()
    End Sub

    Public Sub New(spectrumGenerator As ISpectrumPeakGenerator)
        Me.spectrumGenerator = spectrumGenerator
    End Sub

    Private ReadOnly spectrumGenerator As ISpectrumPeakGenerator

    Public Function CanGenerate(lipid As ILipid, adduct As AdductIon) As Boolean Implements ILipidSpectrumGenerator.CanGenerate
        If lipid.LipidClass = LbmClass.EtherPC Then
            If Equals(adduct.AdductIonName, "[M+H]+") Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Function Generate(lipid As Lipid, adduct As AdductIon, Optional molecule As IMoleculeProperty = Nothing) As IMSScanProperty Implements ILipidSpectrumGenerator.Generate
        Dim spectrum = New List(Of SpectrumPeak)()
        spectrum.AddRange(GetEtherPCSpectrum(lipid, adduct))
        lipid.Chains.ApplyToChain(1, Sub(chain) spectrum.AddRange(GetSn1PositionSpectrum(lipid, chain, adduct)))
        Dim alkyl As AlkylChain = Nothing, acyl As AcylChain = Nothing
        Dim nil As (alkyl As AlkylChain, acyl As AcylChain) = lipid.Chains.Deconstruct(Of AlkylChain, AcylChain)()
        alkyl = nil.alkyl
        acyl = nil.acyl
        If alkyl IsNot Nothing AndAlso acyl IsNot Nothing Then
            If alkyl.DoubleBond.Bonds.Any(Function(b) b.Position = 1) Then
                spectrum.AddRange(GetEtherPCPSpectrum(lipid, alkyl, acyl, adduct))
            Else
                spectrum.AddRange(GetEtherPCOSpectrum(lipid, alkyl, acyl, adduct))
            End If
            spectrum.AddRange(spectrumGenerator.GetAlkylDoubleBondSpectrum(lipid, alkyl, adduct, 0R, 20.0R))
            spectrum.AddRange(spectrumGenerator.GetAcylDoubleBondSpectrum(lipid, acyl, adduct, 0R, 20.0R))
        End If
        spectrum.AddRange(EidSpecificSpectrum(lipid, adduct, 0R, 300.0R))
        spectrum = spectrum.GroupBy(Function(spec) spec, comparer).[Select](Function(specs) New SpectrumPeak(Enumerable.First(specs).mz, specs.Sum(Function(n) n.Intensity), String.Join(", ", specs.[Select](Function(spec) spec.Annotation)), specs.Aggregate(SpectrumComment.none, Function(a, b) a Or b.SpectrumComment))).OrderBy(Function(peak) peak.mz).ToList()
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

    Private Function GetEtherPCSpectrum(lipid As ILipid, adduct As AdductIon) As SpectrumPeak()
        Dim spectrum = New List(Of SpectrumPeak) From {
New SpectrumPeak(adduct.ConvertToMz(lipid.Mass), 999.0R, "Precursor") With {
    .SpectrumComment = SpectrumComment.precursor
},
New SpectrumPeak(adduct.ConvertToMz(Gly_C), 400.0R, "Gly-C") With {
    .SpectrumComment = SpectrumComment.metaboliteclass,
    .IsAbsolutelyRequiredFragmentForAnnotation = True
},
New SpectrumPeak(adduct.ConvertToMz(Gly_O), 400.0R, "Gly-O") With {
    .SpectrumComment = SpectrumComment.metaboliteclass,
    .IsAbsolutelyRequiredFragmentForAnnotation = True
},
New SpectrumPeak(adduct.ConvertToMz(C5H14NO4P), 500.0R, "Header") With {
    .SpectrumComment = SpectrumComment.metaboliteclass,
    .IsAbsolutelyRequiredFragmentForAnnotation = True
},
New SpectrumPeak(adduct.ConvertToMz(lipid.Mass) / 2, 200.0R, "[Precursor]2+") With {
    .SpectrumComment = SpectrumComment.metaboliteclass
}
}
        Return spectrum.ToArray()
    End Function

    Private Function GetEtherPCPSpectrum(lipid As ILipid, alkylChain As IChain, acylChain As IChain, adduct As AdductIon) As SpectrumPeak()
        'new SpectrumPeak(adduct.ConvertToMz(lipid.Mass - alkylChain.Mass), 100d, $"-{alkylChain}"),
        'new SpectrumPeak(adduct.ConvertToMz(lipid.Mass - acylChain.Mass), 100d, $"-{acylChain}"),
        Return {New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - alkylChain.Mass - OxygenMass), 100, $"-{alkylChain}-O") With {
        .SpectrumComment = SpectrumComment.acylchain
        }, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - acylChain.Mass - OxygenMass - HydrogenMass), 200.0R, $"-{acylChain}-O") With {
        .SpectrumComment = SpectrumComment.acylchain
    }}
    End Function

    Private Function GetEtherPCOSpectrum(lipid As ILipid, alkylChain As IChain, acylChain As IChain, adduct As AdductIon) As SpectrumPeak()
        Return {New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - alkylChain.Mass), 100.0R, $"-{alkylChain}") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - acylChain.Mass + HydrogenMass), 100.0R, $"-{acylChain}") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - alkylChain.Mass - OxygenMass), 100.0R, $"-{alkylChain}-O") With {
.SpectrumComment = SpectrumComment.acylchain
}, New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - acylChain.Mass - H2O), 100.0R, $"-{acylChain}-O") With {
.SpectrumComment = SpectrumComment.acylchain
}}
    End Function

    Private Function GetSn1PositionSpectrum(lipid As ILipid, alkylChain As IChain, adduct As AdductIon) As SpectrumPeak()
        Return {New SpectrumPeak(adduct.ConvertToMz(lipid.Mass - alkylChain.Mass - OxygenMass - CH2), 150.0R, "-CH2(Sn1)") With {
.SpectrumComment = SpectrumComment.snposition
}}
    End Function
    Private Shared Function EidSpecificSpectrum(lipid As Lipid, adduct As AdductIon, nlMass As Double, intensity As Double) As SpectrumPeak()
        Dim spectrum = New List(Of SpectrumPeak)()
        Dim chains As SeparatedChains = TryCast(lipid.Chains, SeparatedChains)

        If chains IsNot Nothing Then
            For Each chain In chains.GetDeterminedChains()
                If chain.DoubleBond.Count = 0 OrElse chain.DoubleBond.UnDecidedCount > 0 Then Continue For
                spectrum.AddRange(EidSpecificSpectrumGenerator.EidSpecificSpectrumGen(lipid, chain, adduct, nlMass, intensity))
            Next
        End If
        Return spectrum.ToArray()
    End Function

End Class
