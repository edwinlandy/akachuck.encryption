Public Class PRNG
    Public Shared Function GetRandomBytes(minLength As Integer, maxlength As Integer) As Byte()
        ' ***   1.  Get how long the password will be
        Dim rand As New Random
        Dim passLength As Integer = rand.Next(minLength, maxlength)

        ' ***   2.  Create an array of Bytes to hold the 
        '           random numbers used to make the string's chars
        Dim passBytes(passLength - 1) As Byte

        ' ***   3.  Fill the array with random bytes.
        Using rng As New Security.Cryptography.RNGCryptoServiceProvider
            rng.GetBytes(passBytes)
        End Using

        Return passBytes

    End Function
    Public Shared Function GetRandomBytes() As Byte()

        Return GetRandomBytes(16, 32)

    End Function
    ''' <summary>
    ''' Gets a Cryptographicly Random Base 64 String
    ''' </summary>
    ''' <param name="minLength">The minimum number of Bytes to return.</param>
    ''' <param name="maxlength">The maximum number of Bytes to return</param>
    ''' <returns>A base 64 string that represents random Bytes.</returns>
    ''' <remarks>Note that the minLength and maxLength are for the number of full bytes, 
    ''' and that the base 64 string will be four-thirds longer than the byte count.</remarks>
    Public Shared Function GetRandomBase64Password(minLength As Integer, maxlength As Integer) As String
        Return Convert.ToBase64String(GetRandomBytes(minLength, maxlength))
    End Function
    Public Shared Function GetRandomBase64Password() As String
        Return GetRandomBase64Password(16, 32)
    End Function

    Public Shared Function GetRandomPrintablePassword(ByVal length As Integer) As String
        Dim buffer(length * 2 - 1) As Byte
        Dim text(length - 1) As Char
        Dim textCounter As Integer = 0
        Dim bufferCounter As Integer = 0

        While bufferCounter < buffer.Length And textCounter < text.Length
            buffer = GetRandomBytes(buffer.Length, buffer.Length)

            While bufferCounter < buffer.Length And textCounter < text.Length
                ' We are using a 94 byte search space. We must use
                ' a multiple of 94 to prevent unneeded bias.
                ' Discard a byte larger than 187.
                If buffer(bufferCounter) < 188 Then

                    ' We only want to return printable chars, which are
                    ' represented by bytes 33 to 126.  We don't include space.
                    text(textCounter) = Chr((buffer(bufferCounter) Mod 94) + 33)
                    textCounter += 1

                End If

                bufferCounter += 1
            End While

        End While

        Return New String(text)

    End Function
    Public Shared Function GetRandomPrintablePassword() As String
        Return GetRandomPrintablePassword(16)
    End Function
End Class