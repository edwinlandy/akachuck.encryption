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

Imports Amazon.Runtime
Public Class InstanceProfileCredentialStore
    ' Only one type of credential is provided by the Instance profile.
    Implements CredentialManagement.CredentialStore(Of IAM.TemporaryCredential)


    Public Function ListCredentialNames() As System.Collections.Generic.List(Of String) Implements CredentialManagement.CredentialStore(Of IAM.TemporaryCredential).ListCredentialNames
        ' Returns Roles available to the Instance
        Return InstanceProfileAWSCredentials.GetAvailableRoles
    End Function

    Public Function ReadLatestFromStore(ByRef name As String) As CredentialManagement.StoredCredential(Of IAM.TemporaryCredential) Implements CredentialManagement.CredentialStore(Of IAM.TemporaryCredential).ReadLatestFromStore
        Dim returnValue As New CredentialManagement.StoredCredential(Of IAM.TemporaryCredential)

        Dim instanceCreds As New InstanceProfileAWSCredentials(name)
        returnValue.LastStored = DateTime.UtcNow.Subtract(instanceCreds.PreemptExpiryTime)
        returnValue.Credential = New IAM.TemporaryCredential(instanceCreds.Role, instanceCreds.GetCredentials)

        Return returnValue

    End Function
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="name">Name of the temporary credential</param>
    ''' <param name="version">Temporary Credentials are ephemeral and do not itterate versions.  Property defauts to 1</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReadVersionFromStore(ByRef name As String, version As Integer) As CredentialManagement.StoredCredential(Of IAM.TemporaryCredential) Implements CredentialManagement.CredentialStore(Of IAM.TemporaryCredential).ReadVersionFromStore
        Throw New NotSupportedException("Temporary credentials are ephemeral and do not iterate versions. Versioning not available.")
    End Function

    Public Function ReadVersionsFromStore(ByRef name As String) As System.Collections.Generic.List(Of CredentialManagement.StoredCredential(Of IAM.TemporaryCredential)) Implements CredentialManagement.CredentialStore(Of IAM.TemporaryCredential).ReadVersionsFromStore
        Throw New NotSupportedException("Temporary credentials are ephemeral and do not iterate versions. Versioning not available.")
    End Function

    Public Sub RemoveCredential(credential As IAM.TemporaryCredential) Implements CredentialManagement.CredentialStore(Of IAM.TemporaryCredential).RemoveCredential
        Throw New NotSupportedException("Instance Profile is read only.  Updates not available.")
    End Sub

    Public Sub StoreCredential(credential As IAM.TemporaryCredential) Implements CredentialManagement.CredentialStore(Of IAM.TemporaryCredential).StoreCredential
        Throw New NotSupportedException("Instance Profile is read only.  Updates not available.")
    End Sub
End Class
