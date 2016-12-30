using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace DesktopNote
{
    public partial class Win_Format : RoundedWindow
    {
        RichTextBox RTB_Main = App.mainwin.RTB_Main;

        public Win_Format()
        {
            InitializeComponent();
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
                                Properties.Settings.Default.FontColor = e.NewValue.Value;
                            }
                            break;
                        case "CP_Back":
                            if (!RTB_Main.Selection.IsEmpty) //only change selected
                                RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(e.NewValue.Value)); //the caret color will be changed as well
                            else //change default
                            {
                                RTB_Main.Background = new SolidColorBrush(e.NewValue.Value);
                                Properties.Settings.Default.BackColor = e.NewValue.Value;
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
            App.mainwin.Quit(true);
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
            App.fb.FadeOut();
            var win = new Win_Search();
            win.Owner = App.mainwin;
            win.FadeIn(
                App.mainwin.Left + (App.mainwin.Width - win.Width) / 2, 
                App.mainwin.Top + (App.mainwin.Height - win.Height) / 2
                );
        }

        #endregion

        private void Button_Options_Click(object sender, RoutedEventArgs e)
        {
            App.fb.FadeOut();
            new Win_Options() { Owner = App.mainwin }.FadeIn();
        }
    }
}
