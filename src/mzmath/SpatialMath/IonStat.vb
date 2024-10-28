﻿#Region "Microsoft.VisualBasic::a59e1b1bb82f88cc97b3482df23f02b7, mzmath\SpatialMath\IonStat.vb"

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

    '   Total Lines: 125
    '    Code Lines: 46 (36.80%)
    ' Comment Lines: 68 (54.40%)
    '    - Xml Docs: 98.53%
    ' 
    '   Blank Lines: 11 (8.80%)
    '     File Size: 5.37 KB


    ' Class IonStat
    ' 
    '     Properties: averageIntensity, basePixelX, basePixelY, density, entropy
    '                 maxIntensity, moran, mz, mzmax, mzmin
    '                 mzwidth, pixels, pvalue, Q1Intensity, Q2Intensity
    '                 Q3Intensity, rsd, sparsity
    ' 
    '     Function: DoStat
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.StatsMath
Imports BioNovoGene.Analytical.MassSpectrometry.SingleCells.Deconvolute
Imports Microsoft.VisualBasic.ApplicationServices.Plugin

''' <summary>
''' Stats the ion features inside a MSI raw data slide
''' </summary>
Public Class IonStat

    ''' <summary>
    ''' the ion m/z value of current ms-imaging layer feature
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("F4")> Public Property mz As Double
    ''' <summary>
    ''' the min range value of current ion m/z
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("F4")> Public Property mzmin As Double
    ''' <summary>
    ''' the max range value of current ion m/z
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("F4")> Public Property mzmax As Double
    ''' <summary>
    ''' the description text of the mz range: mzmax - mzmin
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <DisplayName("mz.diff")>
    Public Property mzwidth As String

    ''' <summary>
    ''' the total pixel number of current ion m/z occurs.
    ''' </summary>
    ''' <returns></returns>
    <Category("Spatial")> Public Property pixels As Integer
    ''' <summary>
    ''' the average spatial density
    ''' </summary>
    ''' <returns></returns>
    <Category("Spatial")>
    <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("F2")>
    Public Property density As Double

    ''' <summary>
    ''' the max intensity value of current ion across all pixels
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <DisplayName("max.into")>
    <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("G5")>
    Public Property maxIntensity As Double

    ''' <summary>
    ''' the average intensity value of current ion across all pixels
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <DisplayName("mean.into")>
    <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("G5")>
    Public Property averageIntensity As Double

    ''' <summary>
    ''' the x axis position of the pixel which has the max intensity value of current ion layer
    ''' </summary>
    ''' <returns></returns>
    <DisplayName("basepeak.x")>
    <Category("Spatial")> Public Property basePixelX As Integer
    ''' <summary>
    ''' the y axis position of the pixel which has the max intensity value of current ion layer
    ''' </summary>
    ''' <returns></returns>
    <DisplayName("basepeak.y")>
    <Category("Spatial")> Public Property basePixelY As Integer

    ''' <summary>
    ''' the intensity value of quartile Q1 level(25% quantile)
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <DisplayName("intensity(Q1)")>
    <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("G5")>
    Public Property Q1Intensity As Double
    ''' <summary>
    ''' the intensity value of quartile Q2 level(median value, 50% quantile)
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <DisplayName("intensity(Q2)")>
    <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("G5")>
    Public Property Q2Intensity As Double
    ''' <summary>
    ''' the intensity value of quartile Q3 level(75% quantile)
    ''' </summary>
    ''' <returns></returns>
    <Category("MSdata")> <DisplayName("intensity(Q3)")>
    <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("G5")>
    Public Property Q3Intensity As Double

    Public Property rsd As Double
    Public Property entropy As Double
    Public Property sparsity As Double

    ''' <summary>
    ''' Moran-I index value of current ion layer geometry data
    ''' 
    ''' In statistics, Moran's I is a measure of spatial autocorrelation developed by Patrick Alfred Pierce Moran.
    ''' Spatial autocorrelation is characterized by a correlation in a signal among nearby locations in space. 
    ''' Spatial autocorrelation is more complex than one-dimensional autocorrelation because spatial correlation 
    ''' is multi-dimensional (i.e. 2 or 3 dimensions of space) and multi-directional.
    ''' </summary>
    ''' <returns></returns>
    <Category("Spatial")> <DisplayName("moran I")>
    <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("F3")>
    Public Property moran As Double

    ''' <summary>
    ''' the Moran-I test p-value
    ''' </summary>
    ''' <returns></returns>
    <Category("Spatial")> <DisplayName("moran p-value")>
    <TypeConverter(GetType(FormattedDoubleConverter)), FormattedDoubleFormatString("G4")>
    Public Property pvalue As Double

    Public Shared Function DoStat(rawdata As MzMatrix, Optional grid_size As Integer = 5, Optional parallel As Boolean = True) As IEnumerable(Of IonStat)
        Return rawdata.DoStat(grid_size, parallel)
    End Function
End Class
