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

    <DataContract()>
    Public Class UserCredential
        Inherits CredentialManagement.Credential
        Implements IAWSCredential

        <DataMember()> _
        Public Property Key As String
        <DataMember()> _
        Public Property Secret As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="key">Access Key Id</param>
        ''' <param name="secret">Secret Access Key</param>
        ''' <param name="name">Name used to identify this credential.</param>
        ''' <param name="version">Incremental version of this credential.  Used for key rotation.</param>
        ''' <remarks></remarks>
        Public Sub New(ByRef key As String, ByRef secret As String, ByRef name As String, Optional ByVal version As Integer = 1)
            MyBase.New(name, version)
            Me.Key = key
            Me.Secret = secret

        End Sub

        Public Overrides Function IsProper() As Boolean
            Return PropernessChecker(Me).Result
        End Function

        Public Shared Function PropernessChecker(ByRef cred As UserCredential) As CredentialManagement.CredentialCheckerResult
            Dim returnValue As New CredentialManagement.CredentialCheckerResult
            If String.IsNullOrWhiteSpace(cred.Key) Or String.IsNullOrWhiteSpace(cred.Secret) Then
                returnValue.Result = False
                returnValue.ResultReason = "Both Key and Secret values must not be null or empty."
            Else
                returnValue.Result = True
                returnValue.ResultReason = "Properness checker passed."
            End If
            Return returnValue
        End Function
        Public Shared Function CurrentnessChecker(ByRef cred As UserCredential) As CredentialManagement.CredentialCheckerResult

            Throw New NotImplementedException



        End Function

        Public Function GetAWSCredentials() As Amazon.Runtime.AWSCredentials Implements IAWSCredential.GetAWSCredentials
            Return New Amazon.Runtime.BasicAWSCredentials(Me.Key, Me.Secret)
        End Function
    End Class
End Namespace