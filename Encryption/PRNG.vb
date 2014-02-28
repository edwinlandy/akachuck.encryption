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
        Dim randomBytePool(length * 2 - 1) As Byte
        Dim text(length - 1) As Char
        Dim textCounter As Integer = 0
        Dim poolCounter As Integer = 0

        While poolCounter < randomBytePool.Length And textCounter < text.Length
            randomBytePool = GetRandomBytes(randomBytePool.Length, randomBytePool.Length)

            While poolCounter < randomBytePool.Length And textCounter < text.Length
                ' We are using a search space length of 94. We must use
                ' a multiple of 94 to prevent unnecessary bias.
                ' Discard a byte larger than 187.
                If randomBytePool(poolCounter) < 188 Then

                    ' We only want to return printable chars, which are
                    ' represented by bytes 33 to 126.  We don't include space.
                    text(textCounter) = Chr((randomBytePool(poolCounter) Mod 94) + 33)
                    textCounter += 1

                End If

                poolCounter += 1
            End While

        End While

        Return New String(text)

    End Function
    Public Shared Function GetRandomPrintablePassword() As String
        Return GetRandomPrintablePassword(16)
    End Function
End Class