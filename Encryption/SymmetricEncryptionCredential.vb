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

Imports System.Runtime.Serialization

Public Class SymmetricEncryptionCredential
    Private _keyIterations As Integer

    Public Property OvertParameters As SymmetricEncryptionOvertKeyParameters
    Public Property SecretPassword As SymmetricEncryptionSecretKeyPassword
    Public Property IV As Byte()


    Public ReadOnly Property Key() As Byte()
        Get
            Dim deriv As New Security.Cryptography.Rfc2898DeriveBytes(SecretPassword.Password, OvertParameters.Salt, OvertParameters.KeyIterations)
            Return deriv.GetBytes(OvertParameters.KeySize / 8)
        End Get
    End Property

    Private Sub Instantiate(ByRef listOfSecrets As List(Of SymmetricEncryptionSecretKeyPassword), ByRef overtKeyParameters As SymmetricEncryptionOvertKeyParameters, ByRef IV As Byte())
        Dim password As SymmetricEncryptionSecretKeyPassword = Nothing

        For Each p As SymmetricEncryptionSecretKeyPassword In listOfSecrets
            If p.Id.Equals(overtKeyParameters.PasswordId) Then
                password = p
                Exit For
            End If
        Next
        ' TODO: replace with custom exception
        If password Is Nothing Then
            Throw New ApplicationException("Password not found in password list.")
        End If

        Me.OvertParameters = overtKeyParameters
        Me.SecretPassword = password
        Me.IV = IV
    End Sub
    Public Sub New(ByRef listOfSecrets As List(Of SymmetricEncryptionSecretKeyPassword), ByRef overtKeyParameters As SymmetricEncryptionOvertKeyParameters, ByRef IV As Byte())

        Me.Instantiate(listOfSecrets, overtKeyParameters, IV)

    End Sub
    Public Sub New(ByRef secret As SymmetricEncryptionSecretKeyPassword, ByRef overt As SymmetricEncryptionOvertKeyParameters, ByRef IV As Byte())
        Dim list As New List(Of SymmetricEncryptionSecretKeyPassword)
        list.Add(secret)
        Me.Instantiate(list, overt, IV)


    End Sub



    ''' <summary>
    ''' Creates a new set of encryption credentials based on the
    ''' provided key password, while making a iv and salt.
    ''' </summary>
    ''' <remarks>Creates a new set of encryption credentials based on the
    ''' provided key password, while making a iv and salt.</remarks>
    Public Sub New(ByRef secretPassword As SymmetricEncryptionSecretKeyPassword, ByVal keyIterations As Integer, Optional ByVal algorithm As EncryptionAlgorithm = EncryptionAlgorithm.AES256)

        Me.SecretPassword = secretPassword
        Me.OvertParameters = New SymmetricEncryptionOvertKeyParameters(secretPassword.Id, PRNG.GetRandomBytes, keyIterations, algorithm)

        Select Case OvertParameters.Algorithm
            Case EncryptionAlgorithm.AES256
                Me.IV = PRNG.GetRandomBytes(16, 16)
            Case Else
                Throw New NotImplementedException("Encryption Algorithm Not Supported")
        End Select


    End Sub


    ''' <summary>
    ''' Gets an Encryptor.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetEncryptor() As Security.Cryptography.ICryptoTransform
        Dim returnValue As Security.Cryptography.ICryptoTransform

        Select Case Me.OvertParameters.Algorithm
            Case EncryptionAlgorithm.AES256
                Dim aes As New Security.Cryptography.AesManaged

                aes.KeySize = Me.OvertParameters.KeySize
                aes.Key = Me.Key
                aes.IV = Me.IV
                returnValue = aes.CreateEncryptor

            Case Else
                Throw New NotImplementedException("Invalid Algorithm")
        End Select
        Return returnValue

    End Function
    Public Function GetDecryptor() As Security.Cryptography.ICryptoTransform

        Dim returnValue As Security.Cryptography.ICryptoTransform

        Select Case Me.OvertParameters.Algorithm
            Case EncryptionAlgorithm.AES256
                Dim aes As New Security.Cryptography.AesManaged

                aes.KeySize = Me.OvertParameters.KeySize
                aes.Key = Me.Key
                aes.IV = Me.IV
                returnValue = aes.CreateDecryptor

            Case Else
                Throw New NotImplementedException("Invalid Algorithm")
        End Select
        Return returnValue

    End Function

End Class

Public Class SymmetricEncryptionSecretKeyPassword
    Public Property Id As SymmetricEncryptionSecretKeyPasswordId
    Public Property Password As Byte()
    Public Sub New(ByRef passwordName As String, ByVal passwordVersion As Integer, ByRef password() As Byte)
        Me.Id = New SymmetricEncryptionSecretKeyPasswordId(passwordName, passwordVersion)
        Me.Password = password
    End Sub
    Public Function GetBase64Password() As String
        Return Convert.ToBase64String(Password)
    End Function

End Class
<DataContract()> _
Public Class SymmetricEncryptionSecretKeyPasswordId
    Implements IComparable(Of SymmetricEncryptionSecretKeyPasswordId)
    Implements IEquatable(Of SymmetricEncryptionSecretKeyPasswordId)
    <DataMemberAttribute()> _
    Public Property Name As String
    <DataMemberAttribute()> _
    Public Property Version As Integer
    Public Sub New(ByRef passwordName As String, ByVal passwordVersion As Integer)
        Me.Name = passwordName
        Me.Version = passwordVersion
    End Sub
    Public Overrides Function ToString() As String
        Return Me.Name & "," & Me.Version.ToString


    End Function
    Public Function CompareTo(other As SymmetricEncryptionSecretKeyPasswordId) As Integer Implements System.IComparable(Of SymmetricEncryptionSecretKeyPasswordId).CompareTo

        Dim nameCompare As Integer = Me.Name.CompareTo(other.Name)
        If nameCompare = 0 Then
            Return Me.Version.CompareTo(other.Version)
        Else
            Return nameCompare
        End If

    End Function

    Public Shadows Function Equals(other As SymmetricEncryptionSecretKeyPasswordId) As Boolean Implements System.IEquatable(Of SymmetricEncryptionSecretKeyPasswordId).Equals
        If Me.CompareTo(other) = 0 Then
            Return True
        Else
            Return False
        End If
    End Function

End Class
<System.Runtime.Serialization.DataContract()> _
Public Class SymmetricEncryptionOvertKeyParameters

    <DataMember()> _
    Public Property PasswordId As SymmetricEncryptionSecretKeyPasswordId
    <DataMember()> _
    Public Property Salt As Byte()
    <DataMember()> _
    Public Property KeyIterations As Integer
    <DataMember()> _
    Public Property Algorithm As EncryptionAlgorithm

    Public ReadOnly Property KeySize As Integer
        Get
            Select Case Me.Algorithm
                Case EncryptionAlgorithm.AES256
                    Return 256
                Case Else
                    Throw New NotImplementedException("Encryption algorithm not supported.")
            End Select
        End Get
    End Property
    Public Sub New(ByRef passwordId As SymmetricEncryptionSecretKeyPasswordId, ByRef salt() As Byte, ByRef keyIterations As Integer, Optional ByVal algorithm As EncryptionAlgorithm = EncryptionAlgorithm.AES256)
        Me.PasswordId = passwordId
        Me.Salt = salt
        Me.KeyIterations = keyIterations
        Me.Algorithm = algorithm
    End Sub
End Class
<DataContract()> _
Public Enum EncryptionAlgorithm
    <EnumMember(Value:="AES256")> AES256

End Enum

