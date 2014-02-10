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


    Public Sub New(ByRef secret As SymmetricEncryptionSecretKeyPassword, ByRef overt As SymmetricEncryptionOvertKeyParameters, ByRef IV As Byte())
        Me.SecretPassword = secret
        Me.OvertParameters = overt
        Me.IV = IV

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
    ''' Gets an AES Encryptor with key derived from RFC2898.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetAESEncryptor() As Security.Cryptography.ICryptoTransform

        Dim aes As New Security.Cryptography.AesManaged

        aes.KeySize = Me.OvertParameters.KeySize
        aes.Key = Me.Key
        aes.IV = Me.IV
        Return aes.CreateEncryptor()

    End Function
    Public Function GetAESDecryptor() As Security.Cryptography.ICryptoTransform

        Dim aes As New Security.Cryptography.AesManaged
        aes.KeySize = Me.OvertParameters.KeySize
        aes.Key = Me.Key
        aes.IV = Me.IV
        Return aes.CreateDecryptor

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
Public Class SymmetricEncryptionSecretKeyPasswordId
    Implements IComparable(Of SymmetricEncryptionSecretKeyPasswordId)
    Implements IEquatable(Of SymmetricEncryptionSecretKeyPasswordId)

    Public Property Name As String
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
Public Class SymmetricEncryptionOvertKeyParameters
    Public Property PasswordId As SymmetricEncryptionSecretKeyPasswordId
    Public Property Salt As Byte()
    Public Property KeyIterations As Integer
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
Public Enum EncryptionAlgorithm
    AES256

End Enum

