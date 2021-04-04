Imports System.Net
Imports System.IO

Public Class WebProtocol
	Protected Sub New()

	End Sub
	Public Sub New(url As String, Optional ComputerUA As Boolean = True)
		Me.Url = url
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
		request = WebRequest.Create(url)
		Me.ComputerUA = ComputerUA
	End Sub
	Public ComputerUA As Boolean = True
	Protected request As HttpWebRequest
	Protected response As HttpWebResponse
	Protected randUA As New RandomUserAgent()

	Public Property Url As String
	Public Property UserAgent As String
		Get
			Return request.UserAgent
		End Get
		Set(value As String)
			request.UserAgent = value
		End Set
	End Property
	Public Property Referer As String
		Get
			Return request.Referer
		End Get
		Set(value As String)
			request.Referer = value
		End Set
	End Property
	Public Property Accept As String
		Get
			Return request.Accept
		End Get
		Set(value As String)
			request.Accept = value
		End Set
	End Property
	Public Property Method As String
		Get
			Return request.Method
		End Get
		Set(value As String)
			request.Method = value
		End Set
	End Property
	Public Property Headers As WebHeaderCollection
		Get
			Return request.Headers
		End Get
		Set(value As WebHeaderCollection)
			request.Headers = value
		End Set
	End Property
	Public Property Timeout As Integer
		Get
			Return request.Timeout
		End Get
		Set(value As Integer)
			request.Timeout = value
		End Set
	End Property
	Public Property AutoUA As Boolean = True
	Public Property RequestInterval As Integer = 20


	Private Overloads Function GetContentStream() As Stream
		For i As Integer = 1 To 6
			If AutoUA Then UserAgent = If(ComputerUA, randUA.NextComputerUA, randUA.NextHandphoneUA)
			Try
				response = request.GetResponse()
				Threading.Thread.Sleep(RequestInterval)
			Catch ex As Exception
				If Not i = 6 Then Continue For
				Throw ex
				Exit For
			End Try
		Next
		Return response.GetResponseStream()
	End Function
	Public Overloads Function GetContent() As Stream
		Dim ms As New MemoryStream() With {.Position = 0}
		GetContentStream().CopyTo(ms)
		GetContent = ms
		ms.Dispose()
	End Function
	Public Overloads Function GetContentDocument() As String
		Return New StreamReader(GetContentStream).ReadToEnd()
	End Function
	Public Overloads Function GetContentBytes() As Byte()
		Dim ms As MemoryStream = GetContent()
		GetContentBytes = ms.ToArray()
		ms.Dispose()
	End Function
	Public Overloads Shared Function EscapeDataString(value As String) As String
		Return Uri.EscapeDataString(value)
	End Function
	Public Overloads Shared Function UnescapeDataString(value As String) As String
		Return Uri.UnescapeDataString(value)
	End Function
End Class