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
Imports akaChuck
Imports akaChuck.CredentialManagement

<TestClass()>
Public Class Test_FileSystemCredentialCache
    Const CachePath As String = "C:\CredentialCacheTest"
    <TestMethod()>
    Public Sub Test_FileSystemCredentialCache()


        ' Create three different versions of the same credential
        Dim cred1 As New UsernamePasswordCredential("Test", "TestPassword1", "TestCredential", 1)
        Dim cred2 As New UsernamePasswordCredential("Test", "TestPassword2", "TestCredential", 2)
        Dim cred3 As New UsernamePasswordCredential("Test", "TestPassword3", "TestCredential", 3)

        ' Cache them to the file system
        Dim cache As New FileSystemCredentialCache(Of UsernamePasswordCredential)(CachePath)

        cache.CacheCredential(cred2)
        cache.CacheCredential(cred1)
        cache.CacheCredential(cred3)

        ' ReadFromCache with only the 'name' parameter should return the last version.
        Dim cred4 As UsernamePasswordCredential = cache.ReadFromCache("TestCredential").Credential

        Assert.AreEqual(cred3, cred4)






    End Sub

End Class
