﻿#Region "Microsoft.VisualBasic::0a741aa922fba9bea48406511f06f035, metadb\SMILES\Language\ElementTypes.vb"

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

    '   Total Lines: 52
    '    Code Lines: 12 (23.08%)
    ' Comment Lines: 37 (71.15%)
    '    - Xml Docs: 89.19%
    ' 
    '   Blank Lines: 3 (5.77%)
    '     File Size: 1.88 KB


    '     Enum ElementTypes
    ' 
    '         AtomGroup, Close, Disconnected, Element, Isomers
    '         Key, None, Open
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language

    Public Enum ElementTypes
        None
        ''' <summary>
        ''' alphabet
        ''' </summary>
        Element

        ''' <summary>
        ''' contains multiple atom label inside, example as: NH4+/PO3-
        ''' </summary>
        AtomGroup

        ''' <summary>
        ''' <see cref="Bonds"/>
        ''' </summary>
        Key
        ''' <summary>
        ''' (
        ''' </summary>
        Open
        ''' <summary>
        ''' )
        ''' </summary>
        Close
        ''' <summary>
        ''' ##### Disconnected Structures
        ''' 
        ''' Disconnected compounds are written as individual structures separated by a "." (period). 
        ''' The order in which ions or ligands are listed is arbitrary. There is no implied pairing 
        ''' of one charge with another, nor is it necessary to have a net zero charge. If desired, 
        ''' the SMILES of one ion may be imbedded within another as shown in the example of sodium 
        ''' phenoxide.
        ''' 
        ''' Matching pairs of digits following atom specifications imply that the atoms are bonded 
        ''' to each other. The bond may be explicit (bond symbol and/or direction preceding the ring 
        ''' closure digit) or implicit (a nondirectional single or aromatic bond). This is true 
        ''' whether or not the bond ends up as part of a ring.
        ''' 
        ''' Adjacent atoms separated by dot (.) implies that the atoms are Not bonded To Each other. 
        ''' This Is True whether Or Not the atoms are In the same connected component.
        ''' 
        ''' For example, C1.C1 specifies the same molecule as CC(ethane)
        ''' </summary>
        Disconnected
        ''' <summary>
        ''' E/Z
        ''' </summary>
        Isomers
    End Enum
End Namespace
