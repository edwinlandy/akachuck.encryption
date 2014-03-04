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
Imports System.Configuration.ConfigurationManager
<TestClass()>
Public Class Test_FileSystemCredentialCache
    Private CachePath As String = AppSettings("credentialFileCacheDirectory")
    <TestMethod()>
    Public Sub Test_FileSystemCredentialCache()
        ' Create Cache.
        Dim cache As New FileSystemCredentialCache(Of UsernamePasswordCredential)(CachePath)
        ' Create a credential.
        Dim cred1 As New UsernamePasswordCredential("Test", "TestPassword1", "TestCredential", 1)

        '------------------------------
        '- Test Read/Write ------------
        '------------------------------
        Dim wr As New CredentialCacheReadWriteTester(Of UsernamePasswordCredential)(cache, cred1)
        wr.TestReadWrite()

        '------------------------------
        '- Test exceptions ------------
        '------------------------------
        Dim invalidCache As New FileSystemCredentialCache(Of UsernamePasswordCredential)(IO.Path.Combine(CachePath, "Invalid"))
        Dim ex As New CredentialCacheExceptionTester(Of UsernamePasswordCredential)(cred1.Name, "InvalidName", 2, 453, cache, invalidCache)
        ex.TestNotCachedException()

        '-----------------------------------
        '- Test deserialization ------------
        '-----------------------------------
        ' Cache cred1 to file system.
        cache.CacheCredential(cred1)

        ' Read the credential from cache.
        Dim cred2 As UsernamePasswordCredential = cache.ReadFromCache(cred1.Name, cred1.Version).Credential

        Assert.AreEqual(cred1.Name, cred2.Name)
        Assert.AreEqual(cred1.Version, cred2.Version)
        Assert.AreEqual(cred1.UserName, cred2.UserName)
        Assert.AreEqual(cred1.Password, cred2.Password)

        ' Cleanup.
        cache.DeleteCachedCredentials()

    End Sub


End Class
