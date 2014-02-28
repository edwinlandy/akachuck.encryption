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

Public Class CredentialManager(Of CRED As Credential, C As CredentialCache(Of CRED), S As {CredentialStore(Of CRED)})
    Private Property ReCacheInterval As TimeSpan
    Public Property Name As String
    Public Property Cache As C
    Public Property Store As S

    Public Sub New(ByRef name As String, ByRef cache As C, ByRef reCacheInterval As TimeSpan, ByRef store As S)
        Me.ReCacheInterval = reCacheInterval
        Me.Name = name
    End Sub

    Public Function GetCredential() As CRED
        Dim returnValue As CRED
        Dim needToCache As Boolean = False
        Dim cachedCredential As CachedCredential(Of CRED) = Cache.ReadFromCache(Name)

        If cachedCredential Is Nothing Then
            ' Has not been cached.
            needToCache = True
        Else
            If cachedCredential.TimeSinceCached > ReCacheInterval Then
                ' Need to re-cache
                needToCache = True
            End If
        End If

        If needToCache Then
            returnValue = Store.ReadLatestFromStore(Name).Credential
            Cache.CacheCredential(returnValue)
        Else
            returnValue = cachedCredential.Credential
        End If

        Return returnValue

    End Function

End Class
