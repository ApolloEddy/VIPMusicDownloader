Module MainModuel

	Sub Main(args As String())
		PutsProgramInfo()
		'TestSub()
		'Console.ReadKey()
		Dim ap = New ArgsParser(args).Parse()
		Dim dir As String = My.Computer.FileSystem.CurrentDirectory + "\"

		If args.Length = 0 Then PutsHelp()
		If ap.Help Then PutsHelp() : Exit Sub
		If ap.Kuwo Then
			Console.WriteLine("正在采集信息...")
			Dim kuwo As New KuwoMusic()
			kuwo.DownloadkuwoMusic(ap.url, dir, ap.Lyric)
		End If

		Console.ResetColor()
		'Console.Write("Press any key to exit...")
		'Console.ReadKey()
	End Sub
	Sub TestSub()
		Dim kw As New KuwoMusic()
		kw.GetSelfMadePlayList("2853625919")
	End Sub
	Sub PutsError(message As String)
		Console.ResetColor()
		Console.ForegroundColor = ConsoleColor.Red
		Console.WriteLine($"[Error] {message}")
		Console.ResetColor()
	End Sub
	Sub PutsHelp()
		Console.ResetColor()
		Console.WriteLine(
$"[以下为帮助信息]
  指令/参数		缩写指令	    解释
    HELP		-h | /h		显示帮助信息
    LYRIC		-l | /l		指定是否下载歌词
    [url]		   无		指定需要下载的url (现仅支持酷我音乐)
"
)
	End Sub
	Sub PutsProgramInfo()
		Dim info = My.Application.Info
		Dim InfoString As String =
$"欢迎使用 [{info.AssemblyName} v{info.Version}]，{info.Description}
By [{info.CompanyName}]		 {info.Copyright} - {Year(Now)}
作者[洛尘] QQ：2573993130		QQ交流群：1045839144	
GitHub项目地址：https://github.com/ApolloEddy/VIPMusicDownloader/
"
		Console.WriteLine(InfoString)
	End Sub
End Module