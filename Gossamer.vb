Imports System
Imports System.IO
Imports System.Net

''' <summary>
''' 用于处理网络请求
''' </summary>
Public Class Gossamer
    Private request As HttpWebRequest
    Private response As HttpWebResponse
    Public url As String

    ' 构造函数
    Public Sub New(ByVal url As String)
        Me.url = url
    End Sub

    ''' <summary>
    ''' 获取响应流
    ''' </summary>
    ''' <returns><see cref="Stream"/></returns>
    ''' <exception cref="ProtocolViolationException"></exception>
    ''' <exception cref="ObjectDisposedException"></exception>
    Public Function getContent() As MemoryStream
        ' 怀疑出错，稍后调整
        send()

        For i = 0 To 3 - 1

            Try
                response = CType(request.GetResponse(), HttpWebResponse)
                Exit For
            Catch ex As Exception

                If i = 3 Then
                    Throw ex
                End If
            End Try
        Next

        Dim ms As MemoryStream = New MemoryStream With {.Position = 0}
        response.GetResponseStream().CopyTo(ms)
        response.Dispose()
        response = Nothing
        Return ms
    End Function
    ''' <summary>
    ''' 获取响应文本
    ''' </summary>
    ''' <returns><see cref="String"/></returns>
    Public Function getContentDocument() As String
        send()

        For i = 0 To 5

            Try
                response = CType(request.GetResponse(), HttpWebResponse)
                Exit For
            Catch ex As Exception

                If i = 5 Then
                    Throw ex
                End If

                Threading.Thread.Sleep(20)
            End Try
        Next

        Dim streamReader As StreamReader = New StreamReader(response.GetResponseStream())
        Return streamReader.ReadToEnd()
    End Function
    ''' <summary>
    ''' 获取响应的字节数组
    ''' </summary>
    ''' <returns><see cref="Byte."/></returns>
    Public Function getContentBytes() As Byte()
        Dim ms As MemoryStream = CType(getContent(), MemoryStream)
        Dim bytes As Byte() = ms.ToArray()
        ms.Dispose()
        Return bytes
    End Function

    ' 预先设置好 request 的参数
    Private Sub send(ByVal Optional method As String = "GET", ByVal Optional timeout As Integer = 6000)
        request = CType(WebRequest.Create(url), HttpWebRequest)
        request.Method = method
        request.Timeout = timeout
        ' 为了防止错误 Exception:"请求被中止: 未能创建 SSL/TLS 安全通道。"
        ' ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
    End Sub
End Class
