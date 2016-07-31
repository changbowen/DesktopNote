Imports System.Threading

Class MainWindow
    Private Lock_Save As New Object
    Private Shared CountDown As Integer = 0
    Private doc_loc As String = System.AppDomain.CurrentDomain.BaseDirectory & My.Settings.Doc_Location
    Private assname As String = System.Reflection.Assembly.GetExecutingAssembly.GetName.Name
    Private mousepos As Point

#Region "Docking"
    Private lastdockstatus As DockStatus
    Private currScrnRect As Rect
    Private Property dockedto As DockStatus
        Get
            Return CType(GetValue(dockedtoProperty), DockStatus)
        End Get
        Set(value As DockStatus)
            SetValue(dockedtoProperty, value)
        End Set
    End Property
    Private Shared ReadOnly dockedtoProperty As DependencyProperty = DependencyProperty.Register("dockedto", GetType(DockStatus), GetType(MainWindow), New UIPropertyMetadata(DockStatus.None))

    Private Enum DockStatus
        None
        Docking
        Left
        Right
        Top
        Bottom
    End Enum

    Friend Class GetCurrentMon
        Private Const MONITOR_DEFAULTTOPRIMERTY As Integer = &H1
        Private Const MONITOR_DEFAULTTONEAREST As Integer = &H2
        Private Declare Auto Function MonitorFromWindow Lib "user32" (hwnd As IntPtr, flags As Integer) As IntPtr
        Private Declare Auto Function GetMonitorInfo Lib "user32" (hwnd As IntPtr, ByRef mInfo As MonitorInfo) As Boolean

        <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)> Private Structure MonitorInfo
            Public cbSize As UInteger
            Public rcMonitor As Rect2
            Public rcWork As Rect2
            Public dwFlags As UInteger
        End Structure

        <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)> Private Structure Rect2
            Public left As Integer
            Public top As Integer
            Public right As Integer
            Public bottom As Integer
        End Structure

        Public Function GetInfo() As Rect
            Dim mi As New MonitorInfo
            mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(GetType(MonitorInfo))
            Dim hwmon = MonitorFromWindow(New Interop.WindowInteropHelper(Application.Current.MainWindow).EnsureHandle, MONITOR_DEFAULTTOPRIMERTY)
            If GetMonitorInfo(hwmon, mi) Then
                'convert to device-independent vaues
                Dim mon = mi.rcMonitor
                Dim realp1 As Point
                Dim realp2 As Point
                With PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformFromDevice
                    realp1 = .Transform(New Point(mon.left, mon.top))
                    realp2 = .Transform(New Point(mon.right, mon.bottom))
                End With
                Return New Rect(realp1, realp2)
            Else
                Throw New Exception("Failed to get monitor info.")
            End If
        End Function
    End Class

    'Protected Overrides Sub OnSourceInitialized(e As EventArgs)
    '    MyBase.OnSourceInitialized(e)
    '    Dim source As Interop.HwndSource = PresentationSource.FromVisual(Me)
    '    source.AddHook(New Interop.HwndSourceHook(AddressOf WndProc))
    'End Sub

    'Public Function WndProc(hwnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr, ByRef handled As Boolean) As IntPtr
    '    If msg = &H232 AndAlso My.Settings.AutoDock = True Then 'catch WM_EXITSIZEMOVE event
    '        DockToSide(True) 'thread is STA
    '    End If
    'End Function

    Private Sub DockToSide(Optional changpos As Boolean = False)
        If dockedto = DockStatus.None Then
            Dim toval As Double, tgtpro As DependencyProperty, pad As Double = 15, dockto As DockStatus
            If changpos Then
                currScrnRect = New GetCurrentMon().GetInfo

                If Me.Left <= currScrnRect.Left Then 'dock left
                    toval = currScrnRect.Left - Me.ActualWidth + pad
                    tgtpro = Window.LeftProperty
                    dockto = DockStatus.Left
                ElseIf Me.Left + Me.ActualWidth >= currScrnRect.Right Then 'dock right
                    toval = currScrnRect.Right - pad
                    tgtpro = Window.LeftProperty
                    dockto = DockStatus.Right
                ElseIf Me.Top <= currScrnRect.Top Then 'dock top
                    toval = currScrnRect.Top - Me.ActualHeight + pad
                    tgtpro = Window.TopProperty
                    dockto = DockStatus.Top
                ElseIf Me.Top + Me.ActualHeight >= currScrnRect.Bottom Then 'dock bottom
                    toval = currScrnRect.Bottom - pad
                    tgtpro = Window.TopProperty
                    dockto = DockStatus.Bottom
                Else
                    lastdockstatus = DockStatus.None
                    Topmost = False
                    Exit Sub
                End If
                lastdockstatus = dockto
            Else 'restore last docking position
                dockto = lastdockstatus
                Select Case lastdockstatus
                    Case DockStatus.Left
                        toval = currScrnRect.Left - Me.ActualWidth + pad
                        tgtpro = Window.LeftProperty
                    Case DockStatus.Right
                        toval = currScrnRect.Right - pad
                        tgtpro = Window.LeftProperty
                    Case DockStatus.Top
                        toval = currScrnRect.Top - Me.ActualHeight + pad
                        tgtpro = Window.TopProperty
                    Case DockStatus.Bottom
                        toval = currScrnRect.Bottom - pad
                        tgtpro = Window.TopProperty
                    Case Else
                        Exit Sub
                End Select
            End If

            Topmost = True
            'Debug.Print("dock with changepos " & changpos.ToString & ", to " & toval)
            Dim anim_move As New Animation.DoubleAnimation(toval, New Duration(New TimeSpan(0, 0, 0, 0, 500)), Animation.FillBehavior.Stop)
            anim_move.EasingFunction = New Animation.CubicEase With {.EasingMode = Animation.EasingMode.EaseOut}
            Dim anim_fade As New Animation.DoubleAnimation(0.4, New Duration(New TimeSpan(0, 0, 0, 0, 300)))
            anim_fade.BeginTime = New TimeSpan(0, 0, 0, 0, 200)
            Dim anim_prop As New Animation.ObjectAnimationUsingKeyFrames
            anim_prop.KeyFrames.Add(New Animation.DiscreteObjectKeyFrame(DockStatus.Docking, Animation.KeyTime.FromTimeSpan(New TimeSpan(0, 0, 0))))
            anim_prop.KeyFrames.Add(New Animation.DiscreteObjectKeyFrame(dockto, Animation.KeyTime.FromTimeSpan(New TimeSpan(0, 0, 0, 0, 500))))
            Me.BeginAnimation(tgtpro, anim_move)
            Me.BeginAnimation(Window.OpacityProperty, anim_fade)
            Me.BeginAnimation(dockedtoProperty, anim_prop)
        End If
    End Sub

    Private Sub UnDock()
        If dockedto <> DockStatus.Docking AndAlso dockedto <> DockStatus.None Then
            Dim toval As Double, tgtpro As DependencyProperty, pad As Double = 15, dockto As DockStatus
            If dockedto = DockStatus.Left Then ' Me.Left = currScrnRect.left - Me.ActualWidth + pad Then
                toval = currScrnRect.Left
                tgtpro = Window.LeftProperty
                dockto = DockStatus.None
            ElseIf dockedto = DockStatus.Right Then 'Me.Left = currScrnRect.right - pad Then
                toval = currScrnRect.Right - Me.ActualWidth
                tgtpro = Window.LeftProperty
                dockto = DockStatus.None
            ElseIf dockedto = DockStatus.Top Then 'Me.Top = currScrnRect.top - Me.ActualHeight + pad Then
                toval = currScrnRect.Top
                tgtpro = Window.TopProperty
                dockto = DockStatus.None
            ElseIf dockedto = DockStatus.Bottom Then 'Me.Top = currScrnRect.bottom - pad Then
                toval = currScrnRect.Bottom - Me.ActualHeight
                tgtpro = Window.TopProperty
                dockto = DockStatus.None
            Else
                Exit Sub 'useless
            End If

            Topmost = True
            'Debug.Print("undick " & toval)
            Dim anim_move As New Animation.DoubleAnimation(toval, New Duration(New TimeSpan(0, 0, 0, 0, 300)), Animation.FillBehavior.Stop)
            anim_move.EasingFunction = New Animation.CubicEase With {.EasingMode = Animation.EasingMode.EaseOut}
            Dim anim_fade As New Animation.DoubleAnimationUsingKeyFrames
            anim_fade.KeyFrames.Add(New Animation.DiscreteDoubleKeyFrame(1, Animation.KeyTime.FromTimeSpan(New TimeSpan(0, 0, 0))))
            Dim anim_prop As New Animation.ObjectAnimationUsingKeyFrames
            anim_prop.KeyFrames.Add(New Animation.DiscreteObjectKeyFrame(DockStatus.Docking, Animation.KeyTime.FromTimeSpan(New TimeSpan(0, 0, 0))))
            anim_prop.KeyFrames.Add(New Animation.DiscreteObjectKeyFrame(dockto, Animation.KeyTime.FromTimeSpan(New TimeSpan(0, 0, 0, 0, 500))))
            Me.BeginAnimation(tgtpro, anim_move)
            Me.BeginAnimation(Window.OpacityProperty, anim_fade)
            Me.BeginAnimation(dockedtoProperty, anim_prop)
        End If
    End Sub

    Private Sub Win_Main_MouseEnter(sender As Object, e As MouseEventArgs) Handles Win_Main.MouseEnter
        'undocking
        If My.Settings.AutoDock Then UnDock()
    End Sub

    Private Sub Win_Main_MouseLeave(sender As Object, e As MouseEventArgs) Handles Win_Main.MouseLeave
        If My.Settings.AutoDock AndAlso Application.Current.Windows.Count = 1 AndAlso Not RTB_Main.IsKeyboardFocused AndAlso Not RTB_Main.ContextMenu.IsOpen Then
            DockToSide()
        End If
    End Sub
