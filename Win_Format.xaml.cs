using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace DesktopNote
{
    public partial class Win_Format : RoundedWindow
    {
        public MainWindow MainWin;

        public RichTextBox RTB_Main
        {
            get { return (RichTextBox)GetValue(RTB_MainProperty); }
            set { SetValue(RTB_MainProperty, value); }
        }
        public static readonly DependencyProperty RTB_MainProperty =
            DependencyProperty.Register("RTB_Main", typeof(RichTextBox), typeof(Win_Format), new PropertyMetadata(null));


        public Win_Format(MainWindow mainwin)
        {
            InitializeComponent();
            UpdateTargets(mainwin);
        }

        public void UpdateTargets(MainWindow mainwin)
        {
            MainWin = mainwin;
            RTB_Main = mainwin.RTB_Main;
        }

        private void FB1_Loaded(object sender, RoutedEventArgs e)
        {
            //remove margin in ColorPicker for better looks
            var cp = (Xceed.Wpf.Toolkit.ColorPicker)CP_Paper.Content;
            ((Grid)((ContentControl)cp.Template.FindName("PART_ColorPickerToggleButton", cp)).Content).Margin = new Thickness(0);
            cp = (Xceed.Wpf.Toolkit.ColorPicker)CP_Font.Content;
            ((Grid)((ContentControl)cp.Template.FindName("PART_ColorPickerToggleButton", cp)).Content).Margin = new Thickness(0);
            cp = (Xceed.Wpf.Toolkit.ColorPicker)CP_Back.Content;
            ((Grid)((ContentControl)cp.Template.FindName("PART_ColorPickerToggleButton", cp)).Content).Margin = new Thickness(0);
        }

        private void FB1_FadingIn(object sender, RoutedEventArgs e)
        {
            //initialize values
            LoadValues();
        }

        /// <summary>
        /// Manually refresh control values.
        /// </summary>
        internal void LoadValues()
        {
            WinAPI.BringToTop(this);
            //load filename
            WTB_FileName.Text = MainWin.CurrentSetting.Doc_FileName;
            //load paper color
            ((Xceed.Wpf.Toolkit.ColorPicker)CP_Paper.Content).SelectedColor = MainWin.CurrentSetting.PaperColor;

            if (!RTB_Main.Selection.IsEmpty) {
                //load font
                if (RTB_Main.Selection.GetPropertyValue(TextElement.FontFamilyProperty) is FontFamily caretfont)
                    App.FormatWindow.CB_Font.SelectedValue = caretfont.Source;
                else //multiple fonts
                    App.FormatWindow.CB_Font.SelectedIndex = -1;
                App.FormatWindow.CB_Font.ToolTip = (string)App.Res["tooltip_font_selection"];
                //load font color
                var fontcolorpicker = (Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Font.Content;
                if (RTB_Main.Selection.GetPropertyValue(TextElement.ForegroundProperty) is SolidColorBrush caretfontcolor)
                    fontcolorpicker.SelectedColor = new Color?(caretfontcolor.Color);
                else fontcolorpicker.SelectedColor = null;
                //load back color
                var backcolorpicker = (Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Back.Content;
                if (RTB_Main.Selection.GetPropertyValue(TextElement.BackgroundProperty) is SolidColorBrush caretbackcolor)
                    backcolorpicker.SelectedColor = new Color?(caretbackcolor.Color);
                else backcolorpicker.SelectedColor = null;
            }
            else {
                //load global font
                App.FormatWindow.CB_Font.SelectedValue = MainWin.CurrentSetting.Font;
                App.FormatWindow.CB_Font.ToolTip = (string)App.Res["tooltip_font_default"];
                //load global font color
                ((Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Font.Content).SelectedColor = MainWin.CurrentSetting.FontColor;
                //load global back color
                ((Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Back.Content).SelectedColor = MainWin.CurrentSetting.BackColor;
            }
        }

        #region Menu Events
        private void ColorChange(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue && sender is Xceed.Wpf.Toolkit.ColorPicker cp && cp.IsOpen) {
                switch (((ContentControl)cp.Parent).Name) {
                    case "CP_Font":
                        if (!RTB_Main.Selection.IsEmpty) //only change selected
                            RTB_Main.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(e.NewValue.Value));
                        else //change default
                        {
                            RTB_Main.Foreground = new SolidColorBrush(e.NewValue.Value);
                            MainWin.CurrentSetting.FontColor = e.NewValue.Value;
                        }
                        break;
                    case "CP_Back":
                        if (!RTB_Main.Selection.IsEmpty) //only change selected
                            RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(e.NewValue.Value)); //the caret color will be changed as well
                        else //change default
                        {
                            RTB_Main.Background = new SolidColorBrush(e.NewValue.Value);
                            MainWin.CurrentSetting.BackColor = e.NewValue.Value;
                        }
                        break;
                    case "CP_Paper":
                        MainWin.Rec_BG.Fill = new SolidColorBrush(e.NewValue.Value);
                        MainWin.CurrentSetting.PaperColor = e.NewValue.Value;
                        break;
                }
            }
        }

        internal void ToggleStrike(object sender, RoutedEventArgs e)
        {
            //strike-through
            dynamic tdc = RTB_Main.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (tdc == DependencyProperty.UnsetValue || tdc.Count > 0)
                tdc = null;
            else
                tdc = TextDecorations.Strikethrough;
            RTB_Main.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, tdc);
        }

        internal void ToggleHighlight(object sender, RoutedEventArgs e)
        {
            if (RTB_Main.Selection.GetPropertyValue(TextElement.BackgroundProperty) is SolidColorBrush)
                RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            else {
                RTB_Main.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
                RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
            }
        }

        internal void IncreaseSize(object sender, RoutedEventArgs e)
        {
            if (RTB_Main.Selection.IsEmpty)
                RTB_Main.FontSize += 1;
            else {
                var ele = RTB_Main.Selection.Start.GetNextContextPosition(LogicalDirection.Forward)?.GetAdjacentElement(LogicalDirection.Forward);
                if (ele == null) return;
                Image img = null;
                switch (ele.GetType().Name) {
                    case "BlockUIContainer":
                        img = ((BlockUIContainer)ele).Child as Image;
                        break;
                    case "InlineUIContainer":
                        img = ((InlineUIContainer)ele).Child as Image;
                        break;
                    case "Image":
                        img = (Image)ele;
                        break;
                }
                if (img != null) {
                    img.Width += 2;
                    img.Height += 2;
                }
                else
                    EditingCommands.IncreaseFontSize.Execute(null, RTB_Main);
            }
        }

        internal void DecreaseSize(object sender, RoutedEventArgs e)
        {
            if (RTB_Main.Selection.IsEmpty) {
                if (RTB_Main.FontSize > 1) RTB_Main.FontSize -= 1;
            }
            else {
                var ele = RTB_Main.Selection.Start.GetNextContextPosition(LogicalDirection.Forward)?.GetAdjacentElement(LogicalDirection.Forward);
                if (ele == null) return;
                Image img = null;
                switch (ele.GetType().Name) {
                    case "BlockUIContainer":
                        img = ((BlockUIContainer)ele).Child as Image;
                        break;
                    case "InlineUIContainer":
                        img = ((InlineUIContainer)ele).Child as Image;
                        break;
                    case "Image":
                        img = (Image)ele;
                        break;
                }
                if (img != null) {
                    if (img.Width > 2 && img.Height > 2) {
                        img.Width -= 2;
                        img.Height -= 2;
                    }
                }
                else
                    EditingCommands.DecreaseFontSize.Execute(null, RTB_Main);
            }
        }

        internal void PasteAsText(object sender, RoutedEventArgs e)
        {
            var txt = Clipboard.GetText();
            RTB_Main.CaretPosition.InsertTextInRun(txt);
            var newpos = RTB_Main.CaretPosition.GetPositionAtOffset(txt.Length);
            if (newpos != null) RTB_Main.CaretPosition = newpos;
        }

        internal void Find(object sender, RoutedEventArgs e)
        {
            FadeOut();
            var win = new Win_Search(MainWin);
            win.FadeIn();
        }

        #endregion

        private void CB_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (App.FormatWindow.Opacity == 1 && e.AddedItems.Count == 1) {
                var mi = (ComboBoxItem)e.AddedItems[0];

                if (!RTB_Main.Selection.IsEmpty) //only change selected
                    RTB_Main.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, mi.FontFamily);
                else //change default
                {
                    RTB_Main.FontFamily = mi.FontFamily;
                    MainWin.CurrentSetting.Font = mi.FontFamily.Source;
                }
            }
        }

        private void Button_Options_Click(object sender, RoutedEventArgs e)
        {
            FadeOut();
            new Win_Options(MainWin).FadeIn();
        }

        private void Button_NewNote_Click(object sender, RoutedEventArgs e)
        {
            Helpers.NewNote(MainWin.CurrentSetting);
            Close();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            switch (Helpers.MsgBox("msgbox_delete_confirm", button: MessageBoxButton.YesNoCancel, image: MessageBoxImage.Exclamation)) {
                case MessageBoxResult.Yes:
                    File.Delete(MainWin.CurrentSetting.Doc_Location);
                    File.Delete(MainWin.CurrentSetting.Bak_Location);
                    Setting.NoteList.Remove(MainWin.CurrentSetting.Doc_Location);
                    MainWin.Close(true); FadeOut(); break;
                case MessageBoxResult.No:
                    Setting.NoteList.Remove(MainWin.CurrentSetting.Doc_Location);
                    MainWin.Close(true); FadeOut(); break;
                default:
                    return;
            }

            //exit app if all windows are closed
            if (App.MainWindows.Count == 0) App.Quit();
        }

        private void Button_OpenNote_Click(object sender, RoutedEventArgs e)
        {
            var path = Helpers.OpenFileDialog(this, false, MainWin.CurrentSetting.Doc_Location);
            if (path == null) return;

            Helpers.OpenNote(path)?.Show();
            Close();
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Quit();
        }
        
        private void WTB_FileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var dir = Path.GetDirectoryName(MainWin.CurrentSetting.Doc_Location);
            MainWin.CurrentSetting.Doc_Location = (string.IsNullOrEmpty(dir) ? dir : dir + @"\") + WTB_FileName.Text;
        }
    }
}
