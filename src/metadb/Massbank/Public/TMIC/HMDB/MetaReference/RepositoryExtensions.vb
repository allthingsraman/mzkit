﻿#Region "Microsoft.VisualBasic::ee7407ecf9556ca299563a9f5ed05960, G:/mzkit/src/metadb/Massbank//Public/TMIC/HMDB/MetaReference/RepositoryExtensions.vb"

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
    '    Code Lines: 33
    ' Comment Lines: 13
    '   Blank Lines: 7
    '     File Size: 2.02 KB


    '     Module RepositoryExtensions
    ' 
    '         Function: EnumerateNames, GetMetabolite, PopulateHMDBMetaData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports XmlLinq = Microsoft.VisualBasic.Text.Xml.Linq.Data

Namespace TMIC.HMDB.Repository

    <HideModuleName>
    Public Module RepositoryExtensions

        ReadOnly web As New Dictionary(Of String, WebQuery)

        ''' <summary>
        ''' get hmdb metabolite data from online web services
        ''' </summary>
        ''' <param name="id">
        ''' get data via this given hmdb metabolite id
        ''' </param>
        ''' <param name="cache">
        ''' the local cache dir
        ''' </param>
        ''' <param name="offline">
        ''' running the data query in offline mode?
        ''' </param>
        ''' <returns></returns>
        Public Function GetMetabolite(id As String,
                                      Optional cache$ = "./hmdb/",
                                      Optional offline As Boolean = False) As metabolite

            Dim engine As WebQuery = web.ComputeIfAbsent(cache, lazyValue:=Function() New WebQuery(cache,, offline))
            engine.offlineMode = offline
            Return engine.Query(Of metabolite)(id)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateHMDBMetaData(Xml As String) As IEnumerable(Of MetaReference)
            Return XmlLinq.LoadXmlDataSet(Of MetaReference)(
                XML:=Xml,
                typeName:=NameOf(metabolite),
                xmlns:="http://www.hmdb.ca",
                forceLargeMode:=True
            )
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EnumerateNames(metabolite As MetaReference) As IEnumerable(Of String)
            Return {metabolite.name}.AsList +
                metabolite.synonyms.synonym +
                metabolite.iupac_name
        End Function
    End Module
End Namespace
