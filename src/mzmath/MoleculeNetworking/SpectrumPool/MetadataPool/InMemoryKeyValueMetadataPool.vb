﻿Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.Xml

Namespace PoolData

    Public Class InMemoryKeyValueMetadataPool : Inherits MetadataProxy

        Dim data As Dictionary(Of String, Metadata)

        Dim m_rootId As String
        Dim m_depth As Integer

        Default Public Overrides ReadOnly Property GetById(id As String) As Metadata
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return data(id)
            End Get
        End Property

        Public Overrides ReadOnly Property AllClusterMembers As IEnumerable(Of Metadata)
            Get
                Return data.Values
            End Get
        End Property

        Public Overrides ReadOnly Property Depth As Integer
            Get
                Return m_depth
            End Get
        End Property

        Public Overrides ReadOnly Property RootId As String
            Get
                Return m_rootId
            End Get
        End Property

        Sub New(data As Dictionary(Of String, Metadata), depth As Integer)
            Me.data = data
            Me.m_depth = depth
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Add(id As String, metadata As Metadata)
            Call data.Add(id, metadata)
        End Sub

        Public Overrides Function HasGuid(id As String) As Boolean
            Return data.ContainsKey(id)
        End Function

        Public Overrides Sub SetRootId(hashcode As String)
            m_rootId = hashcode
        End Sub

        Public Overrides Sub Add(id As String, score As Double, align As AlignmentOutput, pval As Double)

        End Sub
    End Class
End Namespace