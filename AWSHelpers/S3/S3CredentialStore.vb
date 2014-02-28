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

Imports Amazon
Imports Amazon.S3
Imports akaChuck.Serialization
Imports akaChuck.Encryption
Imports akaChuck.CredentialManagement

Namespace S3
    Public Class S3CredentialStore(Of T As Credential)
        Implements CredentialStore(Of T)
        Private _BucketName As String

        Public ReadOnly Property BucketName As String
            Get
                Return _BucketName
            End Get
        End Property
        Private _BucketExists As Boolean
        Public ReadOnly Property BucketExists As Boolean
            Get
                Return _BucketExists
            End Get
        End Property
        Private ReadOnly Property S3Client As AmazonS3Client
            Get
                Return New AmazonS3Client(Amazon.RegionEndpoint.USEast1)
            End Get
        End Property
        ' The latest encryption credential with which to encrypt new credentials
        Private Property Cred As SymmetricEncryptionCredential
        ' The list of secret passwords that may be encountered when encrypting from store.
        Private Property PasswordList As List(Of SymmetricEncryptionSecretKeyPassword)
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bucket">Bucket where credentials are stored.</param>
        ''' <param name="passwordList">A list of passwords that may have been used to encrypted previously stored credentials.</param>
        ''' <param name="latestCred">The symmetric encryption credential with which new credentials are to be encrypted before stored.</param>
        ''' <remarks></remarks>
        Public Sub New(ByRef bucket As String, passwordList As List(Of SymmetricEncryptionSecretKeyPassword), latestCred As SymmetricEncryptionCredential)

            Me._BucketName = bucket

            ' Bucket must have versioning is enabled.
            Dim bucketVerReq As New Model.GetBucketVersioningRequest With
                       {
                           .BucketName = BucketName
                       }
            Try
                Dim bucketVerRes As Model.GetBucketVersioningResponse = S3Client.GetBucketVersioning(bucketVerReq)
                ' Exception would be thrown if bucket does not exist
                Me._BucketExists = True

                If Not bucketVerRes.VersioningConfig.Status = VersionStatus.Enabled Then
                    Throw New S3CredentialStoreException("S3CredentialStore requires S3Bucket to have versioning enabled.", _
                                                         S3CredentialStoreException.S3CredentialStoreExceptionReason.S3BucketVersioningNotEnabled, _
                                                         Nothing)
                End If

            Catch s3Ex As AmazonS3Exception
                If s3Ex.ErrorCode = "NoSuchBucket" Then
                    Me._BucketExists = False

                ElseIf s3Ex.ErrorCode = "AccessDenied" Then
                    Throw New S3CredentialStoreException("We do not have possession of bucket.", S3CredentialStoreException.S3CredentialStoreExceptionReason.S3BucketAccessDenied, s3Ex)
                Else
                    Throw s3Ex


                End If
            End Try

        End Sub
        Public Sub CreateBucket()

            ' Throw exception if bucket already exists.  Because of conditions implemented 
            ' in the constructor, this would only be the case if we owned the bucket that 
            ' already exists.
            If Me.BucketExists Then
                Throw New S3CredentialStoreException("Bucket already exists.", S3CredentialStoreException.S3CredentialStoreExceptionReason.S3BucketAlreadyExists, Nothing)
            End If

            Dim createBucketReq As New Model.PutBucketRequest With
            {
                .BucketName = BucketName
            }

            Dim createVersioningReq As New Model.PutBucketVersioningRequest With
                {
                    .BucketName = BucketName,
                    .VersioningConfig = New Model.S3BucketVersioningConfig With
                    {
                        .Status = VersionStatus.Enabled
                    }
                }

            Try
                Dim createBucketRes As Model.PutBucketResponse = S3Client.PutBucket(createBucketReq)
                Me._BucketExists = True
                Dim versioningRes As Model.PutBucketVersioningResponse = S3Client.PutBucketVersioning(createVersioningReq)
            Catch s3Ex As AmazonS3Exception
                If s3Ex.ErrorCode = "BucketAlreadyExists" Then
                    Throw New S3CredentialStoreException("Cannot create bucket. It already exists.", S3CredentialStoreException.S3CredentialStoreExceptionReason.S3BucketAlreadyExists, s3Ex)
                End If
            End Try

        End Sub
        Public Function ListCredentialNames() As System.Collections.Generic.List(Of String) Implements CredentialStore(Of T).ListCredentialNames
            Throw New NotImplementedException
        End Function

        Public Function ReadLatestFromStore(ByRef name As String) As StoredCredential(Of T) Implements CredentialStore(Of T).ReadLatestFromStore
            Dim service As New GetObjectTransaction(Me.BucketName, GetType(T).Name & "/" & name, passwordList)

            Try
                service.ExecuteRequest()
            Catch amazonEx As AmazonS3Exception
                If amazonEx.ErrorCode = "NoSuchKey" Then
                    Throw New CredentialNotStoredException(name, GetType(T))
                Else
                    Throw amazonEx
                End If

            End Try

            Dim storeCred As New StoredCredential(Of T) With
                {
                    .Credential = Json(Of T).FromStream(service.ResponseStream),
                    .LastStored = service.Response.LastModified.ToUniversalTime
                }
            Return storeCred

        End Function

        Public Function ReadVersionFromStore(ByRef name As String, version As Integer) As StoredCredential(Of T) Implements CredentialStore(Of T).ReadVersionFromStore
            Dim returnValue As StoredCredential(Of T) = Nothing
            Dim versionsList As List(Of StoredCredential(Of T)) = ReadVersionsFromStore(name)
            For Each v As StoredCredential(Of T) In versionsList
                If v.Credential.Version = version Then
                    returnValue = v
                    Exit For
                End If
            Next

            If returnValue Is Nothing Then
                Throw New CredentialNotStoredException(name, GetType(T), version)
            End If

            Return returnValue


        End Function

        Public Function ReadVersionsFromStore(ByRef name As String) As System.Collections.Generic.List(Of StoredCredential(Of T)) Implements CredentialStore(Of T).ReadVersionsFromStore
            Dim returnValue As New List(Of StoredCredential(Of T))
            Dim versionList As New List(Of Model.S3ObjectVersion)
            Dim versionReq As New Model.ListVersionsRequest With
                {
                    .BucketName = Me.BucketName,
                    .Prefix = GetType(T).Name & "/" & name
                }

            Dim versionRes As Model.ListVersionsResponse
            Try
                versionRes = S3Client.ListVersions(versionReq)
            Catch amazonEx As AmazonS3Exception
                If amazonEx.ErrorCode = "NoSuchKey" Then
                    Throw New CredentialNotStoredException(name, GetType(T))
                Else
                    Throw amazonEx
                End If

            End Try

            versionList.AddRange(versionRes.Versions)

            While Not versionRes.IsTruncated
                versionReq.KeyMarker = versionRes.NextKeyMarker
                versionRes = S3Client.ListVersions(versionReq)
                versionList.AddRange(versionRes.Versions)
            End While

            For Each v As Model.S3ObjectVersion In versionList
                Using getObjectTrans As New S3.GetObjectTransaction(Me.BucketName, v.Key, v.VersionId, PasswordList)
                    getObjectTrans.ExecuteRequest()

                    Dim storedCred As New StoredCredential(Of T) With
                        {
                            .Credential = Json(Of T).FromStream(getObjectTrans.ResponseStream),
                            .LastStored = getObjectTrans.Response.LastModified
                        }
                    returnValue.Add(storedCred)
                End Using

            Next

            ' Check to make sure name is what we're looking for, and not just a prefix.
            Dim itemsToRemove As New List(Of StoredCredential(Of T))

            For Each s As StoredCredential(Of T) In returnValue
                If Not s.Credential.Name = name Then
                    itemsToRemove.Add(s)
                End If
            Next

            For Each r As StoredCredential(Of T) In itemsToRemove
                returnValue.Remove(r)
            Next

            Return returnValue

        End Function

        Public Sub RemoveCredential(credential As T) Implements CredentialStore(Of T).RemoveCredential
            Throw New NotImplementedException
        End Sub

        Public Sub StoreCredential(credential As T) Implements CredentialStore(Of T).StoreCredential
            Dim unencryptedStrm As New System.IO.MemoryStream
            ' Fill the stream with the serialized credential
            Json(Of T).ToStream(credential, unencryptedStrm)

            Dim putObjectTrans As New PutObjectTransaction(Me.BucketName, GetType(T).Name & "/" & credential.Name, unencryptedStrm, Me.Cred, Nothing)

            putObjectTrans.Request.Metadata.Add("version", credential.Version.ToString)

            putObjectTrans.ExecuteRequest()


        End Sub


    End Class

    Public Class S3CredentialStoreException
        Inherits ApplicationException

        Public Enum S3CredentialStoreExceptionReason
            S3BucketDoesNotExist
            S3BucketAlreadyExists
            S3BucketVersioningNotEnabled
            S3BucketAccessDenied
        End Enum
        Public Property Reason As S3CredentialStoreExceptionReason
        Public Sub New(ByRef message As String, reason As S3CredentialStoreExceptionReason, ByRef innerException As Exception)
            MyBase.New(message, innerException)
            Me.Reason = reason


        End Sub
    End Class
End Namespace