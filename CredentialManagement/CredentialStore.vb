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

Public Interface CredentialStore(Of T As akaChuck.CredentialManagement.Credential)

    Function ReadVersionFromStore(ByRef name As String, ByVal version As Integer) As StoredCredential(Of T)
    Function ReadVersionsFromStore(ByRef name As String) As List(Of StoredCredential(Of T))
    Function ReadLatestFromStore(ByRef name As String) As StoredCredential(Of T)
    Sub StoreCredential(ByVal credential As T)
    Sub RemoveCredential(ByRef name As String)
    Function ListCredentialNames() As List(Of String)

End Interface

Public Class StoredCredential(Of T As akaChuck.CredentialManagement.Credential)
    Public Property Credential As T
    Public Property LastStored As DateTime
    Public ReadOnly Property TimeSinceStored As TimeSpan
        Get
            Return DateTime.UtcNow.Subtract(LastStored)
        End Get
    End Property
End Class
Public Class CredentialNotStoredException
    Inherits ApplicationException

    Public Sub New(ByRef credentialName As String, ByRef credentialType As System.Type)
        MyBase.New("Could not get credential named " & credentialName & " of type " & credentialType.Name & ".")

    End Sub
    Public Sub New(ByRef credentialName As String, ByRef credentialType As System.Type, ByVal version As Integer)
        MyBase.New("Could not get credential named " & credentialName & " of type " & credentialType.Name & " of version " & version.ToString & ".")
    End Sub
End Class