﻿#Region "Microsoft.VisualBasic::9450dd7eea16bb8fc0fe8ddfac685424, mzmath\SingleCells\File\SpatialIndex.vb"

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

    '   Total Lines: 53
    '    Code Lines: 21 (39.62%)
    ' Comment Lines: 24 (45.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (15.09%)
    '     File Size: 1.61 KB


    '     Structure SpatialIndex
    ' 
    '         Properties: X, Y, Z
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging

Namespace File

    ''' <summary>
    ''' the binary file offset of a spatial spot
    ''' </summary>
    Public Structure SpatialIndex : Implements IPoint3D

        ''' <summary>
        ''' spatial z-axis
        ''' </summary>
        ''' <returns></returns>
        Public Property Z As Integer Implements IPoint3D.Z
        ''' <summary>
        ''' spatial x-axis
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Integer Implements RasterPixel.X
        ''' <summary>
        ''' spatial y-axis
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Integer Implements RasterPixel.Y

        ''' <summary>
        ''' the spot point offset in the binary data file
        ''' </summary>
        Dim offset As Long

        Sub New(x As Integer, y As Integer, z As Integer, offset As Long)
            Me.X = x
            Me.Y = y
            Me.Z = z
            Me.offset = offset
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X},{Y},{Z}]=&{StringFormats.Lanudry(bytes:=offset)}"
        End Function

        ''' <summary>
        ''' calculate the spot data offset in the binary file
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Shared Operator +(index As SpatialIndex, offset As Integer) As Long
            Return index.offset + offset
        End Operator

    End Structure
End Namespace
