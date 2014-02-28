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

Imports Amazon.S3
Imports Amazon.S3.Model
Public Class CommonS3Tasks
    Public Shared Function GetS3Client() As AmazonS3Client
        Return New Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.USEast1)

    End Function
    Public Shared Sub CreateBucket(bucketName As String)
        Dim s3Client As AmazonS3Client = GetS3Client()
        Dim req As New PutBucketRequest With
            {
                .BucketName = bucketName
            }
        Dim res As PutBucketResponse = s3Client.PutBucket(req)

    End Sub
    Public Shared Sub DeleteBucket(bucketName As String)
        Dim s3Client As AmazonS3Client = GetS3Client()
        Dim req As New DeleteBucketRequest With
            {
                .BucketName = bucketName
            }
        Dim res As DeleteBucketResponse = s3Client.DeleteBucket(req)
    End Sub
    Public Shared Sub DeleteObject(bucketName As String, objectKey As String)
        Dim req As New DeleteObjectRequest With
            {
                .BucketName = bucketName,
                .Key = objectKey
            }
        Dim res As DeleteObjectResponse = GetS3Client.DeleteObject(req)

    End Sub

    Public Shared Function GetPutObjectRequest(ByRef objectKey As String, ByRef bucketName As String) As PutObjectRequest
        Dim req As New PutObjectRequest With
            {
                .BucketName = bucketName,
                .Key = objectKey
            }
        Return req
    End Function
End Class
