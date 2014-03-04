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

Public Interface CredentialCache(Of T As Credential)

    Sub CacheCredential(ByRef credential As T)
    Sub DeleteCachedCredentials()
    Function IsCached(ByRef name As String, ByVal version As Integer) As Boolean
    Function ReadAllFromCache() As List(Of CachedCredential(Of T))
    Function ReadAllFromCache(ByRef name As String) As List(Of CachedCredential(Of T))
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <returns></returns>
    ''' <remarks>Gets latest version of credential with name specified for credential type.</remarks>
    Function ReadFromCache(ByRef name As String) As CachedCredential(Of T)
    ''' <summary>
    ''' Gets credential with name and version specified for credential type.
    ''' </summary>
    ''' <param name="Name"></param>
    ''' <param name="version"></param>
    ''' <returns></returns>
    ''' <exception cref="CredentialNotCachedException">Thrown if credential is not cached at specified location.</exception>
    ''' <remarks>Gets credential with name and version specified for credential type.-</remarks>
    Function ReadFromCache(ByRef name As String, ByVal version As Integer) As CachedCredential(Of T)

End Interface
Public Class CachedCredential(Of T As Credential)
    Implements IComparable(Of CachedCredential(Of T))

    Public Property Credential As T
    Public Property TimeCached As DateTime
    Public ReadOnly Property TimeSinceCached As TimeSpan
        Get
            Return DateTime.UtcNow.Subtract(TimeCached)
        End Get
    End Property

    Public Function CompareTo(other As CachedCredential(Of T)) As Integer Implements System.IComparable(Of CachedCredential(Of T)).CompareTo
        ' Sort Items by the version of the credential.
        Return Me.Credential.Version - other.Credential.Version
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

