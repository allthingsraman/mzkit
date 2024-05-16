﻿#Region "Microsoft.VisualBasic::841e91c0a3176428c2064bf102687d8d, metadna\MetaDNA_visual\GraphAlgorithm.vb"

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

    '   Total Lines: 117
    '    Code Lines: 99
    ' Comment Lines: 5
    '   Blank Lines: 13
    '     File Size: 5.18 KB


    ' Module GraphAlgorithm
    ' 
    '     Function: CreateGraph
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Public Module GraphAlgorithm

    <Extension>
    Public Function CreateGraph(metaDNA As XML) As NetworkGraph
        Dim g As New NetworkGraph
        Dim kegg_compound As Graph.Node
        Dim candidate_compound As Graph.Node
        Dim edge As Edge
        Dim candidateParent As node
        Dim seedNode As Graph.Node

        For Each compound As compound In metaDNA.compounds
            kegg_compound = New Graph.Node With {
                .label = compound.kegg,
                .data = New NodeData() With {
                    .label = compound.kegg,
                    .origID = compound.kegg,
                    .Properties = New Dictionary(Of String, String) From {
                        {NamesOf.REFLECTION_ID_MAPPING_NODETYPE, "kegg_compound"},
                        {"candidates", compound.size}
                    }
                }
            }

            Call g.AddNode(kegg_compound)

            For Each candidate As unknown In compound.candidates
                candidate_compound = New Graph.Node With {
                    .label = candidate.name,
                    .data = New NodeData With {
                        .label = candidate.name,
                        .origID = candidate.Msn,
                        .Properties = New Dictionary(Of String, String) From {
                            {NamesOf.REFLECTION_ID_MAPPING_NODETYPE, "MetaDNA.candidate"},
                            {"intensity", candidate.intensity},
                            {"infer.depth", candidate.length}
                        }
                    }
                }
                edge = New Edge With {
                    .U = kegg_compound,
                    .V = candidate_compound,
                    .weight = candidate.edges.Length,
                    .data = New EdgeData With {
                        .label = $"{candidate_compound.label} infer as {kegg_compound.label}",
                        .Properties = New Dictionary(Of String, String) From {
                            {NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE, "is_candidate"},
                            {"score.forward", candidate.scores.ElementAtOrNull(0)},
                            {"score.reverse", candidate.scores.ElementAtOrNull(1)}
                        }
                    }
                }

                Call g.AddNode(candidate_compound)
                Call g.AddEdge(edge)

                ' add common seed node
                ' is a metaDNA seed from reference library
                ' spectrum alignment result
                seedNode = g.GetElementByID(candidate.edges(Scan0).ms1)

                If seedNode Is Nothing Then
                    ' 还没有添加进入网络之中
                    seedNode = New Graph.Node With {
                        .label = candidate.edges(Scan0).ms1,
                        .data = New NodeData With {
                            .label = candidate.edges(Scan0).ms1,
                            .origID = candidate.edges(Scan0).ms2,
                            .Properties = New Dictionary(Of String, String) From {
                                {NamesOf.REFLECTION_ID_MAPPING_NODETYPE, "seed"}
                            }
                        }
                    }

                    Call g.AddNode(seedNode)
                End If

                For i As Integer = 0 To candidate.length - 2
                    seedNode = g.GetElementByID(candidate.edges(i).ms1)
                    edge = New Edge With {
                        .data = New EdgeData With {
                            .label = $"{candidate.edges(i).kegg} -> {candidate.edges(i + 1).kegg}",
                            .Properties = New Dictionary(Of String, String) From {
                                {NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE, "infer"}
                            }
                        },
                        .U = seedNode,
                        .V = g.GetElementByID(candidate.edges(i + 1).ms1)
                    }

                    Call g.AddEdge(edge)
                Next

                ' add edge that infer to current candidate
                candidateParent = candidate.edges.Last
                edge = New Edge With {
                    .data = New EdgeData With {
                        .label = $"{candidateParent.kegg} -> {compound.kegg}",
                        .Properties = New Dictionary(Of String, String) From {
                            {NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE, "infer"}
                        }
                    },
                    .U = g.GetElementByID(candidateParent.ms1),
                    .V = g.GetElementByID(candidate.name)
                }
                Call g.AddEdge(edge)
            Next
        Next

        Return g
    End Function
End Module
