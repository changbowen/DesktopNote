using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DesktopNote
{
    /// <summary>
    /// Interaction logic for Win_Search.xaml
    /// </summary>
    public partial class Win_Search : Window
    {
        private bool textchanged = true;

        public Win_Search()
        {
            InitializeComponent();
        }

        private void MarkTextInRange(RichTextBox richTextBox, string searchText, bool searchNext)
        {
            //Get the range to search
            TextRange searchRange;
            if (searchNext)
                searchRange = new TextRange(richTextBox.Selection.Start.GetPositionAtOffset(1), richTextBox.Document.ContentEnd);
            else
                searchRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

            TextPointer start = searchRange.Start.GetNextContextPosition(LogicalDirection.Forward);
            while (start != null)
            {
                var txt = start.GetTextInRun(LogicalDirection.Forward);
                if (txt.Length > 0)
                {
                    int tgtindex = txt.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                    if (tgtindex >= 0)
                    {
                        var selstart = start.GetPositionAtOffset(tgtindex);
                        var selend = start.GetPositionAtOffset(tgtindex + searchText.Length);
                        //if search string is at line start, non chars are included in below msgbox
                        //MsgBox(New TextRange(tgtptr, start.GetNextContextPosition(LogicalDirection.Forward)).Text)
                        ((FrameworkContentElement)selstart.Parent).BringIntoView();
                        richTextBox.Selection.Select(selstart, selend);
                        richTextBox.Focus();
                        break;
                    }
                }
                start = start.GetNextContextPosition(LogicalDirection.Forward);
                if (start == null)
                {
                    textchanged = true;
                    MessageBox.Show((string)Application.Current.Resources["msgbox_searched_to_end"], "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var mainwin = (MainWindow)Application.Current.MainWindow;
            Left = mainwin.Left + (mainwin.Width - Width) / 2;
            Top = mainwin.Top + (mainwin.Height - Height) / 2;
            TB_Search.Focus();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void TB_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            textchanged = true;
        }

        private void Btn_Search_Click(object sender, RoutedEventArgs e)
        {
            if (TB_Search.Text.Trim() == "")
                Close();
            else
            {
                var mainwin = (MainWindow)Application.Current.MainWindow;
                if (textchanged)
                {
                    textchanged = false;
                    MarkTextInRange(mainwin.RTB_Main, TB_Search.Text, false);
                }
                else
                {
                    MarkTextInRange(mainwin.RTB_Main, TB_Search.Text, true);
                }
                Activate();
            }
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
