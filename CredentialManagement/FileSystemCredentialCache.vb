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

Imports akaChuck.Serialization

Public Class FileSystemCredentialCache(Of T As Credential)
    Implements CredentialCache(Of T)

    Private Property CacheTypeDirectory As IO.DirectoryInfo

    Public Sub New(ByRef cacheDirectory As String)

        ' See if directory exists
        Dim cacheDir As New IO.DirectoryInfo(cacheDirectory)
        If Not cacheDir.Exists Then
            ' Create the directory
            cacheDir.Create()
        End If
        ' See if credential type's subdirectory exists
        Dim subDir As New IO.DirectoryInfo(IO.Path.Combine(cacheDir.FullName, GetType(T).Name))

        If Not subDir.Exists Then
            ' Create the directory
            subDir.Create()
        End If

        Me.CacheTypeDirectory = subDir

    End Sub

    Public Sub CacheCredential(ByRef credential As T) Implements CredentialCache(Of T).CacheCredential

        Dim fileName As String = credential.Name & "-" & credential.Version.ToString
        Dim filePath As String = IO.Path.Combine(CacheTypeDirectory.FullName, fileName)

        Json(Of T).ToFile(credential, filePath)

    End Sub
    Public Sub DeleteCachedCredentials() Implements CredentialCache(Of T).DeleteCachedCredentials
        Throw New NotImplementedException
    End Sub
    Function ReadAllFromCache() As List(Of CachedCredential(Of T)) Implements CredentialCache(Of T).ReadAllFromCache
        Dim credList As New List(Of CachedCredential(Of T))
        Dim files As IO.FileInfo() = Me.CacheTypeDirectory.GetFiles()

        ' Will be an empty array if there are no files in the directory
        If files.Length = 0 Then
            '!!! Throw exception
            Throw New CredentialNotCachedException("all names", GetType(T))
        End If

        For Each f As IO.FileInfo In files
            Dim cc As New CachedCredential(Of T) With
                {
                    .Credential = Json(Of T).FromFile(f.FullName),
                    .TimeCached = f.LastWriteTimeUtc
                    }
            credList.Add(cc)
        Next

        Return credList

    End Function
    Function ReadAllFromCache(ByRef name As String) As List(Of CachedCredential(Of T)) Implements CredentialCache(Of T).ReadAllFromCache
        Dim credList As New List(Of CachedCredential(Of T))
        Dim files As IO.FileInfo() = Me.CacheTypeDirectory.GetFiles(name & "-*")
        ' Will be an empty array if there are no files in the directory
        If files.Length = 0 Then
            '!!! Throw exception
            Throw New CredentialNotCachedException(name, GetType(T))
        End If
        For Each f As IO.FileInfo In files
            Dim cc As New CachedCredential(Of T) With
                {
                    .Credential = Json(Of T).FromFile(f.FullName),
                    .TimeCached = f.LastWriteTimeUtc
                    }
            credList.Add(cc)
        Next

        Return credList
    End Function
    ''' <summary>
    ''' Gets the last version of the credential referenced by provided name.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReadFromCache(ByRef name As String) As CachedCredential(Of T) Implements CredentialCache(Of T).ReadFromCache
        Dim credList As New List(Of CachedCredential(Of T))
        Dim files As IO.FileInfo() = Me.CacheTypeDirectory.GetFiles(name & "-*")
        ' Will be an empty array if there are no files in the directory
        If files.Length = 0 Then
            '!!! Throw exception
            Throw New CredentialNotCachedException(name, GetType(T))
        End If
        For Each f As IO.FileInfo In files
            Dim cc As New CachedCredential(Of T) With
                {
                    .Credential = Json(Of T).FromFile(f.FullName),
                    .TimeCached = f.LastWriteTimeUtc
                    }
            credList.Add(cc)
        Next

        credList.Sort()

        If credList.Count > 0 Then
            Return credList(credList.Count - 1)
        Else
            Return Nothing
        End If

    End Function
    Public Function ReadFromCache(ByRef name As String, version As Integer) As CachedCredential(Of T) Implements CredentialCache(Of T).ReadFromCache

        Dim fileName As String = name & "-" & version
        Dim fileInfo As New IO.FileInfo(IO.Path.Combine(CacheTypeDirectory.FullName, fileName))

        If fileInfo.Exists Then
            Dim credential As T = Json(Of T).FromFile(fileInfo.FullName)
            Dim timeCached As DateTime = fileInfo.LastWriteTimeUtc

            Dim returnValue As New CachedCredential(Of T) With
                {
                    .Credential = credential,
                    .TimeCached = timeCached
                }
            Return returnValue
        Else
            '!!! Throw exception
                Throw New CredentialNotCachedException(name, GetType(T))
        End If

    End Function

    Public Function IsCached(ByRef name As String, ByVal version As Integer) As Boolean Implements CredentialCache(Of T).IsCached

        Throw New NotImplementedException
    End Function


End Class
Public Class CredentialNotCachedException
    Inherits ApplicationException

    Public Property CredentialName As String
    Public Property CredentialType As System.Type

    Public Sub New(credentialName As String, credentialType As System.Type)
        MyBase.New("Credential " & credentialName & " of type " & credentialType.Name & " is not cached.")

        Me.CredentialName = credentialName
        Me.CredentialType = credentialType


    End Sub
End Class