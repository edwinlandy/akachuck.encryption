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
        Inherits AWSTransaction(Of Req, Res, Amazon.IdentityManagement.AmazonIdentityManagementServiceClient)

        Protected Sub New(ByRef cred As IAWSCredential)
            MyBase.New(New AmazonIdentityManagementServiceClient(cred.GetAWSCredentials))
        End Sub
    
    End Class

    Public Class ListAccessKeyTransaction
        Inherits IAMTransaction(Of Model.ListAccessKeysRequest, Model.ListAccessKeysResponse)

        Public Sub New(ByRef cred As IAWSCredential, Optional ByVal username As String = Nothing)
            MyBase.New(cred)
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