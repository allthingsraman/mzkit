﻿#Region "Microsoft.VisualBasic::6e08488e48837cdab9760a8d14836f26, assembly\assembly\UnifyReader\MsDataReader.vb"

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

    '   Total Lines: 131
    '    Code Lines: 82 (62.60%)
    ' Comment Lines: 28 (21.37%)
    '    - Xml Docs: 96.43%
    ' 
    '   Blank Lines: 21 (16.03%)
    '     File Size: 5.99 KB


    '     Class MsDataReader
    ' 
    '         Function: GetActivationMethod, GetBPC, GetCentroided, GetCharge, GetCollisionEnergy
    '                   GetMsLevel, GetMsMs, GetParentMz, GetPolarity, GetScanId
    '                   GetScanTime, GetTIC, IsEmpty, ScanProvider
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra

Namespace DataReader

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="Scan"></typeparam>
    ''' <remarks>
    ''' the scan number and parent scan number is used for create the multiple stage product tree of the mass spectrum data.
    ''' </remarks>
    Public MustInherit Class MsDataReader(Of Scan) : Implements IDataReader, ISpectrumReader(Of Scan)

        Public MustOverride Function GetScanTime(scan As Scan) As Double
        Public MustOverride Function GetScanId(scan As Scan) As String
        Public MustOverride Function IsEmpty(scan As Scan) As Boolean

        ''' <summary>
        ''' get the scan number of current scan data
        ''' </summary>
        ''' <param name="scan"></param>
        ''' <returns></returns>
        Public MustOverride Function GetScanNumber(scan As Scan) As String
        ''' <summary>
        ''' get the scan number of parent scan data
        ''' </summary>
        ''' <param name="scan"></param>
        ''' <returns></returns>
        Public MustOverride Function GetParentScanNumber(scan As Scan) As String

        ''' <summary>
        ''' get ms1 or ms2 data
        ''' </summary>
        ''' <param name="scan"></param>
        ''' <returns></returns>
        Public MustOverride Function GetMsMs(scan As Scan) As ms2() Implements ISpectrumReader(Of Scan).GetMsMs
        Public MustOverride Function GetMsLevel(scan As Scan) As Integer
        Public MustOverride Function GetBPC(scan As Scan) As Double
        Public MustOverride Function GetTIC(scan As Scan) As Double
        Public MustOverride Function GetParentMz(scan As Scan) As Double
        Public MustOverride Function GetPolarity(scan As Scan) As String
        Public MustOverride Function GetCharge(scan As Scan) As Integer
        Public MustOverride Function GetActivationMethod(scan As Scan) As ActivationMethods
        Public MustOverride Function GetCollisionEnergy(scan As Scan) As Double
        Public MustOverride Function GetCentroided(scan As Scan) As Boolean

        ''' <summary>
        ''' A unify method for create the scan data provider
        ''' </summary>
        ''' <returns>
        ''' <see cref="MsDataReader(Of Scan)"/>, and <see cref="ISpectrumReader(Of Scan)"/> 
        ''' </returns>
        Public Shared Function ScanProvider() As Object
            Select Case GetType(Scan)
                Case GetType(mzXML.scan) : Return New mzXMLScan
                Case GetType(mzML.spectrum) : Return New mzMLScan
                Case GetType(imzML.ScanReader) : Return New imzMLScan
                Case Else
                    Throw New NotImplementedException("the spectrum reader for scan data type is not implemeted yet: " & GetType(Scan).ToString)
            End Select
        End Function

#Region "IDataReader"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetScanTime(scan As Object) As Double Implements IDataReader.GetScanTime
            Return GetScanTime(DirectCast(scan, Scan))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetScanId(scan As Object) As String Implements IDataReader.GetScanId
            Return GetScanId(DirectCast(scan, Scan))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function IsEmpty(scan As Object) As Boolean Implements IDataReader.IsEmpty
            Throw New NotImplementedException()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetMsMs(scan As Object) As ms2() Implements IDataReader.GetMsMs
            Return GetMsMs(DirectCast(scan, Scan))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetMsLevel(scan As Object) As Integer Implements IDataReader.GetMsLevel
            Return GetMsLevel(DirectCast(scan, Scan))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetBPC(scan As Object) As Double Implements IDataReader.GetBPC
            Return GetBPC(DirectCast(scan, Scan))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetTIC(scan As Object) As Double Implements IDataReader.GetTIC
            Return GetTIC(DirectCast(scan, Scan))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetParentMz(scan As Object) As Double Implements IDataReader.GetParentMz
            Return GetParentMz(DirectCast(scan, Scan))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetPolarity(scan As Object) As String Implements IDataReader.GetPolarity
            Return GetPolarity(DirectCast(scan, Scan))
        End Function

        Private Function GetCharge(scan As Object) As Integer Implements IDataReader.GetCharge
            Return GetCharge(DirectCast(scan, Scan))
        End Function

        Private Function GetActivationMethod(scan As Object) As ActivationMethods Implements IDataReader.GetActivationMethod
            Return GetActivationMethod(DirectCast(scan, Scan))
        End Function

        Private Function GetCollisionEnergy(scan As Object) As Double Implements IDataReader.GetCollisionEnergy
            Return GetCollisionEnergy(DirectCast(scan, Scan))
        End Function

        Private Function GetCentroided(scan As Object) As Boolean Implements IDataReader.GetCentroided
            Return GetCentroided(DirectCast(scan, Scan))
        End Function
#End Region

    End Class
End Namespace
