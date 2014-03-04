Public Class AWSClientConfig
    Public Property Credential As IAWSCredential
    Public Property Region As Amazon.RegionEndpoint

    Public Sub New(ByRef cred As IAWSCredential, region As Amazon.RegionEndpoint)
        Me.Credential = cred
        Me.Region = region
    End Sub
End Class
