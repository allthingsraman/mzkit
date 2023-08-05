﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.My.JavaScript
Imports Microsoft.VisualBasic.Net.Http

Namespace PoolData

    Public Class Restful

        Public Property code As Integer
        Public Property debug As Object
        Public Property info As Object

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ParseJSON(json As WebResponseResult) As Restful
            If json Is Nothing Then
                Return New Restful With {.code = -1, .info = Nothing}
            Else
                Return ParseJSON(json.html)
            End If
        End Function

        Public Shared Function ParseJSON(json As String) As Restful
            If json.StringEmpty OrElse json.TextEquals("null") Then
                Return New Restful With {.code = -1, .info = Nothing}
            Else
                Dim obj As JavaScriptObject = DirectCast(JsonParser.Parse(json), JsonObject)
                Dim code As String = obj!code

                Return New Restful With {
                    .code = Integer.Parse(code),
                    .debug = obj!debug,
                    .info = obj!info
                }
            End If
        End Function

    End Class
End Namespace