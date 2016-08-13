Class Application
    Private Declare Unicode Function PathIsNetworkPath Lib "shlwapi" Alias "PathIsNetworkPathW" (ByVal pszPath As String) As Boolean

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Private Sub RunCheck()
        AddHandler AppDomain.CurrentDomain.AssemblyResolve,
            Function(sender As Object, e As ResolveEventArgs)
                Dim desiredAssembly = New Reflection.AssemblyName(e.Name).Name
                If desiredAssembly = "Xceed.Wpf.Toolkit" Then
                    Dim ressourceName = "DesktopNote." + desiredAssembly + ".dll"
                    Using stream = Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(ressourceName)
                        Dim assemblyData(CInt(stream.Length)) As Byte
                        stream.Read(assemblyData, 0, assemblyData.Length)
                        Return Reflection.Assembly.Load(assemblyData)
                    End Using
                Else
                    Return Nothing
                End If
            End Function

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
