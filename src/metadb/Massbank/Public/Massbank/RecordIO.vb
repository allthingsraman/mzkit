﻿#Region "Microsoft.VisualBasic::2c2919dd2b3156a377fc772033a31fb2, G:/mzkit/src/metadb/Massbank//Public/Massbank/RecordIO.vb"

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

    '   Total Lines: 199
    '    Code Lines: 161
    ' Comment Lines: 1
    '   Blank Lines: 37
    '     File Size: 7.42 KB


    '     Module RecordIO
    ' 
    '         Function: (+2 Overloads) __createObject, __createPeaksData, __loadSection, LoadFile, ParseAnnotations
    '                   ScanLoad
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.BioDeep.Chemistry.Massbank.DATA
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports Node =
    System.Collections.Generic.Dictionary(Of
        String,
        Microsoft.VisualBasic.Language.List(Of String))

Namespace Massbank

    Public Module RecordIO

        Public Function ScanLoad(DIR$) As Record()
            Dim out As New List(Of Record)

            For Each record$ In ls - l - r - "*.txt" <= DIR
                out += RecordIO.LoadFile(txt:=record)
            Next

            Return out
        End Function

        Public Iterator Function LoadFile(txt As String) As IEnumerable(Of Record)
            For Each data As String() In txt.ReadAllLines.Split("//")
                Dim annotationHeaders$() = Nothing
                Dim nodes As Dictionary(Of String, Node) = data.__loadSection(annotationHeaders)
                ' 文件区段读取完毕，开始生成数据对象
                Dim r As Record = nodes.__createObject(annotationHeaders)

                Yield r
            Next
        End Function

        <Extension>
        Private Function __createObject(nodes As Dictionary(Of String, Node), annotationHeaders$()) As Record
            Dim out As Record = DirectCast(GetType(Record).__createObject(nodes("$_")), Record)

            out.AC = DirectCast(GetType(AC).__createObject(nodes(NameOf(Record.AC))), AC)
            out.CH = DirectCast(GetType(CH).__createObject(nodes(NameOf(Record.CH))), CH)
            out.MS = DirectCast(GetType(DATA.MS).__createObject(nodes(NameOf(Record.MS))), DATA.MS)
            out.SP = DirectCast(GetType(SP).__createObject(nodes.TryGetValue(NameOf(Record.SP))), SP)
            out.PK = nodes(NameOf(Record.PK)).__createPeaksData(annotationHeaders)

            Return out
        End Function

        <Extension>
        Private Function __createPeaksData(node As Node, annotationHeaders$()) As PK
            Dim pk As New PK

            pk.NUM_PEAK = node.TryGetValue(NameOf(pk.NUM_PEAK)).DefaultFirst
            pk.SPLASH = node.TryGetValue(NameOf(pk.SPLASH)).DefaultFirst

            Try
                pk.ANNOTATION = node _
                    .TryGetValue(NameOf(pk.ANNOTATION)) _
                    .ParseAnnotations(annotationHeaders) _
                    .ToArray
            Catch ex As Exception

            End Try

            pk.PEAK = node(NameOf(pk.PEAK)) _
                .Select(Function(s$)
                            Dim t$() = s.Split
                            Dim i As i32 = Scan0

                            Return New PeakData With {
                                .mz = t(++i),
                                .int = t(++i),
                                .relint = t(++i)
                            }
                        End Function) _
                .ToArray

            Return pk
        End Function

        <Extension>
        Private Iterator Function ParseAnnotations(lines As IEnumerable(Of String), annotationHeaders$()) As IEnumerable(Of Entity)
            Dim i As Integer = -1

            For Each s As String In lines.SafeQuery
                Dim t$() = Strings.Trim(s).Split
                Dim table As PropertyValue() =
                    t.Where(Function(ss) Not ss.StringEmpty) _
                     .SeqIterator _
                     .Select(Function(k)
                                 Return New PropertyValue With {
                                    .Key = k.i,
                                    .Property = annotationHeaders(k),
                                    .Value = +k
                                 }
                             End Function) _
                     .ToArray

                i += 1

                Yield New Entity With {
                    .ID = i,
                    .Properties = table
                }
            Next
        End Function

        <Extension>
        Private Function __createObject(type As Type, node As Node) As Object
            Dim o As Object = Activator.CreateInstance(type)
            Dim schema = type.Schema(PropertyAccess.Writeable,, True)

            If node Is Nothing Then
                Return o
            End If

            For Each name$ In node.Keys
                If schema(name).PropertyType Is GetType(String) Then
                    Call schema(name).SetValue(o, node(name).FirstOrDefault)
                Else
                    Call schema(name).SetValue(o, node(name).ToArray)
                End If
            Next

            Return o
        End Function

        Const pk_annotation = "PK$ANNOTATION"

        <Extension>
        Private Function __loadSection(data$(), ByRef annotationHeaders$()) As Dictionary(Of String, Node)
            Dim nodes As New Dictionary(Of String, Node) From {
                {"$_", New Node},
                {"CH", New Node},
                {"AC", New Node},
                {"MS", New Node},
                {"PK", New Node},
                {"SP", New Node}
            }
            Dim table$ = ""
            Dim readTable As Boolean = False
            Dim appendNodeData =
            Sub(path$, value$)
                Dim nodeName As NamedValue(Of String) = path.GetTagValue("$")
                Dim node As Node = nodes(nodeName.Name)

                If Not node.ContainsKey(nodeName.Value) Then
                    node(nodeName.Value) = New List(Of String)
                End If

                node(nodeName.Value) += value
            End Sub

            For Each line$ In data
                Dim value As NamedValue(Of String) = line.GetTagValue(":", trim:=True)

                If readTable Then
                    If line.First = " "c OrElse Not (line.Contains("$") OrElse line.Contains(":")) Then
                        Call appendNodeData(table, value:=line.Trim)
                        Continue For
                    Else
                        readTable = False
                    End If
                End If

                If value.Name = pk_annotation OrElse value.Name = "PK$PEAK" Then
                    table = value.Name
                    readTable = True

                    If value.Name = pk_annotation Then
                        annotationHeaders = value.Value.Split
                    End If

                    Continue For
                End If

                If value.Name.Contains("$") Then
                    With value
                        Call appendNodeData(
                            path:= .Name,
                            value:= .Value
                        )
                    End With
                Else
                    If Not nodes("$_").ContainsKey(value.Name) Then
                        nodes("$_")(value.Name) = New List(Of String)
                    End If

                    nodes("$_")(value.Name) += value.Value
                End If
            Next

            Return nodes
        End Function
    End Module
End Namespace
