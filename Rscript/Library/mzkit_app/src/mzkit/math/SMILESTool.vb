﻿#Region "Microsoft.VisualBasic::a9dc7f2519df3dfb5f366c502a3ce1f2, Rscript\Library\mzkit_app\src\mzkit\math\SMILESTool.vb"

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

    '   Total Lines: 211
    '    Code Lines: 121 (57.35%)
    ' Comment Lines: 73 (34.60%)
    '    - Xml Docs: 90.41%
    ' 
    '   Blank Lines: 17 (8.06%)
    '     File Size: 8.94 KB


    ' Module SMILESTool
    ' 
    '     Function: asFormula, asGraph, asMatrix, atomGroups, atomLinks
    '               atoms_table, parseSMILES, score
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.BioDeep.Chemoinformatics
Imports BioNovoGene.BioDeep.Chemoinformatics.Formula
Imports BioNovoGene.BioDeep.Chemoinformatics.SMILES
Imports BioNovoGene.BioDeep.Chemoinformatics.SMILES.Embedding
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports list = SMRUCC.Rsharp.Runtime.Internal.Object.list
Imports RDataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' ### Simplified molecular-input line-entry system
''' 
''' The simplified molecular-input line-entry system (SMILES) is a specification in the 
''' form of a line notation for describing the structure of chemical species using short
''' ASCII strings. SMILES strings can be imported by most molecule editors for conversion
''' back into two-dimensional drawings or three-dimensional models of the molecules.
'''
''' The original SMILES specification was initiated In the 1980S. It has since been 
''' modified And extended. In 2007, an open standard called OpenSMILES was developed In
''' the open source chemistry community.
''' </summary>
<Package("SMILES", Category:=APICategories.UtilityTools)>
Module SMILESTool

    Sub Main()
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(ChemicalFormula), AddressOf atoms_table)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function atoms_table(smiles As ChemicalFormula, args As list, env As Environment) As RDataframe
        Return atomGroups(smiles)
    End Function

    ''' <summary>
    ''' Parse the SMILES molecule structre string
    ''' </summary>
    ''' <param name="SMILES"></param>
    ''' <param name="strict"></param>
    ''' <returns>
    ''' A chemical graph object that could be used for build formula or structure analysis
    ''' </returns>
    ''' <remarks>
    ''' SMILES denotes a molecular structure as a graph with optional chiral 
    ''' indications. This is essentially the two-dimensional picture chemists
    ''' draw to describe a molecule. SMILES describing only the labeled
    ''' molecular graph (i.e. atoms and bonds, but no chiral or isotopic 
    ''' information) are known as generic SMILES.
    ''' </remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("parse")>
    <RApiReturn(GetType(ChemicalFormula))>
    Public Function parseSMILES(SMILES As String, Optional strict As Boolean = True) As ChemicalFormula
        Return ParseChain.ParseGraph(SMILES, strict)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("as.formula")>
    <RApiReturn(GetType(Formula))>
    Public Function asFormula(SMILES As ChemicalFormula, Optional canonical As Boolean = True) As Formula
        Return SMILES.GetFormula(canonical)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("as.graph")>
    Public Function asGraph(smiles As ChemicalFormula) As NetworkGraph
        Return smiles.AsGraph
    End Function

    ''' <summary>
    ''' cast the smiles molecule graph as matrix
    ''' </summary>
    ''' <param name="smiles"></param>
    ''' <param name="atoms">a set of the target atom group keys</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("graph_matrix")>
    <RApiReturn(GetType(NumericMatrix))>
    Public Function asMatrix(smiles As ChemicalFormula, atoms As String()) As Object
        Return SparseGraph.CreateMatrix(smiles.AllBonds, atoms)
    End Function

    ''' <summary>
    ''' get atoms table from the SMILES structure data
    ''' </summary>
    ''' <param name="SMILES"></param>
    ''' <returns></returns>
    <ExportAPI("atoms")>
    Public Function atomGroups(SMILES As ChemicalFormula) As RDataframe
        Dim elements As SmilesAtom() = SMILES.GetAtomTable.ToArray
        Dim rowKeys As String() = elements.Select(Function(a) a.id).ToArray
        Dim atoms As String() = elements.Select(Function(a) a.atom).ToArray
        Dim groups As String() = elements.Select(Function(a) a.group).ToArray
        Dim ionCharge As Integer() = elements.Select(Function(a) a.ion_charge).ToArray
        Dim links As Integer() = elements.Select(Function(a) a.links).ToArray
        Dim partners As String() = elements.Select(Function(a) a.connected.JoinBy("; ")).ToArray
        Dim graph_id As Integer() = elements.Select(Function(a) a.graph_id).ToArray
        Dim aromatic As Boolean() = elements.Select(Function(a) a.aromatic).ToArray

        Return New RDataframe With {
            .rownames = rowKeys,
            .columns = New Dictionary(Of String, Array) From {
                {"atom", atoms},
                {"group", groups},
                {"ion_charge", ionCharge},
                {"links", links},
                {"connected", partners},
                {"graph_id", graph_id},
                {"aromatic", aromatic}
            }
        }
    End Function

    ''' <summary>
    ''' evaluate the similarity score between two molecular strcuture 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="kappa"></param>
    ''' <param name="normalize_size"></param>
    ''' <returns>
    ''' a tuple list that contains the score metrics between to given
    ''' molecular strucutre data:
    ''' 
    ''' 1. cos
    ''' 2. euclidean
    ''' 3. jaccard
    ''' 
    ''' </returns>
    <ExportAPI("score")>
    <RApiReturn(TypeCodes.double)>
    Public Function score(x As ChemicalFormula, y As ChemicalFormula,
                          Optional kappa As Double = 2,
                          Optional normalize_size As Boolean = False) As list

        Dim a As AtomLink() = x.GraphEmbedding(kappa, normalize_size).ToArray
        Dim b As AtomLink() = y.GraphEmbedding(kappa, normalize_size).ToArray
        Dim vec As New VectorEmbedding(a, b)

        Return New list(
            slot("cos") = vec.Cosine,
            slot("euclidean") = vec.Euclidean,
            slot("jaccard") = vec.Jaccard
        )
    End Function

    ''' <summary>
    ''' create graph embedding result for a specific molecular strucutre data
    ''' </summary>
    ''' <param name="SMILES">the molecular structure data which is parsed from a given smiles string</param>
    ''' <param name="kappa">kappa parameter for SGT embedding algorithm</param>
    ''' <param name="normalize_size"></param>
    ''' <returns>
    ''' a dataframe object that contains the SGT embedding result of a molecular 
    ''' strcutre data, contains the data fields:
    ''' 
    ''' 1. atom1 the label of the atom group
    ''' 2. atom2 the label of the another atom group
    ''' 3. weight the embedding score result of current link
    ''' 4. vk SGT vk score
    ''' 5. v0 SGT v0 score
    ''' 6. vertex a set of the vertex data for generates current graph embedding score data
    ''' </returns>
    <ExportAPI("links")>
    <RApiReturn(GetType(AtomLink))>
    Public Function atomLinks(SMILES As ChemicalFormula,
                              Optional kappa As Double = 2,
                              Optional normalize_size As Boolean = False,
                              Optional tabular As Boolean = True) As Object

        Dim links As AtomLink() = SMILES _
            .GraphEmbedding(kappa, normalize_size) _
            .ToArray

        If Not tabular Then
            Return links
        End If

        Dim atom1 As String() = links.Select(Function(l) l.atom1).ToArray
        Dim atom2 As String() = links.Select(Function(l) l.atom2).ToArray
        Dim weight As Double() = links.Select(Function(l) l.score).ToArray
        Dim vk As Double() = links.Select(Function(l) l.vk).ToArray
        Dim v0 As Double() = links.Select(Function(l) l.v0).ToArray
        Dim vertex As String() = links _
            .Select(Function(l) l.vertex.ToBEncodeString) _
            .ToArray

        Return New RDataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"atom1", atom1},
                {"atom2", atom2},
                {"weight", weight},
                {"vk", vk},
                {"v0", v0},
                {"vertex", vertex}
            }
        }
    End Function
End Module
