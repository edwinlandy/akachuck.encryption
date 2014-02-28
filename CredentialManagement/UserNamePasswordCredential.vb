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

<System.Runtime.Serialization.DataContractAttribute()>
Public Class UsernamePasswordCredential
    Inherits akaChuck.CredentialManagement.Credential
    Delegate Function CredentialChecker(ByRef credential As UsernamePasswordCredential) As CredentialCheckerResult

    Private _credentialName As String
    Private _UserName As String
    Private _Password As String

    Public ReadOnly Property UserName As String
        Get
            Return _UserName
        End Get
    End Property
    Public ReadOnly Property Password As String
        Get
            Return _Password
        End Get
    End Property

    Public Sub New(ByRef userName As String, ByRef password As String, Optional ByVal credentialName As String = "UsernamePasswordCredential", Optional ByVal version As Integer = 1)
        MyBase.new(credentialName, version)

        _UserName = userName
        _Password = password

    End Sub
    Public Function IsCurrent(ByRef currentnessChecker As CredentialChecker) As Boolean
        Return currentnessChecker.Invoke(Me).Result
    End Function
    Public Overrides Function IsProper() As Boolean
        Return PropernessChecker(Me).Result

    End Function
  
    Public Shared Function PropernessChecker(ByRef credential As UsernamePasswordCredential) As CredentialCheckerResult
        Dim returnValue As New CredentialCheckerResult
        Dim result As Boolean = True
        Dim reason As New Text.StringBuilder


        'Items must contain values
        If String.IsNullOrEmpty(credential.UserName) Or String.IsNullOrEmpty(credential.Password) Then
            result = False
            reason.AppendLine("Both 'UserName' and 'Password' items must not be null or empty strings")
        End If

        returnValue.Result = result
        returnValue.ResultReason = reason.ToString

        Return returnValue
    End Function



End Class
