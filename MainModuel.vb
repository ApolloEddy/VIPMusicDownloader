Module MainModuel

	Sub Main(args As String())
		'TestSub()
		'Console.ReadKey()
		Dim ap = New ArgsParser(args).Parse()
		Dim dir As String = My.Computer.FileSystem.CurrentDirectory + "\"

		If args.Length = 0 Then PutsHelp()
		If ap.Help Then PutsHelp() : Exit Sub
		If ap.Kuwo Then
			Console.WriteLine("正在采集信息...")
			Dim kuwo As New KuwoMusic()
			kuwo.DownloadkuwoMusic(ap.url, dir)
		End If

		Console.ResetColor()
		'Console.Write("Press any key to exit...")
		'Console.ReadKey()
	End Sub
	Sub TestSub()
		Dim kw As New KuwoMusic()
		Dim page = kw.GetPageContentDocument("http://www.kuwo.cn/playlist_detail/3174330312")
		kw.GetPlayList(page)
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
$"[指令]		缩写指令	参数			解释
HELP		-h | /h		无			显示帮助信息
KUWO		-k | /k		[url]			下载[url]指定的歌曲 (音源:酷我音乐)
"
)
	End Sub
End Module