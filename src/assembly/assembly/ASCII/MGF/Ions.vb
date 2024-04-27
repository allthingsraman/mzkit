﻿#Region "Microsoft.VisualBasic::c3ff3864fc011e72f6abde22bec0caf3, G:/mzkit/src/assembly/assembly//ASCII/MGF/Ions.vb"

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

    '   Total Lines: 109
    '    Code Lines: 55
    ' Comment Lines: 42
    '   Blank Lines: 12
    '     File Size: 3.50 KB


    '     Class Ions
    ' 
    '         Properties: Accession, Charge, Database, Instrument, Locus
    '                     Meta, Peaks, PepMass, Rawfile, RtInSeconds
    '                     Sequence, Title
    ' 
    '         Function: CreateDocs, GetLibrary, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace ASCII.MGF

    ''' <summary>
    ''' Data model of a mgf ion
    ''' 
    ''' > http://www.matrixscience.com/help/data_file_help.html
    ''' </summary>
    Public Class Ions

        Public Property Title As String
        ''' <summary>
        ''' The meta data collection in the title property
        ''' </summary>
        ''' <returns></returns>
        Public Property Meta As Dictionary(Of String, String)
        ''' <summary>
        ''' MS1 rt in seconds format
        ''' </summary>
        ''' <returns></returns>
        Public Property RtInSeconds As Double
        ''' <summary>
        ''' charge state
        ''' </summary>
        ''' <returns></returns>
        Public Property Charge As Integer
        ''' <summary>
        ''' Database entries to be searched
        ''' </summary>
        ''' <returns></returns>
        Public Property Accession As String
        Public Property Instrument As String
        Public Property Rawfile As String
        ''' <summary>
        ''' Hierarchical scan range identifier
        ''' </summary>
        ''' <returns></returns>
        Public Property Locus As String
        ''' <summary>
        ''' Element sequence
        ''' </summary>
        ''' <returns></returns>
        Public Property Sequence As String
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public Property Database As String
        Public Property PepMass As NamedValue
        ''' <summary>
        ''' MS/MS peaks
        ''' </summary>
        ''' <returns></returns>
        Public Property Peaks As ms2()

        Public Overrides Function ToString() As String
            Return $"{Title} ({Peaks.SafeQuery.Count} peaks)"
        End Function

        Public Function CreateDocs() As String
            Dim text As New StringBuilder

            Using writer As New StringWriter(text)
                Call Me.WriteAsciiMgf(out:=writer)
            End Using

            Return text.ToString
        End Function

        Public Function GetLibrary() As LibraryMatrix
            Return New LibraryMatrix With {
                .ms2 = Peaks,
                .Name = Title
            }
        End Function

        ''' <summary>
        ''' Try to cast a mgf ion object to the mzkit general spectrum peak data
        ''' </summary>
        ''' <param name="ion"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(ion As Ions) As PeakMs2
            Dim meta As New MetaData(ion.Meta)
            Dim id As String = If(ion.Accession, ion.Title)

            meta!ion_intensity = ion.PepMass.text

            Return New PeakMs2 With {
                .activation = meta.activation,
                .collisionEnergy = meta.collisionEnergy,
                .file = ion.Rawfile,
                .mz = Val(ion.PepMass.name),
                .mzInto = ion.Peaks,
                .rt = ion.RtInSeconds,
                .scan = meta.scan,
                .lib_guid = id,
                .meta = meta,
                .precursor_type = meta.precursor_type,
                .intensity = Val(ion.PepMass.text)
            }
        End Operator

    End Class
End Namespace
