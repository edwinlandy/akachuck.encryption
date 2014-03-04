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

Imports Amazon.S3.Model
Imports akaChuck.Encryption
Imports akaChuck.Serialization

Namespace S3

    Public Class PutObjectTransaction
        Inherits S3Transaction(Of PutObjectRequest, PutObjectResponse)
        Private _EncryptedTransaction As Boolean
        Private Cred As Encryption.SymmetricEncryptionCredential
        Private CachingDirectory As String

        Public Property BucketName As String
        Public Property ObjectKey As String
        Public ReadOnly Property EncryptedTransaction As Boolean
            Get
                Return _EncryptedTransaction
            End Get
        End Property
        Public Property RequestStream As IO.Stream

        Public Sub New(ByRef bucketName As String, ByRef objectKey As String, ByRef inputStream As IO.Stream, ByVal config As AWSClientConfig)
            MyBase.New(config)
            Me._EncryptedTransaction = False

            Me.Request = New PutObjectRequest With
                         {
                             .BucketName = bucketName,
                             .Key = objectKey
                         }
            Me.RequestStream = inputStream

        End Sub
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bucketName">The bucket to store to object in.</param>
        ''' <param name="objectKey">The object's key.</param>
        ''' <param name="unencryptedStream">The unencrypted stream to encrypt and store in S3.</param>
        ''' <param name="encryptionCred">The symmetric encryption credential to use.</param>
        ''' <param name="cachingDirectory">Streams too big to fit in memory must be cached to file.  The file system directory to cache the encrypted data in. 
        ''' Pass Null value to use system memory instead.</param>
        ''' <remarks></remarks>
        Public Sub New(ByRef bucketName As String, ByRef objectKey As String, ByRef unencryptedStream As IO.Stream, _
                       ByRef encryptionCred As Encryption.SymmetricEncryptionCredential, ByRef config As AWSClientConfig, ByRef cachingDirectory As String)
            Me.New(bucketName, objectKey, unencryptedStream, config)
            Me._EncryptedTransaction = True

            ' AWS S3 will be receiving an encrypted file.
            Me.Request.ContentType = "application/octet-stream"
            Me.Request.Metadata.Add("overtkeyparameters", Json(Of SymmetricEncryptionOvertKeyParameters).GetBase64String(encryptionCred.OvertParameters))
            Me.Request.Metadata.Add("iv", Convert.ToBase64String(encryptionCred.IV))
            Me.CachingDirectory = cachingDirectory
            Me.Cred = encryptionCred
        End Sub

        Public Overrides Sub ExecuteRequest()
            If Me.EncryptedTransaction Then
                If Me.CachingDirectory Is Nothing Then
                    ' Encrypt stream to memory stream.  AWS SDK needs a seekable stream.
                    Using ms As New IO.MemoryStream
                        Using encryptstrm As New SymmetricEncryptorStream(Me.RequestStream, Me.Cred)
                            encryptstrm.CopyTo(ms)
                            Me.Request.InputStream = ms
                            Me.Response = Client.PutObject(Request)
                        End Using
                    End Using
                Else
                    ' Encrypt stream to file.  AWS SDK needs a seekable stream, so we are using a file stream.
                    Dim tempFullPath As String = IO.Path.Combine(Me.CachingDirectory, IO.Path.GetRandomFileName)

                    Using fs As New IO.FileStream(tempFullPath, IO.FileMode.Create)
                        Using encryptedStrm As New Encryption.SymmetricEncryptorStream(Me.RequestStream, Me.Cred)
                            encryptedStrm.CopyTo(fs, 8192)
                        End Using
                    End Using

                    Me.Request.FilePath = tempFullPath

                    Response = Client.PutObject(Request)

                    ' Delete the temporary file
                    IO.File.Delete(tempFullPath)
                End If

            Else
                Me.Request.InputStream = Me.RequestStream
                Response = Client.PutObject(Request)
            End If

        End Sub
    End Class

    Public Class GetObjectTransaction
        Inherits S3Transaction(Of GetObjectRequest, GetObjectResponse)
        Implements IDisposable

        Private _EncryptedTransaction As Boolean
        Private Property PasswordList As List(Of Encryption.SymmetricEncryptionSecretKeyPassword)
        Public ReadOnly Property EncryptedTransaction As Boolean
            Get
                Return _EncryptedTransaction
            End Get
        End Property
        Public Property ResponseStream As IO.Stream

        ' Standard Request
        Public Sub New(ByRef bucketName As String, ByRef objectKey As String, ByRef config As AWSClientConfig)
            MyBase.New(config)
            _EncryptedTransaction = False


            Request = New GetObjectRequest With
                {
                    .BucketName = bucketName,
                    .Key = objectKey
                }

        End Sub
        ' Standard Request with version
        Public Sub New(ByRef bucketName As String, ByRef objectKey As String, ByRef versionId As String, ByRef config As AWSClientConfig)
            MyBase.New(config)
            _EncryptedTransaction = False

            Request = New GetObjectRequest With
                {
                    .BucketName = bucketName,
                    .Key = objectKey,
                    .VersionId = versionId
                }

        End Sub
        ' Encrypted request
        Public Sub New(ByRef bucketName As String, ByRef objectkey As String, ByRef passwordList As List(Of Encryption.SymmetricEncryptionSecretKeyPassword), ByRef config As AWSClientConfig)
            MyBase.New(config)
            _EncryptedTransaction = True
            Request = New GetObjectRequest With
                {
                    .BucketName = bucketName,
                    .Key = objectkey
                }
            Me.PasswordList = passwordList
        End Sub
        ' Encrypted request with version
        Public Sub New(ByRef bucketName As String, ByRef objectkey As String, ByRef versionId As String, ByRef passwordList As List(Of Encryption.SymmetricEncryptionSecretKeyPassword), ByRef config As AWSClientConfig)
            MyBase.New(config)
            _EncryptedTransaction = True
            Request = New GetObjectRequest With
                {
                    .BucketName = bucketName,
                    .Key = objectkey,
                    .VersionId = versionId
                }
            Me.PasswordList = passwordList
        End Sub
        Public Overrides Sub ExecuteRequest()

            If Me.EncryptedTransaction Then
                Response = Client.GetObject(Request)
                Dim overtParams As SymmetricEncryptionOvertKeyParameters = Serialization.Json(Of SymmetricEncryptionOvertKeyParameters).FromBase64String(Response.Metadata("overtkeyparameters"))
                Dim IV() As Byte = Convert.FromBase64String(Response.Metadata("iv"))
                Dim cred As New SymmetricEncryptionCredential(PasswordList, overtParams, IV)
                Me.ResponseStream = New SymmetricDecryptorStream(Response.ResponseStream, cred)

            Else
                Response = Client.GetObject(Request)
                ResponseStream = Response.ResponseStream
            End If

        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Me.ResponseStream.Dispose()
                End If
            End If
            Me.disposedValue = True
        End Sub


        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

    Public Class DeleteObjectTransaction
        Inherits S3Transaction(Of DeleteObjectRequest, DeleteObjectResponse)
        Public Sub New(ByRef bucketName As String, ByRef objectKey As String, ByRef config As AWSClientConfig, Optional ByRef versionId As String = Nothing)
            MyBase.New(config)
            Me.Request = New DeleteObjectRequest With
                         {
                             .BucketName = bucketName,
                             .Key = objectKey,
                             .VersionId = versionId
                         }
        End Sub

        Public Overrides Sub ExecuteRequest()
            Me.Response = Client.DeleteObject(Me.Request)
        End Sub
    End Class

    Public Class ListVersionsTransaction
        Inherits S3Transaction(Of ListVersionsRequest, ListVersionsResponse)
        Public Sub New(ByRef bucketName As String, ByRef objectKeyPrefix As String, ByRef config As AWSClientConfig)
            MyBase.New(config)
            Me.Request = New ListVersionsRequest With
                         {
                             .BucketName = bucketName,
                             .Prefix = objectKeyPrefix
                         }
        End Sub
        Public Overrides Sub ExecuteRequest()
            Me.Response = Me.Client.ListVersions(Me.Request)
        End Sub
    End Class

End Namespace

