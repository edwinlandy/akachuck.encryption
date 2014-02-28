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

Imports akaChuck
Imports akaChuck.CredentialManagement

Public Class TestUserNamePasswordCredential

    <CLSCompliant(False)>
    Shared Function CurrentnessChecker(ByRef credential As UsernamePasswordCredential) As CredentialCheckerResult
        Dim returnValue As New CredentialCheckerResult

        If credential.UserName = "edwin" And credential.Password = "abc123" Then
            returnValue.Result = True
            returnValue.ResultReason = "Credential is current."

        Else
            returnValue.Result = False
            returnValue.ResultReason = "Credential is not current."
        End If
        Return returnValue

    End Function
End Class
