Public Class Win_Search
    Private textchanged As Boolean = True

    Private Sub Btn_Search_Click(sender As Object, e As RoutedEventArgs) Handles Btn_Search.Click
        If TB_Search.Text.Trim = "" Then
            Me.Close()
        Else
            Dim mainwin As MainWindow = Application.Current.MainWindow
            If textchanged Then
                textchanged = False
                MarkTextInRange(mainwin.RTB_Main, TB_Search.Text, False)
            Else
                MarkTextInRange(mainwin.RTB_Main, TB_Search.Text, True)
            End If
            Me.Activate()
        End If
    End Sub

    Private Sub MarkTextInRange(richTextBox As RichTextBox, searchText As String, searchNext As Boolean)
        'Get the range to search
        Dim searchRange As TextRange
        If searchNext Then
            searchRange = New TextRange(richTextBox.Selection.Start.GetPositionAtOffset(1), richTextBox.Document.ContentEnd)
        Else
            searchRange = New TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd)
        End If

        Dim start As TextPointer = searchRange.Start.GetNextContextPosition(LogicalDirection.Forward)
        Do While start IsNot Nothing
            Dim txt = start.GetTextInRun(LogicalDirection.Forward)

            If txt.Length > 0 Then
                Dim tgtindex As Integer = txt.IndexOf(searchText, StringComparison.OrdinalIgnoreCase)
                If tgtindex >= 0 Then
                    Dim selstart = start.GetPositionAtOffset(tgtindex)
                    Dim selend = start.GetPositionAtOffset(tgtindex + searchText.Length)
                    'if search string is at line start, non chars are included in below msgbox
                    'MsgBox(New TextRange(tgtptr, start.GetNextContextPosition(LogicalDirection.Forward)).Text)
                    CType(selstart.Parent, FrameworkContentElement).BringIntoView()
                    richTextBox.Selection.Select(selstart, selend)
                    richTextBox.Focus()
                    Exit Do
                End If
            End If
            start = start.GetNextContextPosition(LogicalDirection.Forward)
            If start Is Nothing Then
                textchanged = True
                MsgBox("Search has reached the end of the document.", MsgBoxStyle.Information)
            End If
        Loop

        'Do the search
        'Dim offset As Integer = searchRange.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase)
        'If offset >= 0 Then 'found
        '    Dim start As TextPointer = GetTextPositionAtOffset(searchRange.Start, offset)
        '    Dim tr As New TextRange(start, start.DocumentEnd)
        '    Dim result = New TextRange(start, GetTextPositionAtOffset(start, searchText.Length))
        '    richTextBox.Selection.Select(result.Start, result.End)
        'End If
    End Sub

    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton = MouseButton.Left Then
            Me.DragMove()
        End If
    End Sub

    Private Sub Btn_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles Btn_Cancel.Click
        Me.Close()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Dim mainwin As MainWindow = Application.Current.MainWindow
        Me.Left = mainwin.Left + (mainwin.Width - Me.Width) / 2
        Me.Top = mainwin.Top + (mainwin.Height - Me.Height) / 2

        TB_Search.Focus()
    End Sub

    Private Sub TB_Search_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TB_Search.TextChanged
        textchanged = True
    End Sub
End Class
