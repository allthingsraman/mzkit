﻿Imports BioNovoGene.BioDeep.Chemistry.Model
Imports BioNovoGene.BioDeep.Chemoinformatics.SMILES
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce
Imports Microsoft.VisualBasic.Imaging

Public Class frmSMILESViewer

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim smilesStr As String = Strings.Trim(TextBox1.Text)
        Dim graph As ChemicalFormula = ParseChain.ParseGraph(smilesStr)
        Dim network As New NetworkGraph

        For Each v In graph.vertex
            network.CreateNode(v.label, New NodeData With {.label = v.elementName, .color = Brushes.Black})
        Next
        For Each l In graph.graphEdges
            Dim url = network.CreateEdge(
                  u:=l.U.label,
                  v:=l.V.label,
                  weight:=l.bond
              )

            url.data.style = New Pen(Color.Red, 2)
            network.AddEdge(url)
        Next

        Canvas1.Graph() = network
        Canvas1.ShowLabel = True
    End Sub

    Private Sub frmSMILESViewer_Load(sender As Object, e As EventArgs) Handles Me.Load
        Canvas1.SetFDGParams(New ForceDirectedArgs With {.Repulsion = 10000.0!})
    End Sub

    Private Sub Canvas1_Load(sender As Object, e As EventArgs) Handles Canvas1.Load

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim kcf As String = TextBox2.Text
        Dim model As KCF = IO.LoadKCF(kcf)
        Dim visual As Image = KEGGdraw.Canvas.Draw(model).AsGDIImage

        PictureBox1.BackgroundImage = visual
    End Sub
End Class