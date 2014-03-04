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
' along with this program.  If not, see <http://www.gnu.org/licenses/>.)

Imports Amazon.S3.Model

Namespace S3
    Public MustInherit Class S3Transaction(Of Req As {Amazon.Runtime.AmazonWebServiceRequest}, Res As {Amazon.Runtime.AmazonWebServiceResponse})
        Inherits AWSTransaction(Of Req, Res, Amazon.S3.AmazonS3Client)
        Protected Sub New(ByRef config As AWSClientConfig)
            MyBase.New(New Amazon.S3.AmazonS3Client(config.Credential.GetAWSCredentials, config.Region))
        End Sub
    End Class
End Namespace

