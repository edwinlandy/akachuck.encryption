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

Imports akaChuck
Imports System.Text
Imports Amazon.S3.Model
<TestClass()>
Public Class TestS3ObjectSymmetricEncryption
    Private CachingDirectory As String = "C:\HelgaDogSolo\TestCachingDir"
    <TestMethod()>
    Public Sub TestS3Encryption_Decryption()
        Dim testBytes() As Byte = Encryption.PRNG.GetRandomBytes()
        Dim testStream As New IO.MemoryStream(testBytes)
        Dim password As New Encryption.SymmetricEncryptionSecretKeyPassword("TestPassword", 1, Encryption.PRNG.GetRandomBytes)
        Dim passwordList As New List(Of Encryption.SymmetricEncryptionSecretKeyPassword)
        passwordList.Add(password)

        ' Using AES256
        Dim cred As New Encryption.SymmetricEncryptionCredential(password, 2000, Encryption.EncryptionAlgorithm.AES256)

        ' Create test bucket
        Dim testBucketName As String = "HelgaDogSoloTest-S3ObjectSymmetricEncryption-1"
        Dim testObjectKey As String = Encryption.PRNG.GetRandomBase64Password(32, 32)

        CommonS3Tasks.CreateBucket(testBucketName)

        ' Put Object
        Dim putObject As New AWS.S3.PutObjectTransaction(testBucketName, testObjectKey, testStream, cred, CachingDirectory)
        putObject.ExecuteRequest()

        ' Get object
        Threading.Thread.Sleep(2000)

        Dim getObject As New AWS.S3.GetObjectTransaction(testBucketName, testObjectKey, passwordList)
        getObject.ExecuteRequest()

        Dim getObjStrm As New IO.MemoryStream
        getObject.ResponseStream.CopyTo(getObjStrm)

        Dim resBytes() As Byte = getObjStrm.ToArray

        CollectionAssert.AreEqual(testBytes, resBytes)

        'TODO: Test that object was encrypted as written to S3.

        ' Clean up
        CommonS3Tasks.DeleteObject(testBucketName, testObjectKey)
        CommonS3Tasks.DeleteBucket(testBucketName)

    End Sub


End Class
