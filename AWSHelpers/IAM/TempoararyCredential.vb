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

Imports System.Runtime.Serialization
Namespace IAM

    <DataContract()> _
    Public Class TemporaryCredential
        Inherits CredentialManagement.Credential

        <DataMember()> _
        Public Property Key As String
        <DataMember()> _
        Public Property Secret As String
        <DataMember()> _
        Public Property Token As String


        ' Note that temporary credentials are ephemeral, and do not iterate versions.
        Public Sub New(ByRef name As String, ByRef key As String, ByRef secret As String, ByRef token As String)
            MyBase.New(name, 1)
        End Sub
        Public Sub New(ByRef name As String, ByRef instanceCred As Amazon.Runtime.ImmutableCredentials)
            MyBase.New(name, 1)

            Me.Key = instanceCred.AccessKey
            Me.Secret = instanceCred.SecretKey
            Me.Token = instanceCred.Token

        End Sub
        Public Overrides Function IsProper() As Boolean
            Return PropernessChecker(Me).Result
        End Function
        Public Shared Function PropernessChecker(ByRef cred As TemporaryCredential) As CredentialManagement.CredentialCheckerResult

            Dim returnValue As New CredentialManagement.CredentialCheckerResult
            If String.IsNullOrWhiteSpace(cred.Key) Or String.IsNullOrWhiteSpace(cred.Secret) Or String.IsNullOrWhiteSpace(cred.Token) Then
                returnValue.Result = False
                returnValue.ResultReason = "Key, Secret, and Token properties must not be null or empty."
            Else
                returnValue.Result = True
                returnValue.ResultReason = "Passed properness checks."
            End If
            Return returnValue
        End Function


    End Class
End Namespace