#End Region

#Region "Menu Items"
    Private Sub MenuItem_Help_Click(sender As Object, e As RoutedEventArgs)
        MsgBox("Available editing features can be accessd from menu or keyboard combination." & vbCrLf &
               "Use Ctrl + mouse wheel to Change font size" & vbCrLf &
               "Change font or font size when there is a selection will only change selected text." & vbCrLf &
               "Note content will be auto saved to application root.", MsgBoxStyle.Information)
    End Sub

    Private Sub MenuItem_Exit_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub



    Private Sub MenuItem_Bullet_Click(sender As Object, e As RoutedEventArgs)
        'Dim tr = New TextRange(RTB_Main.Document.ContentStart, RTB_Main.CaretPosition)
        'If tr.Text.Length = 0 Then
        '    If RTB_Main.Document.Blocks.Count = 0 Then
        '        RTB_Main.Document.Blocks.Add(New List(New ListItem(New Paragraph)))
        '    Else
        '        RTB_Main.Document.Blocks.InsertBefore(RTB_Main.Document.Blocks(0), New List(New ListItem(New Paragraph)))
        '    End If
        'Else
        '    RTB_Main.Document.Blocks.Add(New List(New ListItem(New Paragraph)))
        'End If
        EditingCommands.ToggleBullets.Execute(Nothing, RTB_Main)
    End Sub

    Private Sub MenuItem_AutoStart_Click(sender As Object, e As RoutedEventArgs) Handles MI_AutoStart.Click
        Dim run = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
        If MI_AutoStart.IsChecked Then
            run.SetValue(assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String)
        Else
            run.DeleteValue(assname, False)
        End If
    End Sub

    Private Sub MenuItem_AutoDock_Click(sender As Object, e As RoutedEventArgs) Handles MI_AutoDock.Click
        If MI_AutoDock.IsChecked Then
            My.Settings.AutoDock = True
            DockToSide(True)
        Else
            My.Settings.AutoDock = False
            Me.Topmost = False
        End If
        My.Settings.Save()
    End Sub

    Private Sub MenuItem_Reset_Click(sender As Object, e As RoutedEventArgs)
        Dim tr As New TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd)
        tr.ClearAllProperties()
    End Sub

    Private Sub ToggleStrike()
        'strike-through
        Dim tdc = RTB_Main.Selection.GetPropertyValue(Inline.TextDecorationsProperty)
        If tdc Is DependencyProperty.UnsetValue OrElse tdc.Count > 0 Then
            tdc = Nothing
        Else
            tdc = TextDecorations.Strikethrough
        End If
        RTB_Main.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, tdc)
    End Sub

    Private Sub PasteAsText()
        'method 1 not working well
        'Dim p1 = RTB_Main.CaretPosition
        'ApplicationCommands.Paste.Execute(Nothing, RTB_Main)
        'Dim p2 = RTB_Main.CaretPosition
        'RTB_Main.Selection.Select(p1, p2)
        'RTB_Main.Selection.ClearAllProperties()

        'method 2
        RTB_Main.CaretPosition.InsertTextInRun(Clipboard.GetText)
    End Sub

    Private Sub Find()
        Dim search_win As New Win_Search
        search_win.Show()
    End Sub
