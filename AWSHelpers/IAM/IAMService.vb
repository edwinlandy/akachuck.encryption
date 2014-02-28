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

Imports akaChuck.CredentialManagement
Imports akaChuck.AWS.IAM
Imports Amazon.IdentityManagement
Namespace IAM
    Public MustInherit Class IAMTransaction(Of Req As {Amazon.Runtime.AmazonWebServiceRequest}, Res As {Amazon.Runtime.AmazonWebServiceResponse})

        Public Property Client As Amazon.IdentityManagement.AmazonIdentityManagementServiceClient
        Public Property Request As Req
        Public Property Response As Res

        Protected Sub New(ByRef awsCredential As Amazon.Runtime.AWSCredentials)
            Me.Client = New Amazon.IdentityManagement.AmazonIdentityManagementServiceClient(awsCredential)
        End Sub
        Public MustOverride Sub ExecuteRequest()
    End Class

    Public Class ListAccessKeyTransaction
        Inherits IAMTransaction(Of Model.ListAccessKeysRequest, Model.ListAccessKeysResponse)

        Public Sub New(ByRef awsCred As UserCredential, Optional ByVal username As String = Nothing)
            MyBase.New(New Amazon.Runtime.BasicAWSCredentials(awsCred.Key, awsCred.Secret))
            Me.Request = New Model.ListAccessKeysRequest With
                         {
                             .UserName = username
                         }
        End Sub

        Public Sub New(ByRef awsCred As TemporaryCredential, Optional ByVal username As String = Nothing)
            MyBase.New(New Amazon.Runtime.SessionAWSCredentials(awsCred.Key, awsCred.Secret, awsCred.Token))
            Me.Request = New Model.ListAccessKeysRequest With
                         {
                             .UserName = username
                         }
        End Sub

        Public Overrides Sub ExecuteRequest()

            Me.Response = Client.ListAccessKeys(Me.Request)

        End Sub
    End Class
End Namespace