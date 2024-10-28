﻿#Region "Microsoft.VisualBasic::f4416b5f6a2eb9100f1b6fb99190659f, metadb\SMILES\Graph\ChemicalKey.vb"

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

    '   Total Lines: 23
    '    Code Lines: 11 (47.83%)
    ' Comment Lines: 7 (30.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (21.74%)
    '     File Size: 642 B


    ' Class ChemicalKey
    ' 
    '     Properties: bond
    ' 
    '     Function: AtomGroups, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Network

''' <summary>
''' the edge connection between the atoms
''' </summary>
Public Class ChemicalKey : Inherits Edge(Of ChemicalElement)

    ''' <summary>
    ''' the charge of current chemical key
    ''' </summary>
    ''' <returns></returns>
    Public Property bond As Bonds

    Public Iterator Function AtomGroups() As IEnumerable(Of ChemicalElement)
        Yield U
        Yield V
    End Function

    Public Overrides Function ToString() As String
        Return $"{U.elementName}{bond.Description}{V.elementName} (+{CInt(bond)})"
    End Function

End Class
