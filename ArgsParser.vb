Public Class ArgsParser
	Public Help As Boolean = False
	Public Kuwo As Boolean = False
	Public Lyric As Boolean = False
	Public url As String
	Protected Arguments As String()

	Public Sub New(args As String())
		Arguments = args
	End Sub
	Public Function Parse() As ArgsParser
		For Each arg In Arguments
			arg = LCase(arg)
			If TestArg(arg, "help") Then Help = True : Return Me
			If TestArg(arg, "lyric") Then Lyric = True : Continue For

			If arg.Contains("kuwo.cn") Then Kuwo = True
			If arg IsNot "" Then url = arg : Return Me
		Next

		Return Me
	End Function
	Public Function TestArg(arg As String, tar As String) As Boolean
		Dim retBoolean As Boolean = False
		If (arg = tar) Or (arg = "-" + tar(0)) Or (arg = "/" + tar(0)) Or (arg = "/" + tar) Or (arg = "-" + tar) Then retBoolean = True
		Return retBoolean
	End Function
End Class