#End Region

#Region "RTB_Main Events"
    Private Sub RTB_Main_TextChanged(sender As Object, e As TextChangedEventArgs) Handles RTB_Main.TextChanged
        If RTB_Main.IsFocused Then
            CountDown = 2000
            TB_Status.Visibility = Windows.Visibility.Hidden
        End If
    End Sub

    Private Sub RTB_Main_KeyDown(sender As Object, e As KeyEventArgs) Handles RTB_Main.PreviewKeyDown
        If e.Key = Key.D AndAlso Keyboard.Modifiers = ModifierKeys.Control Then
            ToggleStrike()
        ElseIf e.Key = Key.V AndAlso Keyboard.Modifiers = ModifierKeys.Control + ModifierKeys.Shift Then
            PasteAsText()
        ElseIf e.Key = Key.F AndAlso Keyboard.Modifiers = ModifierKeys.Control Then
            Find()
        End If
    End Sub

    Private Sub RTB_Main_MouseWheel(sender As Object, e As MouseWheelEventArgs) Handles RTB_Main.PreviewMouseWheel
        'when padding is set on list, changing font size results in incorrect bullet position.
        If Keyboard.Modifiers = ModifierKeys.Control Then
            e.Handled = True
            If e.Delta > 0 Then 'wheel up
                If RTB_Main.Selection.IsEmpty Then
                    RTB_Main.FontSize += 1
                Else
                    Dim ele = RTB_Main.Selection.Start.GetNextContextPosition(LogicalDirection.Forward).GetAdjacentElement(LogicalDirection.Forward)
                    If TypeOf ele Is InlineUIContainer Then
                        Dim img = TryCast(CType(ele, InlineUIContainer).Child, Image)
                        If img IsNot Nothing Then
                            img.Width += 2
                            img.Height += 2
                        Else
                            EditingCommands.IncreaseFontSize.Execute(Nothing, RTB_Main)
                        End If
                    Else
                        EditingCommands.IncreaseFontSize.Execute(Nothing, RTB_Main)
                    End If
                    'RTB_Main.Selection.ApplyPropertyValue(FontSizeProperty, CType(RTB_Main.Selection.GetPropertyValue(FontSizeProperty), Double) + 1)
                End If
            Else
                If RTB_Main.Selection.IsEmpty Then
                    If RTB_Main.FontSize > 1 Then RTB_Main.FontSize -= 1
                Else
                    Dim ele = RTB_Main.Selection.Start.GetNextContextPosition(LogicalDirection.Forward).GetAdjacentElement(LogicalDirection.Forward)
                    If TypeOf ele Is InlineUIContainer Then
                        Dim img = TryCast(CType(ele, InlineUIContainer).Child, Image)
                        If img IsNot Nothing Then
                            If img.Width > 2 AndAlso img.Height > 2 Then
                                img.Width -= 2
                                img.Height -= 2
                            End If
                        Else
                            EditingCommands.DecreaseFontSize.Execute(Nothing, RTB_Main)
                        End If
                    Else
                        EditingCommands.DecreaseFontSize.Execute(Nothing, RTB_Main)
                    End If
                    'Dim oldsize = CType(RTB_Main.Selection.GetPropertyValue(FontSizeProperty), Double)
                    'If oldsize > 1 Then RTB_Main.Selection.ApplyPropertyValue(FontSizeProperty, oldsize - 1)
                End If
            End If
        End If
    End Sub

    Private Sub Rec_BG_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles Rec_BG.MouseLeftButtonDown
        Rec_BG.CaptureMouse()
        If e.ClickCount = 2 Then
            If Me.WindowState = Windows.WindowState.Normal Then
                Me.WindowState = Windows.WindowState.Maximized
            ElseIf Me.WindowState = Windows.WindowState.Maximized Then
                Me.WindowState = Windows.WindowState.Normal
            End If
        Else
            mousepos = e.GetPosition(Me)
        End If
    End Sub

    Private Sub Rec_BG_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Rec_BG.MouseLeftButtonUp
        Rec_BG.ReleaseMouseCapture()
        If My.Settings.AutoDock Then DockToSide(True)
    End Sub

    Private Sub Rec_BG_MouseMove(sender As Object, e As MouseEventArgs) Handles Rec_BG.MouseMove
        If e.LeftButton = MouseButtonState.Pressed AndAlso mousepos <> Nothing Then
            Dim pos = e.GetPosition(Me)
            Me.Left += pos.X - mousepos.X
            Me.Top += pos.Y - mousepos.Y
        End If
    End Sub

    Private Sub RTB_Main_LostKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs) Handles RTB_Main.LostKeyboardFocus
        If My.Settings.AutoDock AndAlso Application.Current.Windows.Count = 1 AndAlso Not RTB_Main.ContextMenu.IsOpen Then
            DockToSide()
        End If
    End Sub
