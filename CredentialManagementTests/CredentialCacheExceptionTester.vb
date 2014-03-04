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

Imports akaChuck.CredentialManagement
''' <summary>
''' Use for testing exceptions on classes inheriting CredentialCache.
''' </summary>
''' <typeparam name="CRED"></typeparam>
''' <remarks></remarks>
<CLSCompliant(False)> _
Public Class CredentialCacheExceptionTester(Of CRED As Credential)
    Public Property InvalidCredentialCache As CredentialCache(Of CRED)
    Public Property InvalidName As String
    Public Property InvalidVersion As Integer
    Public Property ValidCredentialCache As CredentialCache(Of CRED)
    Public Property ValidName As String
    Public Property ValidVersion As String

    Public Sub New(ByRef validName As String, ByRef invalidName As String, ByVal validVersion As Integer, _
                   ByVal invalidVersion As Integer, ByRef validCache As CredentialCache(Of CRED), ByRef invalidCache As CredentialCache(Of CRED))

        Me.InvalidCredentialCache = invalidCache
        Me.InvalidName = invalidName
        Me.InvalidVersion = invalidVersion

        Me.ValidCredentialCache = validCache
        Me.ValidName = validName
        Me.ValidVersion = validVersion

    End Sub
    ''' <summary>
    ''' CredentialNotCached Exception should be thrown when a credential is not cached.
    ''' This method tests that this is so.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub TestNotCachedException()
        '-----------------------------------------------------------
        '- Try to read latest credential with bad credential name.--
        '-----------------------------------------------------------
        Try
            ValidCredentialCache.ReadFromCache(InvalidName)
            Assert.Fail("Expected Exception. Did not get one.")
        Catch notCached As CredentialNotCachedException
            ' Good Exception
        Catch ex As Exception
            Assert.Fail("Method did not throw expected exception")
        End Try

        '-----------------------------------------------------------
        '- Try to read all credentials with bad credential name.----
        '-----------------------------------------------------------
        Try
            ValidCredentialCache.ReadAllFromCache(InvalidName)
            Assert.Fail("Expected Exception. Did not get one.")
        Catch notCached As CredentialNotCachedException
            ' Good Exception
        Catch ex As Exception
            Assert.Fail("Method did not throw expected exception")
        End Try

        '-----------------------------------------------------------
        '- Try to read credential with invalid version.-------------
        '-----------------------------------------------------------
        Try
            ValidCredentialCache.ReadFromCache(ValidName, InvalidVersion)
            Assert.Fail("Expected Exception. Did not get one.")
        Catch notCached As CredentialNotCachedException
            ' Good Exception
        Catch ex As Exception
            Assert.Fail("Method did not throw expected exception")
        End Try

        '-----------------------------------------------------------------
        '- Try to read all credentials from a cache that contains none.---
        '-----------------------------------------------------------------
        Try
            InvalidCredentialCache.ReadAllFromCache()
            Assert.Fail("Expected Exception. Did not get one.")
        Catch notCached As CredentialNotCachedException
            ' Good Exception
        Catch ex As Exception
            Assert.Fail("Method did not throw expected exception")
        End Try

    End Sub
End Class
