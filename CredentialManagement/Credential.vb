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

Imports System.Runtime.Serialization

<System.Runtime.Serialization.DataContractAttribute()>
Public MustInherit Class Credential
    Implements IComparable(Of Credential)

    <System.Runtime.Serialization.DataMember(Name:="Name")>
    Protected _Name As String
    ''' <summary>
    ''' Unique name to be used by Credential Caching and Management
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property
    ''' <summary>
    ''' Credential version for purposes of credential rotation.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <System.Runtime.Serialization.DataMember()>
    Public Property Version As Integer

    Protected Sub New(ByRef name As String, Optional ByVal version As Integer = 1)
        Me._Name = name
        Me.Version = version

    End Sub

    Public MustOverride Function IsProper() As Boolean

    Public Function CompareTo(other As Credential) As Integer Implements System.IComparable(Of Credential).CompareTo

        Return Me.Version - other.Version

    End Function
End Class


Public Class CredentialCheckerResult
    Public Property Result As Boolean
    Public Property ResultReason As String

End Class
Public Class CredentialCheckerException
    Inherits ApplicationException
    Public Property Result As CredentialCheckerResult
    Sub New(ByVal message As String, ByVal result As CredentialCheckerResult)
        MyBase.New(message)
        Me.Result = result
    End Sub
    Sub New(ByVal message As String, ByVal innerException As Exception)
        MyBase.New(message, innerException)
    End Sub
End Class
