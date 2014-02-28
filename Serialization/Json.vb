' Copyright (C) 2014 a.k.a. Chuck, Inc.
'
' Authored by Edwin Landy - edwin@akaChuck.com.
'
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Text
Imports System.Runtime.Serialization.Json

Public Class Json(Of T)
    Public Shared Function FromStream(ByRef serializedObject As IO.Stream) As T

        Dim ds As New DataContractJsonSerializer(GetType(T))
        Return ds.ReadObject(serializedObject)

    End Function
 
    Public Shared Function FromFile(ByRef path As String) As T
        Using fs As New IO.FileStream(path, IO.FileMode.Open)
            Return FromStream(fs)
        End Using
    End Function

    Public Shared Function FromBytes(ByRef serializedObject As Byte()) As T
        Using ms As New IO.MemoryStream(serializedObject)
            Return FromStream(ms)
        End Using
    End Function
    Public Shared Function FromBase64String(ByRef serializedObject As String) As T
        Return FromBytes(Convert.FromBase64String(serializedObject))
    End Function
    Public Shared Function FromUTF8String(ByRef serializedObject As String) As T
        Return FromBytes(System.Text.Encoding.UTF8.GetBytes(serializedObject))
    End Function
    ''' <summary>
    ''' Gets a stream containing the JSON-serialized object.
    ''' </summary>
    ''' <param name="objectToSerialize"></param>
    ''' <param name="strm">Memory stream that will contain the serialized object.</param>
    ''' <remarks></remarks>
    Public Shared Sub ToStream(ByRef objectToSerialize As T, ByRef strm As IO.Stream)
        Dim s As New DataContractJsonSerializer(GetType(T))
        s.WriteObject(strm, objectToSerialize)
        strm.Position = 0
    End Sub

    Public Shared Sub ToFile(objectToSerialize As T, filePath As String)
        Using fs As IO.FileStream = IO.File.Create(filePath)
            Using ms As New IO.MemoryStream
                ToStream(objectToSerialize, ms)
                ms.CopyTo(fs)
            End Using
        End Using
    End Sub
    Public Shared Function GetBytes(objectToSerialize As T) As Byte()
        Dim returnValue As Byte()
        Using ms As New IO.MemoryStream
            ToStream(objectToSerialize, ms)
            returnValue = ms.ToArray
        End Using

        Return returnValue
    End Function
    Public Shared Function GetBase64String(objectToSerialize As T) As String
        Return Convert.ToBase64String(GetBytes(objectToSerialize))
    End Function
    Public Shared Function GetUTF8String(objectToSerialize As T) As String
        Return Encoding.UTF8.GetString(GetBytes(objectToSerialize))
    End Function


End Class
