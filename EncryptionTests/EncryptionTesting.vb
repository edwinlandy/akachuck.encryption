Imports System.Text
Imports Org.BouncyCastle.Crypto
Imports akaChuck
''' <summary>
''' Using the BouncyCastle Crypto Library to test akaChucks' implementation of  
''' the functionality found in .Net System.Security.Cryptography.
''' 
''' Bouncy Castle (http://bouncycastle.org/) uses the MIT license.
''' </summary>
''' <remarks></remarks>
<TestClass()>
Public Class EncryptionTesting

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()>
    Public Sub Test_KeyGeneration()
        '------------------------------------
        '----- 1. Get key with akaChuck -----
        '------------------------------------
        Dim secret1 As New Encryption.SymmetricEncryptionSecretKeyPassword("TestPassword", 1, Encryption.PRNG.GetRandomBytes)
        ' Use infrequently used iteration count to make sure we're not using an underlying default value.
        Dim cred1 As New akaChuck.Encryption.SymmetricEncryptionCredential(secret1, 1123)
        Dim akaChuckKey1() As Byte = cred1.Key

        '-----------------------------------------
        '----- 2. Get key with Bouncy Castle -----
        '-----------------------------------------
        Dim bcKeyDer As New Org.BouncyCastle.Crypto.Generators.Pkcs5S2ParametersGenerator()
        bcKeyDer.Init(secret1.Password, cred1.OvertParameters.Salt, cred1.OvertParameters.KeyIterations)
        Dim bcparam As Parameters.KeyParameter = bcKeyDer.GenerateDerivedParameters("aes256", 256)
        Dim bcKey1() As Byte = bcparam.GetKey()

        '-----------------------------------------
        '----- 3. See that they match ------------
        '-----------------------------------------

        CollectionAssert.AreEqual(bcKey1, akaChuckKey1)

        '--------------------------------------------------------------------
        '----- 3. Get key with akaChuck using different password ------------
        '--------------------------------------------------------------------

        Dim secret2 As New Encryption.SymmetricEncryptionSecretKeyPassword("TestPassword", 1, Encryption.PRNG.GetRandomBytes)
        Dim cred2 As New akaChuck.Encryption.SymmetricEncryptionCredential(secret2, cred1.OvertParameters, cred1.IV)
        Dim akaChuckKey2() As Byte = cred2.Key

        '-----------------------------------------
        '----- 3. See that they don't match ------
        '-----------------------------------------

        CollectionAssert.AreNotEqual(bcKey1, akaChuckKey2)

    End Sub

End Class
