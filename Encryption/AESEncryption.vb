Public Class AESEncryption
    Public Shared Function Encrypt(ByRef unencryptedValue As String, ByRef creds As SymmetricEncryptionCredential) As Byte()
        Dim inBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(unencryptedValue)
        Dim outbytes() As Byte = Encrypt(inBytes, creds)

        Return outbytes
    End Function
    Public Shared Function Encrypt(ByRef unencryptedValue As Byte(), ByRef creds As SymmetricEncryptionCredential) As Byte()

        Dim outBytes() As Byte

        Using inStream As New IO.MemoryStream(unencryptedValue)
            Using outStream As New IO.MemoryStream()
                EncryptToStream(inStream, outStream, creds)
                outBytes = outStream.ToArray
            End Using
        End Using

        Return outBytes

    End Function
    Public Shared Function Encrypt(ByRef unencryptedStream As IO.Stream, ByRef creds As SymmetricEncryptionCredential) As Byte()

        Dim outBytes() As Byte
        Using outStream As New IO.MemoryStream()

            EncryptToStream(unencryptedStream, outStream, creds)
            outBytes = outStream.ToArray

        End Using
        Return outBytes
    End Function
    Public Shared Sub EncryptToStream(ByRef unencryptedStream As IO.Stream, ByRef outStream As IO.Stream, ByRef creds As SymmetricEncryptionCredential)

        Using encryptStream As New System.Security.Cryptography.CryptoStream(outStream, creds.GetAESDecryptor, Security.Cryptography.CryptoStreamMode.Write)
            unencryptedStream.CopyTo(encryptStream, 8192)
            encryptStream.FlushFinalBlock()

        End Using



    End Sub
    Public Shared Function Decrypt(ByRef encryptedValue As Byte(), ByRef creds As SymmetricEncryptionCredential) As Byte()
        Dim outBytes() As Byte
        Using outStream As New IO.MemoryStream
            Using decryptionStream As New System.Security.Cryptography.CryptoStream(outStream, creds.GetAESDecryptor, Security.Cryptography.CryptoStreamMode.Write)
                decryptionStream.Write(encryptedValue, 0, encryptedValue.Length)
                decryptionStream.FlushFinalBlock()

                outBytes = outStream.ToArray

                decryptionStream.Close()
            End Using
            outStream.Close()
        End Using
        Return outBytes
    End Function
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="encryptedValue">Encrypted string are stored in Base64 encoding.</param>
    ''' <param name="creds"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Decrypt(ByRef encryptedValue As String, ByRef creds As SymmetricEncryptionCredential) As Byte()

        Dim inBytes() As Byte = Convert.FromBase64String(encryptedValue)
        Return Decrypt(inBytes, creds)

    End Function

    Public Shared Function Decrypt(ByRef envelope As SymmetricEncryptionEnvelope, ByRef passwordList As List(Of SymmetricEncryptionSecretKeyPassword))
        Dim password As SymmetricEncryptionSecretKeyPassword = Nothing

        For Each p As SymmetricEncryptionSecretKeyPassword In passwordList
            If p.Id.Equals(envelope.KeyParameters.PasswordId) Then
                password = p
                Exit For
            End If
        Next

        ' TODO: replace with custom exception
        If password Is Nothing Then
            Throw New ApplicationException("Password not found in password list.")
        End If

        Dim cred As New SymmetricEncryptionCredential(password, envelope.KeyParameters, envelope.IV)

        Return Decrypt(envelope.EncryptedData, cred)


    End Function
    ''' <summary>
    ''' Decrypts the specified value into a UTF8 string.
    ''' </summary>
    ''' <param name="encryptedValue"></param>
    ''' <param name="creds"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DecryptToString(ByRef encryptedValue As Byte(), ByRef creds As SymmetricEncryptionCredential) As String
        Dim returnValue As String = System.Text.Encoding.UTF8.GetString(Decrypt(encryptedValue, creds))
        Return returnValue

    End Function
End Class
