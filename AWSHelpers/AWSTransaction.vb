Public MustInherit Class AWSTransaction(Of Req As {Amazon.Runtime.AmazonWebServiceRequest}, Res As {Amazon.Runtime.AmazonWebServiceResponse}, C As Amazon.Runtime.AmazonWebServiceClient)
    Public Property Client As C
    Public Property Request As Req
    Public Property Response As Res
    Protected Sub New(ByRef client As C)
        Me.Client = client
    End Sub
    Public MustOverride Sub ExecuteRequest()
End Class