#End Region

    Private Sub Win_Main_Loaded(sender As Object, e As RoutedEventArgs) Handles Win_Main.Loaded
        'check for update
        Task.Run(Sub()
                     Dim updatelocation = "\\SRV496-01CN\PUBLIC_IT\Apps_Update\" & assname & ".exe"
                     If My.Computer.FileSystem.FileExists(updatelocation) Then
                         Dim ass_local = System.Reflection.Assembly.GetExecutingAssembly
                         Dim ass_remote = System.Reflection.AssemblyName.GetAssemblyName(updatelocation)
                         If ass_local.GetName.Version < ass_remote.Version Then
                             MsgBox("There is a new version available. Be sure to check it out at:" & vbCrLf & updatelocation, MsgBoxStyle.Information + MsgBoxStyle.OkOnly, "Update Available!")
                         End If
                     End If
                 End Sub)

        'check and merge previous settings
        If My.Settings.UpgradeFlag = True Then
            My.Settings.Upgrade()
            My.Settings.UpgradeFlag = False
            My.Settings.Save()
        End If

        'load settings
        Me.Width = My.Settings.Win_Size.Width
        Me.Height = My.Settings.Win_Size.Height
        If My.Settings.Win_Pos <> Nothing Then
            Me.Left = My.Settings.Win_Pos.X
            Me.Top = My.Settings.Win_Pos.Y
        End If
        'dockedto = My.Settings.DockedTo
        lastdockstatus = My.Settings.DockedTo
        RTB_Main.FontFamily = New FontFamily(My.Settings.Font)

        'adding fonts to menu
        For Each f In Fonts.SystemFontFamilies
            Dim mi As New MenuItem With {.Header = f.Source, .FontFamily = f}
            mi.ToolTip = f.Source
            If f.Source = My.Settings.Font Then mi.IsChecked = True
            MI_Font.Items.Add(mi)
        Next
        MI_Font.Items.SortDescriptions.Add(New System.ComponentModel.SortDescription("Header", System.ComponentModel.ListSortDirection.Ascending))
        AddHandler MI_Font.Click, Sub(s1 As Object, e1 As RoutedEventArgs)
                                      For Each f As MenuItem In MI_Font.Items
                                          f.IsChecked = False
                                      Next
                                      Dim mi As MenuItem = e1.Source
                                      If Not RTB_Main.Selection.IsEmpty Then 'only change selected
                                          RTB_Main.Selection.ApplyPropertyValue(FontFamilyProperty, mi.FontFamily)
                                      Else 'change every paragraph
                                          mi.IsChecked = True
                                          RTB_Main.FontFamily = mi.FontFamily
                                          My.Settings.Font = mi.FontFamily.Source
                                          For Each b In RTB_Main.Document.Blocks
                                              b.FontFamily = mi.FontFamily
                                          Next
                                      End If
                                  End Sub

        'loading contents
        If My.Computer.FileSystem.FileExists(doc_loc) Then
            Try
                Dim tr As New TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd)
                'tr.Load(New IO.FileStream(doc_loc, IO.FileMode.Open), DataFormats.Xaml)
                tr.Load(New IO.FileStream(doc_loc, IO.FileMode.Open), DataFormats.XamlPackage)
            Catch
                MsgBox("There was an error loading the note contents. Please refer to the following backup file at application root for recovery." & vbCrLf & My.Settings.Bak_Location, MsgBoxStyle.Critical, "Loading Notes Failed")
            End Try
        End If

        RTB_Main.IsUndoEnabled = False
        RTB_Main.IsUndoEnabled = True
        'without the above two lines, Load actions can be undone.

        If RTB_Main.Document.Blocks.Count > 0 Then 'unifying font size for new paragraghs
            RTB_Main.FontSize = RTB_Main.Document.Blocks(0).FontSize
            For Each b In RTB_Main.Document.Blocks
                b.ClearValue(FontSizeProperty)
                'b.SetValue(PaddingProperty, New Thickness(55, 0, 0, 0))
                'If TypeOf b Is List Then
                '    RTB_Main.Resources("ListPadding") = b.Padding
                'End If
            Next

        End If

        'check auto dock
        If My.Settings.AutoDock = True Then MI_AutoDock.IsChecked = True

        'check auto start
        Dim run = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
        Dim run_value As String = run.GetValue(assname)
        If run_value <> "" Then
            MI_AutoStart.IsChecked = True
            If run_value <> System.Reflection.Assembly.GetExecutingAssembly().Location Then
                run.SetValue(assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String)
            End If
        End If

        currScrnRect = New GetCurrentMon().GetInfo

        Dim task_save As New Thread(AddressOf SaveNotes)
        task_save.IsBackground = True
        task_save.Start()
    End Sub

    Private Sub SaveNotes()
        Do
            Do While CountDown <= 0
                Thread.Sleep(1000)
            Loop
            Do
                Thread.Sleep(500)
                CountDown -= 500
            Loop While CountDown > 0
            SaveToXamlPkg()
        Loop
    End Sub

    Private Sub SaveToXamlPkg()
        SyncLock Lock_Save
            Dim tr As TextRange
            Dim isUIthread As Boolean = Dispatcher.CheckAccess
            Dim result As String
            If isUIthread Then
                tr = New TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd)
            Else
                Dispatcher.Invoke(Sub() tr = New TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd))
            End If

            Try

                If isUIthread Then
                    Using ms As New IO.FileStream(doc_loc, IO.FileMode.Create)
                        tr.Save(ms, DataFormats.XamlPackage, True)
                    End Using
                    My.Computer.FileSystem.WriteAllText(My.Settings.Bak_Location, tr.Text, False)
                Else
                    Dispatcher.Invoke(Sub()
                                          Using ms As New IO.FileStream(doc_loc, IO.FileMode.Create)
                                              tr.Save(ms, DataFormats.XamlPackage, True)
                                          End Using
                                          My.Computer.FileSystem.WriteAllText(My.Settings.Bak_Location, tr.Text, False)
                                      End Sub)
                End If

                result = "Saved"
            Catch
                result = "Save failed"
            End Try

            If isUIthread Then
                TB_Status.Text = result
                TB_Status.Visibility = Windows.Visibility.Visible
            Else
                Dispatcher.Invoke(Sub()
                                      TB_Status.Text = result
                                      TB_Status.Visibility = Windows.Visibility.Visible
                                  End Sub)
            End If
        End SyncLock
    End Sub

    Private Sub Win_Main_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Win_Main.Closing
        SaveToXamlPkg()
        My.Settings.Win_Pos = New System.Drawing.Point(Me.Left, Me.Top)
        My.Settings.Win_Size = New System.Drawing.Size(Me.Width, Me.Height)
        My.Settings.DockedTo = lastdockstatus
        My.Settings.Save()
    End Sub

    Private Sub MenuItem_Click(sender As Object, e As RoutedEventArgs)

    End Sub
End Class