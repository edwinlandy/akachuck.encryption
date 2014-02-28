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

Imports System.Text
Imports Amazon
Imports Amazon.S3
Imports akaChuck.AWS.S3
Imports akaChuck.AWS.IAM

Imports akaChuck.CredentialManagement

<TestClass()>
Public Class TestS3CredentialStore


    Private S3Client As New Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.USEast1)


    <TestMethod()>
    Public Sub Test_S3CredentialStore_BucketCreation()
       
        Dim password As New akaChuck.Encryption.SymmetricEncryptionSecretKeyPassword("TestPassword", 1, akaChuck.Encryption.PRNG.GetRandomBytes)
        Dim passwordList As New List(Of akaChuck.Encryption.SymmetricEncryptionSecretKeyPassword)
        passwordList.Add(password)
        Dim cred As New akaChuck.Encryption.SymmetricEncryptionCredential(password, 1000, akaChuck.Encryption.EncryptionAlgorithm.AES256)


        '------------------------------------------
        '----- 1. BucketExists Property works.-----
        '------------------------------------------
        ' Bucket does not already exist.
        Dim store1 As New S3CredentialStore(Of UsernamePasswordCredential)("HelgaDogSoloTest-S3CredentialStore-BucketCreation-1", passwordList, cred)
        Assert.IsFalse(store1.BucketExists)

        ' Create proper Bucket.
        store1.CreateBucket()
        Assert.IsTrue(store1.BucketExists)

        '----------------------------------------------------------------------------
        '----- 2. Constructor throws expected exception when bucket is improper.-----
        '----------------------------------------------------------------------------
        ' Create improper Bucket with no versioning
        Dim req2 As New Amazon.S3.Model.PutBucketRequest With
            {
                .BucketName = "HelgaDogSoloTest-S3CredentialStore-BucketCreation-2"
            }
        Dim res2 As Amazon.S3.Model.PutBucketResponse = Me.S3Client.PutBucket(req2)


        Try
            ' Should throw exception when bucket that exists is not proper.
            Dim store2 As New S3CredentialStore(Of UsernamePasswordCredential)(req2.BucketName, passwordList, cred)
            ' Execution should not make it here.
            Assert.Fail()

        Catch credStoreEx As S3CredentialStoreException
            Assert.AreEqual(S3CredentialStoreException.S3CredentialStoreExceptionReason.S3BucketVersioningNotEnabled, credStoreEx.Reason)
        Catch ex As Exception
            ' Exception should be S3CredentialStoreException
            Assert.Fail()
        End Try

        '----------------------------------------------------------------------------
        '----- 3. Constructor throws exception when bucket is owned not by us.-------
        '----------------------------------------------------------------------------

        Try
            ' Should throw exception - This test depends on the existance of the bucket on another account
            Dim store3 As New S3CredentialStore(Of UsernamePasswordCredential)("HelgaDogSoloTest-S3CredentialStore-BucketCreation-3", passwordList, cred)

            ' Code execution should not make it here
            Assert.Fail()
        Catch s3StoreEx As S3CredentialStoreException
            Assert.AreEqual(S3CredentialStoreException.S3CredentialStoreExceptionReason.S3BucketAccessDenied, s3StoreEx.Reason)

        Catch ex As Exception
            Assert.Fail("This is not the exception we were looking for.")
        End Try

        '----------------------------------------------------------------------------
        '----- 4. CreateBucket throws exception when bucket already exists.----------
        '----------------------------------------------------------------------------

        Try
            ' Should throw exception
            store1.CreateBucket()

            ' Code Execution should not make it this far
            Assert.Fail()
        Catch s3StoreEx As S3CredentialStoreException
            Assert.AreEqual(S3CredentialStoreException.S3CredentialStoreExceptionReason.S3BucketAlreadyExists, s3StoreEx.Reason)
        Catch ex As Exception
            Assert.Fail("This is not the exception we were looking for.")
        End Try

        ' Clean Up
        ' Delete buckets
        Dim delreq As New Model.DeleteBucketRequest With
            {
                .BucketName = store1.BucketName
                }
        S3Client.DeleteBucket(delreq)
        delreq.BucketName = req2.BucketName
        S3Client.DeleteBucket(delreq)

    End Sub

End Class
