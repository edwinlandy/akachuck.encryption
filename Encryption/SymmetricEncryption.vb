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

Public Class SymmetricEncryptorStream
    Inherits Security.Cryptography.CryptoStream
    Private Property unencryptedStream As IO.Stream

    Public Sub New(ByRef unencryptedStream As IO.Stream, ByRef cred As SymmetricEncryptionCredential)
        MyBase.New(unencryptedStream, cred.GetEncryptor, Security.Cryptography.CryptoStreamMode.Read)
        Me.unencryptedStream = unencryptedStream
    End Sub

    Public Sub Encrypt(ByRef outStream As IO.Stream, Optional ByVal bufferSize As Integer = 8192)
        Me.CopyTo(outStream, bufferSize)
    End Sub
    Public Function EncryptToBytes(Optional ByVal bufferSize As Integer = 8192) As Byte()
        Dim returnValue() As Byte
        Using stm As New IO.MemoryStream()
            Me.CopyTo(stm, bufferSize)
            returnValue = stm.ToArray()
        End Using
        Return returnValue
    End Function
End Class

Public Class SymmetricDecryptorStream
    Inherits Security.Cryptography.CryptoStream

    Public Sub New(ByRef encryptedStream As IO.Stream, ByRef cred As SymmetricEncryptionCredential)
        MyBase.New(encryptedStream, cred.GetDecryptor, Security.Cryptography.CryptoStreamMode.Read)
    End Sub
    Public Function DecryptToBytes() As Byte()
        Dim returnValue() As Byte
        Using stm As New IO.MemoryStream()
            Me.CopyTo(stm)
            returnValue = stm.ToArray()
        End Using
        Return returnValue
    End Function

End Class

Public Class SymmetricEncryption
    Public Shared Sub EncryptToStream(ByRef unencryptedStream As IO.Stream, ByRef outStream As IO.Stream, ByRef cred As SymmetricEncryptionCredential)
        Using encryptorStream As New SymmetricEncryptorStream(unencryptedStream, cred)
            encryptorStream.CopyTo(outStream)
        End Using
    End Sub
    Public Shared Function GetEncryptedBytes(ByRef unencryptedStream As IO.Stream, ByRef cred As SymmetricEncryptionCredential) As Byte()
        Using encryptor As New SymmetricEncryptorStream(unencryptedStream, cred)
            Return encryptor.EncryptToBytes
        End Using
    End Function
    Public Shared Function GetEncryptedBytes(ByRef unencryptedValue As String, ByRef creds As SymmetricEncryptionCredential) As Byte()
        Dim inBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(unencryptedValue)
        Dim outbytes() As Byte = GetEncryptedBytes(inBytes, creds)

        Return outbytes
    End Function

    Public Shared Function GetEncryptedBytes(ByRef unencryptedValue As Byte(), ByRef cred As SymmetricEncryptionCredential) As Byte()

        Using inStream As New IO.MemoryStream(unencryptedValue)
            Return GetEncryptedBytes(inStream, cred)
        End Using

    End Function
    Public Shared Function GetEncryptionEnvelope(ByRef unencryptedValue As Byte(), ByRef cred As SymmetricEncryptionCredential) As SymmetricEncryptionEnvelope

        Dim returnValue As New SymmetricEncryptionEnvelope() With
            {
                .EncryptedData = GetEncryptedBytes(unencryptedValue, cred),
                .IV = cred.IV,
                .KeyParameters = New SymmetricEncryptionOvertKeyParameters(cred.SecretPassword.Id, cred.OvertParameters.Salt, _
                                                                           cred.OvertParameters.KeyIterations, cred.OvertParameters.Algorithm)
            }
        Return returnValue
    End Function
 
    Public Shared Function GetDecryptedBytes(ByRef encryptedValue As Byte(), ByRef cred As SymmetricEncryptionCredential) As Byte()
        Dim outBytes() As Byte
        Using encryptedStream As New IO.MemoryStream(encryptedValue)
            Using decryptor As New SymmetricDecryptorStream(encryptedStream, cred)
                outBytes = decryptor.DecryptToBytes
            End Using
        End Using
        Return outBytes
    End Function
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="encryptedValue">Encrypted string are stored in Base64 encoding.</param>
    ''' <param name="cred"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetDecryptedBytes(ByRef encryptedValue As String, ByRef cred As SymmetricEncryptionCredential) As Byte()

        Dim inBytes() As Byte = Convert.FromBase64String(encryptedValue)
        Return GetDecryptedBytes(inBytes, cred)

    End Function

    Public Shared Function GetDecryptedBytes(ByRef envelope As SymmetricEncryptionEnvelope, ByRef passwordList As List(Of SymmetricEncryptionSecretKeyPassword)) As Byte()

        Dim cred As New SymmetricEncryptionCredential(passwordList, envelope.KeyParameters, envelope.IV)

        Return GetDecryptedBytes(envelope.EncryptedData, cred)
    End Function
    ''' <summary>
    ''' Decrypts the specified value into a UTF8 string.
    ''' </summary>
    ''' <param name="encryptedValue"></param>
    ''' <param name="creds"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DecryptToString(ByRef encryptedValue As Byte(), ByRef creds As SymmetricEncryptionCredential) As String
        Dim returnValue As String = System.Text.Encoding.UTF8.GetString(GetDecryptedBytes(encryptedValue, creds))
        Return returnValue

    End Function
End Class


