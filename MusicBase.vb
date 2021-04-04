Imports System.IO

Public MustInherit Class MusicBase
	Protected Friend Function GetPageContentDocument(url As String) As String
		Dim page As String
		Dim gossamer As New WebProtocol(url)
		page = gossamer.getContentDocument()
		Return page
	End Function
	Protected Friend Function GetContent(url As String) As Byte()
		Dim bytes As Byte()
		Dim gossamer As New WebProtocol(url)
		bytes = gossamer.GetContentBytes()
		Return bytes
	End Function
	Protected Friend Sub SaveAudio(bytes As Byte(), path As String)
		File.Create(path).Write(bytes, 0, bytes.Length - 1)
	End Sub
	Protected Friend Sub DowmloadMusic(url As String, path As String)
		SaveAudio(GetContent(url), path)
	End Sub
	Sub PutsError(message As String)
		Console.ResetColor()
		Console.ForegroundColor = ConsoleColor.Red
		Console.WriteLine($"[Error] {message}")
		Console.ResetColor()
	End Sub
End Class
