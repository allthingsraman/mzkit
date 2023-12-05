﻿#Region "Microsoft.VisualBasic::f34a6b0fa9e664d199ad0b50c92b35d5, mzkit\src\metadb\Massbank\Public\TMIC\HMDB\MetaReference\WebQuery.vb"

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

'   Total Lines: 29
'    Code Lines: 24
' Comment Lines: 0
'   Blank Lines: 5
'     File Size: 1.15 KB


'     Class WebQuery
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: ParseXml
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.BioDeep.Chemistry.MetaLib.CrossReference
Imports Microsoft.VisualBasic.Net.Http

Namespace TMIC.HMDB.Repository

    Public Class WebQuery : Inherits WebQuery(Of String)

        Public Sub New(<CallerMemberName>
                       Optional cache As String = Nothing,
                       Optional interval As Integer = -1,
                       Optional offline As Boolean = False)

            MyBase.New(url:=Function(id) $"http://www.hmdb.ca/metabolites/{id.FormatHMDB}.xml",
                       contextGuid:=Function(id) id,
                       parser:=AddressOf ParseXml,
                       prefix:=Function(id) Mid(id.Match("\d+").ParseInteger.ToString, 1, 2),
                       cache:=cache,
                       interval:=interval,
                       offline:=offline
            )
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function ParseXml(xml$, null As Type) As metabolite
            Return xml.LoadFromXml(Of metabolite)(throwEx:=False)
        End Function
    End Class
End Namespace
