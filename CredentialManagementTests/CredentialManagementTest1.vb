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
Imports akaChuck.CredentialManagement

<TestClass()>
Public Class CredentialManagementTest1

    Private testContextInstance As TestContext

    '''<summary>
    '''Gets or sets the test context which provides
    '''information about and functionality for the current test run.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = value
        End Set
    End Property

#Region "Additional test attributes"
    '
    ' You can use the following additional attributes as you write your tests:
    '
    ' Use ClassInitialize to run code before running the first test in the class
    ' <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    ' End Sub
    '
    ' Use ClassCleanup to run code after all tests in a class have run
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    '
    ' Use TestInitialize to run code before running each test
    ' <TestInitialize()> Public Sub MyTestInitialize()
    ' End Sub
    '
    ' Use TestCleanup to run code after each test has run
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region

    <TestMethod()>
    Public Sub UserNamePasswordCredentialTest()
        '----------------------------------------------
        '-----Create proper and current credential-----
        '----------------------------------------------
        Dim userName1 As String = "edwin"
        Dim password1 As String = "abc123"
        Dim cred1 As New UsernamePasswordCredential(userName1, password1, "Cred1", 2)

        ' cred1 should be proper and current.
        Assert.IsTrue(cred1.IsProper)
        Assert.IsTrue(cred1.IsCurrent(AddressOf TestUserNamePasswordCredential.CurrentnessChecker))

        ' cred1's other properties should be what's expected
        Assert.AreEqual(CInt(2), cred1.Version)
        Assert.AreEqual("Cred1", cred1.Name)

        '------------------------------------
        '-----Create improper credential-----
        '------------------------------------
        Dim userName2 As String = "test2"
        Dim password2 As String = "" '<---- Empty string should fail
        Dim cred2 As New UsernamePasswordCredential(userName2, password2, "Cred2", 3)

        ' cred2 is not proper
        Assert.IsFalse(cred2.IsProper())

        '---------------------------------------
        '-----Create not current credential-----
        '---------------------------------------
        Dim userName3 As String = "edwin"
        Dim password3 As String = "notcurrentpassword"
        Dim cred3 As New UsernamePasswordCredential(userName3, password3, "Cred3", 4)

        ' cred3 is not current.
        Assert.IsFalse(cred3.IsCurrent(AddressOf TestUserNamePasswordCredential.CurrentnessChecker))



    End Sub

End Class
