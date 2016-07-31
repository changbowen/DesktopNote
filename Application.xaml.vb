Class Application
    Private Declare Unicode Function PathIsNetworkPath Lib "shlwapi" Alias "PathIsNetworkPathW" (ByVal pszPath As String) As Boolean

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Private Sub RunCheck()
        If PathIsNetworkPath(System.AppDomain.CurrentDomain.BaseDirectory) Then
            MsgBox("Please do not run this program from a network location. Copy to a local drive first." & vbCrLf & "Program will exit now.", MsgBoxStyle.Exclamation)
            Application.Current.Shutdown()
            Exit Sub
        End If

        If Process.GetProcessesByName(Process.GetCurrentProcess.ProcessName).Length > 1 Then
            MsgBox("Only one instance of DesktopNote can be running.", MsgBoxStyle.Exclamation)
            Application.Current.Shutdown()
            Exit Sub
        End If

        Dim mainwin As New MainWindow
        mainwin.Show()
    End Sub
End Class
