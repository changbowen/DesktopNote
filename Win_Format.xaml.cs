using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace DesktopNote
{
    public partial class Win_Format : RoundedWindow
    {
        public MainWindow MainWin;
        public RichTextBox RTB_Main;

        /// <param name="owner">Setting owner causes Win_Format to close when the owner window closes.</param>
        public Win_Format(Window owner, MainWindow mainwin)
        {
            InitializeComponent();
            UpdateCaller(owner, mainwin);
        }

        public void UpdateCaller(Window owner, MainWindow mainwin)
        {
            if (owner != null) Owner = owner;
            MainWin = mainwin;
            RTB_Main = mainwin.RTB_Main;
        }

        #region Menu Events
        private void ColorChange(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue && ((Xceed.Wpf.Toolkit.ColorPicker)sender).IsOpen)
            {
                var cp = (ContentPresenter)VisualTreeHelper.GetParent((DependencyObject)sender);
                if (cp != null)
                {
                    switch (cp.Name)
                    {
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
                        //moved to Win_Options.
                        //case "CP_Paper":
                        //    App.mainwin.Rec_BG.Fill = new SolidColorBrush(e.NewValue.Value);
                        //    Properties.Settings.Default.PaperColor = e.NewValue.Value;
                        //    break;
                    }
                }
            }
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Quit(true);
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
            var tdc = RTB_Main.Selection.GetPropertyValue(TextElement.BackgroundProperty) as SolidColorBrush;
            if (tdc != null)
                RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            else
            {
                RTB_Main.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
                RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
            }
        }

        internal void IncreaseSize(object sender, RoutedEventArgs e)
        {
            if (RTB_Main.Selection.IsEmpty)
                RTB_Main.FontSize += 1;
            else
            {
                var ele = RTB_Main.Selection.Start.GetNextContextPosition(LogicalDirection.Forward).GetAdjacentElement(LogicalDirection.Forward);
                Image img = null;
                switch (ele.GetType().Name)
                {
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
                if (img != null)
                {
                    img.Width += 2;
                    img.Height += 2;
                }
                else
                    EditingCommands.IncreaseFontSize.Execute(null, RTB_Main);
            }
        }

        internal void DecreaseSize(object sender, RoutedEventArgs e)
        {
            if (RTB_Main.Selection.IsEmpty)
            {
                if (RTB_Main.FontSize > 1) RTB_Main.FontSize -= 1;
            }
            else
            {
                var ele = RTB_Main.Selection.Start.GetNextContextPosition(LogicalDirection.Forward).GetAdjacentElement(LogicalDirection.Forward);
                Image img = null;
                switch (ele.GetType().Name)
                {
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
                if (img != null)
                {
                    if (img.Width > 2 && img.Height > 2)
                    {
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
            win.FadeIn(
                MainWin.Left + (MainWin.Width - win.Width) / 2,
                MainWin.Top + (MainWin.Height - win.Height) / 2
                );
        }

        #endregion

        private void Button_Options_Click(object sender, RoutedEventArgs e)
        {
            FadeOut();
            new Win_Options(MainWin).FadeIn();
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
            if (!RTB_Main.Selection.IsEmpty)
            {
                var caretfont = RTB_Main.Selection.GetPropertyValue(TextElement.FontFamilyProperty) as FontFamily;
                if (caretfont != null)
                    App.FormatWindow.CB_Font.SelectedValue = caretfont.Source;
                else //multiple fonts
                    App.FormatWindow.CB_Font.SelectedIndex = -1;
                App.FormatWindow.CB_Font.ToolTip = (string)Application.Current.Resources["tooltip_font_selection"];

                var caretfontcolor = RTB_Main.Selection.GetPropertyValue(TextElement.ForegroundProperty) as SolidColorBrush;
                var fontcolorpicker = (Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Font.Content;
                if (caretfontcolor != null) fontcolorpicker.SelectedColor = new Color?(caretfontcolor.Color);
                else fontcolorpicker.SelectedColor = null;

                var caretbackcolor = RTB_Main.Selection.GetPropertyValue(TextElement.BackgroundProperty) as SolidColorBrush;
                var backcolorpicker = (Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Back.Content;
                if (caretbackcolor != null) backcolorpicker.SelectedColor = new Color?(caretbackcolor.Color);
                else backcolorpicker.SelectedColor = null;
            }
            else
            {
                App.FormatWindow.CB_Font.SelectedValue = MainWin.CurrentSetting.Font;
                App.FormatWindow.CB_Font.ToolTip = (string)Application.Current.Resources["tooltip_font_default"];
                ((Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Font.Content).SelectedColor = MainWin.CurrentSetting.FontColor;
                ((Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Back.Content).SelectedColor = MainWin.CurrentSetting.BackColor;
            }
        }

        public static void NewNote(int refidx = -1)
        {
            var set = Properties.Settings.Default;
            try
            {
                int newidx = 0;
                foreach (System.Configuration.SettingsPropertyValue propval in set.PropertyValues)
                {
                    if (propval.Property.PropertyType == typeof(StringCollection))
                    {
                        if (refidx == -1)//create from default
                        {
                            var defval = XElement.Parse((string)propval.Property.DefaultValue).Element("string").Value;
                            newidx = ((StringCollection)propval.PropertyValue).Add(defval);
                        }
                        else//create from specified index
                        {
                            newidx = ((StringCollection)propval.PropertyValue).Add(((StringCollection)propval.PropertyValue)[refidx]);
                        }
                    }
                }

                //set location to new
                var now = DateTime.Now.ToString("yyyyMMddHHmmss");
                var path = Environment.CurrentDirectory + "\\" + "DesktopNoteContent" + @"_" + now;
                set.Doc_Location[newidx] = path;
                set.Bak_Location[newidx] = path + @".txt";

                var win = new MainWindow(newidx);
                App.MainWindows.Add(win);
                win.Show();
                win.Top += 20d; win.Left += 20d;
            }
            catch { }
        }

        private void Button_NewNote_Click(object sender, RoutedEventArgs e)
        {
            NewNote(MainWin.CurrentSetting.SettingIndex);
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show((string)Application.Current.Resources["msgbox_delete_confirm"], "", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
            var set = Properties.Settings.Default;
            try
            {
                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        break;
                    case MessageBoxResult.Yes:
                        System.IO.File.Delete(MainWin.CurrentSetting.Doc_Location);
                        System.IO.File.Delete(MainWin.CurrentSetting.Bak_Location);
                        set.Doc_Location[MainWin.CurrentSetting.SettingIndex] = "";//deletion will be done after restart
                        set.Save();
                        App.MainWindows[MainWin.CurrentSetting.SettingIndex] = null;
                        MainWin.Close();
                        FadeOut();
                        break;
                    case MessageBoxResult.No:
                        set.Doc_Location[MainWin.CurrentSetting.SettingIndex] = "";//deletion will be done after restart
                        set.Save();
                        App.MainWindows[MainWin.CurrentSetting.SettingIndex] = null;
                        MainWin.Close();
                        FadeOut();
                        break;
                }
                //check if the last window was closed
                if (!App.MainWindows.Where(w => w != null).Any())
                {
                    NewNote();
                }
            }
            catch { }
        }
    }
}
