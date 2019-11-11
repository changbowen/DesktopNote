using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DesktopNote
{
    public partial class Win_Search : RoundedWindow
    {
        public readonly MainWindow MainWin;
        private bool textchanged = true;

        public Win_Search(MainWindow mainwin)
        {
            InitializeComponent();
            Owner = mainwin;
            MainWin = mainwin;
            App.SearchWindow = this;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            App.SearchWindow = null;
        }

        private void MarkTextInRange(RichTextBox richTextBox, string searchText, bool searchNext)
        {
            //Get the range to search
            TextRange searchRange;
            if (searchNext)
                searchRange = new TextRange(richTextBox.Selection.Start.GetPositionAtOffset(1), richTextBox.Document.ContentEnd);
            else
                searchRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

            TextPointer start = searchRange.Start;
            //TextPointer start = searchRange.Start.GetNextContextPosition(LogicalDirection.Forward); why?
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
                    Helpers.MsgBox("msgbox_searched_to_end", button: MessageBoxButton.OK, image: MessageBoxImage.Information);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TB_Search.Focus();
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
                if (textchanged)
                {
                    textchanged = false;
                    MarkTextInRange(MainWin.RTB_Main, TB_Search.Text, false);
                }
                else
                {
                    MarkTextInRange(MainWin.RTB_Main, TB_Search.Text, true);
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
