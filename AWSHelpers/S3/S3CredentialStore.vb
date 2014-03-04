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
        Private Property ClientConfig As AWS.AWSClientConfig

        ' The latest encryption credential with which to encrypt new credentials
        Private Property EncryptionCred As SymmetricEncryptionCredential
        ' The list of secret passwords that may be encountered when encrypting from store.
        Private Property PasswordList As List(Of SymmetricEncryptionSecretKeyPassword)
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bucketName">Bucket where credentials are stored.</param>
        ''' <param name="passwordList">A list of passwords that may have been used to encrypted previously stored credentials.</param>
        ''' <param name="latestCred">The symmetric encryption credential with which new credentials are to be encrypted before stored.</param>
        ''' <remarks></remarks>
        Public Sub New(ByRef bucketName As String, passwordList As List(Of SymmetricEncryptionSecretKeyPassword), latestCred As SymmetricEncryptionCredential, ByRef clientConfig As AWSClientConfig)
            Me.ClientConfig = clientConfig
            Me._BucketName = bucketName

            ' Bucket must have versioning is enabled.
            Dim bucketVerTrans As New GetBucketVersioningTransaction(bucketName, clientConfig)
            Try
                bucketVerTrans.ExecuteRequest()
                ' Exception would be thrown if bucket does not exist
                Me._BucketExists = True

                If Not bucketVerTrans.Response.VersioningConfig.Status = Amazon.S3.VersionStatus.Enabled Then
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

            Dim putBucketTrans As New PutBucketTransaction(BucketName, ClientConfig)
            Dim putVersioningTrans As New PutBucketVersioningTransaction(BucketName, Amazon.S3.VersionStatus.Enabled, ClientConfig)

            Try
                putBucketTrans.ExecuteRequest()
                Me._BucketExists = True
                putVersioningTrans.ExecuteRequest()

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
            Dim service As New GetObjectTransaction(Me.BucketName, GetObjectKey(name), PasswordList, ClientConfig)

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

            Dim versionTrans As New ListVersionsTransaction(BucketName, GetObjectKey(name), ClientConfig)

            Try
                versionTrans.ExecuteRequest()

            Catch amazonEx As AmazonS3Exception
                If amazonEx.ErrorCode = "NoSuchKey" Then
                    Throw New CredentialNotStoredException(name, GetType(T))
                Else
                    Throw amazonEx
                End If

            End Try

            versionList.AddRange(versionTrans.Response.Versions)

            While versionTrans.Response.IsTruncated
                versionTrans.Request.KeyMarker = versionTrans.Response.NextKeyMarker
                versionTrans.ExecuteRequest()
                versionList.AddRange(versionTrans.Response.Versions)
            End While

            For Each v As Model.S3ObjectVersion In versionList
                Using getObjectTrans As New S3.GetObjectTransaction(Me.BucketName, v.Key, v.VersionId, PasswordList, ClientConfig)
                    getObjectTrans.ExecuteRequest()

                    Dim storedCred As New StoredCredential(Of T) With
                        {
                            .Credential = Json(Of T).FromStream(getObjectTrans.ResponseStream),
                            .LastStored = getObjectTrans.Response.LastModified
                        }

                    ' Check to make sure name is what we're looking for, and not just a prefix.
                    If storedCred.Credential.Name = name Then
                        returnValue.Add(storedCred)
                    End If

                End Using

            Next

            Return returnValue

        End Function

        Public Sub RemoveCredential(ByRef name As String) Implements CredentialStore(Of T).RemoveCredential
            Dim versionList As New List(Of Model.S3ObjectVersion)
            ' GetVersions
            Dim listVerTrans As New ListVersionsTransaction(Me.BucketName, GetObjectKey(name), Me.ClientConfig)
            listVerTrans.ExecuteRequest()

            versionList.AddRange(listVerTrans.Response.Versions)

            While listVerTrans.Response.IsTruncated
                listVerTrans.Request.KeyMarker = listVerTrans.Response.NextKeyMarker
                listVerTrans.ExecuteRequest()
                versionList.AddRange(listVerTrans.Response.Versions)
            End While

            ' Delete Version, if exists
            For Each v As Model.S3ObjectVersion In versionList
                ' Have to test objectKey, because S3 only allows to supply key prefix to the request.
                ' other names that start with the name we are deleting are included in the request.
                If v.Key = GetObjectKey(name) Then
                    Dim delTrans As New DeleteObjectTransaction(BucketName, GetObjectKey(name), ClientConfig, v.VersionId)
                    delTrans.ExecuteRequest()
                End If
            Next

        End Sub

        Public Sub StoreCredential(credential As T) Implements CredentialStore(Of T).StoreCredential
            Dim unencryptedStrm As New System.IO.MemoryStream
            ' Fill the stream with the serialized credential
            Json(Of T).ToStream(credential, unencryptedStrm)

            Dim putObjectTrans As New PutObjectTransaction(Me.BucketName, GetObjectKey(credential.Name), unencryptedStrm, ClientConfig)

            putObjectTrans.Request.Metadata.Add("version", credential.Version.ToString)

            putObjectTrans.ExecuteRequest()
        End Sub

        Private Function GetObjectKey(ByRef credentialName As String) As String
            Return GetType(T).Name & "/" & credentialName
        End Function
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