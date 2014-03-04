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

Imports akaChuck.AWS.S3
Imports Amazon.S3.Model
Imports Amazon.S3

Namespace S3
    Public Class PutBucketTransaction
        Inherits S3Transaction(Of PutBucketRequest, PutBucketResponse)
        Public Sub New(ByRef bucketName As String, ByRef config As AWSClientConfig)
            MyBase.New(config)
            Me.Request = New PutBucketRequest With
                         {
                             .BucketName = bucketName
                         }
        End Sub

        Public Overrides Sub ExecuteRequest()
            Me.Response = Client.PutBucket(Request)
        End Sub
    End Class
    Public Class ListObjectsTransaction
        Inherits S3Transaction(Of ListObjectsRequest, ListObjectsResponse)


        Public ReadOnly Property ReturnedObjects As List(Of Model.S3Object)
            Get
                Return Me.Response.S3Objects
            End Get
        End Property

        Public Sub New(ByVal bucketName As String, config As AWSClientConfig)
            MyBase.New(config)
            Me.Request = New ListObjectsRequest With
                        {
                            .BucketName = bucketName
                        }
        End Sub

        Public Overrides Sub ExecuteRequest()
            Me.Response = Client.ListObjects(Me.Request)

        End Sub
    End Class

    Public Class PutBucketVersioningTransaction
        Inherits S3Transaction(Of PutBucketVersioningRequest, PutBucketVersioningResponse)
        Public Sub New(ByRef bucketName As String, ByVal versioningStatus As Amazon.S3.VersionStatus, ByRef config As AWSClientConfig)
            MyBase.New(config)

            Me.Request = New PutBucketVersioningRequest With
            {
                .BucketName = bucketName,
                .VersioningConfig = New S3BucketVersioningConfig With
                    {
                        .Status = versioningStatus
                    }
            }
        End Sub
        Public Overrides Sub ExecuteRequest()
            Me.Response = Client.PutBucketVersioning(Me.Request)
        End Sub
    End Class

    Public Class GetBucketVersioningTransaction
        Inherits S3Transaction(Of GetBucketVersioningRequest, GetBucketVersioningResponse)
        Public Sub New(ByRef bucketName As String, ByRef config As AWSClientConfig)
            MyBase.New(config)
            Me.Request = New GetBucketVersioningRequest With
                         {
                             .BucketName = bucketName
                         }
        End Sub
        Public Overrides Sub ExecuteRequest()
            Me.Response = Client.GetBucketVersioning(Me.Request)
        End Sub
    End Class

    Public Class DeleteBucketTransaction
        Inherits S3Transaction(Of DeleteBucketRequest, DeleteBucketResponse)
        Public Sub New(ByRef bucketName As String, config As AWSClientConfig)
            MyBase.New(config)
            Me.Request = New DeleteBucketRequest With
                         {
                             .BucketName = bucketName
                         }
        End Sub
        Public Overrides Sub ExecuteRequest()
            Me.Response = Client.DeleteBucket(Me.Request)
        End Sub
    End Class
End Namespace
