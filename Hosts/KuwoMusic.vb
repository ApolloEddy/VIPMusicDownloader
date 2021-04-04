Public Class KuwoMusic : Inherits MusicBase
	Public Const musicURL As String = "http://www.kuwo.cn/url?format={format}&rid={rid}&response=url&type=convert_url3&br={br}&from=web&httpsStatus=1"
	Public Const jsonURL As String = "http://m.kuwo.cn/newh5/singles/songinfoandlrc?musicId={musicId}&httpsStatus=1"
	Public Const HostURL As String = "http://www.kuwo.cn"
	Public Const HandphoneJsonUrl As String = "http://m.kuwo.cn/newh5app/api/mobile/v1/music/playlist/{id}?pn={pn}&rn={rn}"
	Protected UnDownloaded As Integer = 0
	Protected rand As New Random(CLng($"{Hour(Now)}{Second(Now)}{Date.Now.Millisecond}"))

	Public Sub New()
		Randomize()
	End Sub
	Public Sub DownloadkuwoMusic(url As String, path As String, Optional lyric As Boolean = False)
		If url.Contains("playlist_detail") Then
			Console.WriteLine("[提示] 这是一个歌单链接，即将下载整部歌单...")
			Dim page As String = GetPageContentDocument(url)
			Dim plist As List(Of String) = GetPlayList(page)
			path += GetPlistName(page) + "\"
			IO.Directory.CreateDirectory(path)
			DownloadList(plist, path, lyric)
			Console.WriteLine($"共计捕获 [{plist.Count}] 首歌， 成功下载了 [{plist.Count - UnDownloaded}] 首歌！")
		ElseIf url.Contains("play_detail") Then
			Console.WriteLine("[提示] 这是一首单曲，即将下载...")
			DownloadSingleSong(url, path, lyric)
		ElseIf url.Contains("m.kuwo.cn/h5app/") And url.Contains("/playlist/") Then
			Console.WriteLine("[提示] 这是一个临时歌单链接(自建歌单)，即将下载整部歌单...")
			Dim listid As String = TextParser.RegParseOne(url, "[0-9]{3,}")
			Dim plist As List(Of String) = GetSelfMadePlayList(listid)
			path += GetSelfMadePlayListName(url) + "\"
			IO.Directory.CreateDirectory(path)
			DownloadList(plist, path, lyric)
			Console.WriteLine($"共计捕获 [{plist.Count}] 首歌， 成功下载了 [{plist.Count - UnDownloaded}] 首歌！")
		Else
			PutsError($"错误或暂时无法识别的url链接，请检查后重试！") : Exit Sub
		End If
	End Sub
	Public Sub DownloadList(playList As List(Of String), path As String, Optional lyric As Boolean = False)
		For Each song As String In playList
			DownloadSingleSong(song, path, lyric)
			Threading.Thread.Sleep(rand.Next(5, 20)) ' 防御反爬虫措施
		Next
	End Sub
	Public Sub DownloadSingleSong(url As String, path As String, Optional lyric As Boolean = False)
		Dim mid As String
		Dim json As String
		Dim musicInfo As New MusicInfo
		For i As Integer = 1 To 5
			Try
				mid = GetMusicID(url)
				json = GetMusicInfoJson(mid)
				musicInfo = GetMusicInfo(json)
				Exit For
			Catch ex As Exception
				PutsError(ex.Message)
				Console.WriteLine($"正在尝试第 [{i}] 重新请求该歌曲")
				If i = 5 Then UnDownloaded += 1 : PutsError("下载失败！即将下载下一首歌曲...") : Exit Sub Else Continue For
			End Try
		Next
		PutsMusicInfo(musicInfo)
		Dim mUri As String = GetDefaultAudioUrl(musicInfo)
		Dim fpath As String = GetFileName(musicInfo, mUri, path)
		Try
			SaveAudio(GetAudioContentBytes(mUri), fpath)
		Catch ex As Exception
			PutsError(ex.Message) : Exit Sub
		End Try
		Console.WriteLine($"完成！{vbNewLine}")
		If lyric Then
			Console.Write("正在保存歌词(文件名与音乐文件名相同)...")
			SaveLyrics(musicInfo, path)
			Console.WriteLine("完成！")
		End If
		Console.WriteLine($"歌曲 [{musicInfo.Name} - {musicInfo.Artist}] 已下载至目录 ""{fpath}""{vbNewLine}")
	End Sub
	Public Sub SaveLyrics(musicInfo As MusicInfo, path As String)
		Dim file = New IO.FileInfo(path + musicInfo.Name + " - " + musicInfo.Artist + ".lrc").CreateText()
		For Each item As String In musicInfo.Lyric
			file.WriteLine(item + vbNewLine)
		Next
		file.Close()
		file.Dispose()
	End Sub
	Public Function GetMusicID(url As String) As String
		' http://www.kuwo.cn/play_detail/27781486
		' 检查url格式
		Dim uri As New Uri(url)
		If Not uri.AbsoluteUri.StartsWith("http://www.kuwo.cn/") Or uri.AbsoluteUri.StartsWith("m.kuwo.cn") Then
			Throw New ArgumentException($"错误的url：[{url}]！")
		End If
		' 分析url，获取musicid
		Dim retMusicId As String =
			uri.AbsoluteUri.
			Replace("http://www.kuwo.cn/play_detail/", "").
			Replace("http://www.kuwo.cn/playlist_detail/", "").
			Replace("http://m.kuwo.cn/newh5app/api/mobile/v1/music/playlist/", "").
			Replace("m.kuwo.cn/newh5app/", "").
			Replace("/", "").Replace("\", "")
		If IsNumeric(retMusicId) Then Return retMusicId Else Throw New Exception("[musicId] 解析失败！")
	End Function
	Public Function GetMusicInfoJson(musicId As String) As String
		Dim json As String
		json = GetPageContentDocument(jsonURL.Replace("{musicId}", musicId))
		Return json
	End Function
	Public Function GetMusicInfo(json As String) As MusicInfo
		Return New MusicInfo(json)
	End Function
	Public Function GetDefaultAudioUrl(musicInfo As MusicInfo) As String
		' 分析数据，获取源音频url
		Dim url As New Text.StringBuilder(musicURL)
		url.Replace("{format}", musicInfo.Formats.Split("|")(0))
		url.Replace("{rid}", musicInfo.musicId)
		url.Replace("{br}", musicInfo.coopFormats(0))
		Dim audioJson As String = GetPageContentDocument(url.ToString())
		Dim audioUrl As String = New TextParser(audioJson).ExtractOne("url""(\s*):(\s*)""", """")
		Return audioUrl
	End Function
	Public Function GetAudioContentBytes(url As String) As Byte()
		Return GetContent(url)
	End Function
	Public Function GetFileName(musicInfo As MusicInfo, audioUrl As String, dir As String) As String
		Dim extension As String = audioUrl.Replace(".kuwo.cn", "")
		extension = TextParser.ExtractOne(extension, "\.", "$").Replace("/", "").Replace("\", "")
		Return $"{dir}{musicInfo.Name} - {musicInfo.Artist}.{extension}"
	End Function
	Public Sub PutsMusicInfo(musicInfo As MusicInfo)
		Console.ResetColor()
		Console.Write(
$"已抓获歌曲内容如下：
      [音源] 酷我音乐
      [曲名] {musicInfo.Name}
      [歌手] {musicInfo.Artist}
      [专辑] {musicInfo.Album}
      [格式] {musicInfo.Formats}
      [音质] {EnumListInString(musicInfo.coopFormats)}
正在下载..."
		)
	End Sub
	Private Function EnumListInString(list As List(Of String)) As String
		Dim ret As New Text.StringBuilder()
		For Each item As String In list
			If item IsNot list(list.Count - 1) Then ret.Append(item + "|") Else ret.Append(item)
		Next
		Return ret.ToString()
	End Function
	Private Function GetPlistName(page As String) As String
		Return TextParser.ExtractOne(page, "<title>(\s*)", "_酷我音乐(\s*)</title>")
	End Function
	Private Function GetLyric(json As String) As List(Of String)
		Dim ret As New List(Of String)
		' 第一步，获取整个数组
		Dim lrclist As String = TextParser.ExtractOne(json, "lrclist"":\[", "\]")
		' 第二步，拆分数组
		Dim line As String
		For Each item As String In TextParser.Extract(lrclist, "{", "}")
			line = $"[{TextParser.ExtractOne(item, """time"":""", """}")}]{TextParser.ExtractOne(item, "{""lineLyric"":""", """,")}"
			ret.Add(line)
		Next
		Return ret
	End Function
	Public Function GetPlayList(page As String) As List(Of String)
		Dim ret As New List(Of String)
		For Each s As String In TextParser.RegParse(page, "<a(\s*)title=""(.+?)""(\s*)href=""(.+?)""(\s*)class=""name""(\s*)data-v(.+?)>")
			ret.Add(HostURL + TextParser.ExtractOne(s, "href=""", """"))
		Next
		Console.WriteLine($"共抓取到 [{ret.Count}] 首歌曲，即将开始下载...{vbNewLine}")
		Return ret
	End Function
	Public Function GetSelfMadePlayListName(url As String) As String
		Dim page As String = New WebProtocol(url, False).GetContentDocument()
		Return GetPlistName(page)
	End Function
	Public Function GetSelfMadePlayList(listId As String) As List(Of String)
		Dim ret As New List(Of String)
		Dim i As Integer = 0
		Dim page = New WebProtocol(HandphoneJsonUrl.Replace("{id}", listId).Replace("{pn}", i.ToString).Replace("{rn}", "1"), False).GetContentDocument()
		Dim total As String = TextParser.ExtractOne(page, """total"":", ",")
		Do
			i += 1
			page = New WebProtocol(HandphoneJsonUrl.Replace("{id}", listId).Replace("{pn}", i.ToString).Replace("{rn}", total), False).GetContentDocument()
			For Each s As String In TextParser.Extract(page, """id"":", ",""name""")
				ret.Add("http://www.kuwo.cn/play_detail/" + s)
			Next
		Loop Until i * 100 > total
		Console.WriteLine($"共抓取到 [{ret.Count}] 首歌曲，即将开始下载...{vbNewLine}")
		Return ret
	End Function
End Class

Public Structure MusicInfo
	Dim Name As String
	Dim Album As String
	Dim AlbumId As String
	Dim Artist As String
	Dim coopFormats As List(Of String)
	Dim Formats As String
	Dim musicId As String
	Dim picUrl As String
	Dim reqid As String
	Dim Lyric As List(Of String)

	Sub New(json As String)
		If TextParser.ExtractOne(json, "msg"":""", """") = "音乐查询失败" Then Throw New Exception("未能成功请求到歌曲！")
		Dim tp As New TextParser(json.Replace(vbNewLine, "").Replace(vbCr, "").Replace(vbCr, ""))
		' 截取片段 [songinfo]
		tp = New TextParser(tp.RegParseOne("songinfo(.*)\}"))
		' 分析歌名
		Name = tp.ExtractOne("songName"":""", """")
		' 分析专辑名
		Album = tp.ExtractOne("album"":""", """")
		' 分析专辑Id
		AlbumId = tp.ExtractOne("albumId"":""", """")
		' 分析作者（艺术家）
		Artist = tp.ExtractOne("artist"":""", """")
		' 分析音乐品质（比特率）[coopFormat]
		coopFormats = New List(Of String)
		Dim list As List(Of String) = TextParser.RegParse(tp.ExtractOne("\[", "\]"), "\""(.+?)\""")
		For Each f As String In list ' 去除多余的引号
			coopFormats.Add(f.Replace("""", ""))
		Next
		If coopFormats.Count = 0 Then coopFormats.Add("128kmp3") ' 为防止没有提供音乐品质而自动填充
		' 分析音频格式
		Formats = tp.ExtractOne("formats"":""", """")
		' 分析音乐Id
		musicId = tp.ExtractOne("musicrId"":""MUSIC_", """")
		' 分析封面图片链接
		picUrl = tp.ExtractOne("pic"":""", """")
		' 分析请求id [reqid]
		reqid = tp.ExtractOne("reqid"":""", """")
		' 分析歌词
		Lyric = GetLyric(json)
	End Sub
	Private Function GetLyric(json As String) As List(Of String)
		Dim ret As New List(Of String)
		' 第一步，获取整个数组
		Dim lrclist As String = TextParser.ExtractOne(json, "lrclist"":\[", "\]")
		' 第二步，拆分数组
		Dim line As String
		For Each item As String In TextParser.Extract(lrclist, "{", "}")
			line = $"[{TextParser.ExtractOne(item, """time"":""", """}")}]{TextParser.ExtractOne(item, "{""lineLyric"":""", """,")}"
			ret.Add(line)
		Next
		Return ret
	End Function
End Structure