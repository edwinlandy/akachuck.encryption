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
Public Class CredentialCacheReadWriteTester(Of CRED As Credential)
    Public Property Cache As CredentialCache(Of CRED)
    Public Property Credential As CRED
    Public Sub New(ByRef credCache As CredentialCache(Of CRED), ByVal cred As CRED)
        Me.Cache = credCache
        Me.Credential = cred

    End Sub

    Public Sub TestReadWrite()
        ' Make sure we have a proper credential
        Assert.IsTrue(Me.Credential.IsProper)

        '------------------------------------------------------------
        '- 1. Cache credential with three different version numbers -
        '------------------------------------------------------------
        Dim originalName As String = Me.Credential.Name
        Me.Credential.Version = 2
        Cache.CacheCredential(Me.Credential)
        Me.Credential.Version = 3
        Cache.CacheCredential(Me.Credential)
        Me.Credential.Version = 1
        Cache.CacheCredential(Me.Credential)

        '-----------------------------------------------------------------
        '- 2. Cache one credential with different name from others -------
        '-----------------------------------------------------------------

        Dim differentName As String = akaChuck.Encryption.PRNG.GetRandomBase64Password
        Me.Credential.Name = differentName
        Cache.CacheCredential(Me.Credential)
        '-----------------------------------------------------------------
        '- 3. See that cache can read latest credential version    -------
        '-----------------------------------------------------------------

        Dim cred3 As CachedCredential(Of CRED) = Cache.ReadFromCache(originalName)
        ' We just cached the credential. The cached credential should reflect this.
        Assert.IsTrue(DateTime.Now.Subtract(cred3.TimeCached) < New TimeSpan(0, 0, 10))
        ' We should have gotten version 3 of the credential.
        Assert.IsTrue(cred3.Credential.Version = 3)
        ' Credential should still be proper
        Assert.IsTrue(cred3.Credential.IsProper)

        '-----------------------------------------------------------------
        '- 4. See that we can get all credentials with given name  -------
        '-----------------------------------------------------------------
        Dim credList1 As List(Of CachedCredential(Of CRED)) = Cache.ReadAllFromCache(originalName)
        ' We got three items.
        Assert.IsTrue(CInt(3) = CInt(credList1.Count))
        ' All are differnt.
        CollectionAssert.AllItemsAreUnique(credList1)

        '--------------------------------------------------------------------
        '- 5. See that we can get all credentials of the credential type.----
        '--------------------------------------------------------------------
        Dim credList2 As List(Of CachedCredential(Of CRED)) = Cache.ReadAllFromCache()
        Assert.IsTrue(credList2.Count = 4)

        '----------------------------------------------------------------------------
        '- 6. See that we can get the one differently named credential by itself.----
        '----------------------------------------------------------------------------
        Dim credList3 As List(Of CachedCredential(Of CRED)) = Cache.ReadAllFromCache(differentName)
        Assert.IsTrue(credList3.Count = 1)
        Assert.IsTrue(credList3(0).Credential.Name = differentName)


        ' Cleanup
        Cache.DeleteCachedCredentials()


    End Sub
End Class
