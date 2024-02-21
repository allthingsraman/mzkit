﻿#Region "Microsoft.VisualBasic::9ae7e6d2abc54c8b4231219b1148475d, mzkit\src\metadb\Massbank\Public\TMIC\HMDB\Assembly\disease.vb"

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

'   Total Lines: 28
'    Code Lines: 21
' Comment Lines: 0
'   Blank Lines: 7
'     File Size: 728 B


'     Class disease
' 
'         Properties: name, omim_id, references
' 
'     Structure reference
' 
'         Properties: pubmed_id, reference_text
' 
'         Function: ToString
' 
'     Class protein
' 
'         Properties: gene_name, name, protein_accession, protein_type, uniprot_id
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization

Namespace TMIC.HMDB

    Public Class disease

        <MessagePackMember(0)> Public Property name As String
        <MessagePackMember(1)> Public Property omim_id As String
        <MessagePackMember(2)> Public Property references As reference()

    End Class

    Public Structure reference
        <MessagePackMember(0)> Public Property reference_text As String
        <MessagePackMember(1)> Public Property pubmed_id As String

        Public Overrides Function ToString() As String
            Return reference_text
        End Function
    End Structure

    Public Class protein

        <MessagePackMember(0)> Public Property protein_accession As String
        <MessagePackMember(1)> Public Property name As String
        <MessagePackMember(2)> Public Property uniprot_id As String
        <MessagePackMember(3)> Public Property gene_name As String
        <MessagePackMember(4)> Public Property protein_type As String

    End Class
End Namespace
