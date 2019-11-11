//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace DesktopNote
{
    public partial class Win_Options : RoundedWindow
    {
        public readonly MainWindow MainWin;
        private readonly RichTextBox RTB_Main;
        private string assname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public Win_Options(MainWindow mainwin)
        {
            InitializeComponent();
            Owner = mainwin;
            MainWin = mainwin;
            RTB_Main = MainWin.RTB_Main;
            App.OptionsWindow = this;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            App.OptionsWindow = null;
        }

        private void CB_AutoStart_Click(object sender, RoutedEventArgs e)
        {
            var run = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (CB_AutoStart.IsChecked == true)
                run.SetValue(assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
            else
                run.DeleteValue(assname, false);
        }

        private void CB_AutoDock_Click(object sender, RoutedEventArgs e)
        {
            if (CB_AutoDock.IsChecked == true)
            {
                MainWin.CurrentSetting.AutoDock = true;
                MainWin.DockToSide(true);
            }
            else
            {
                MainWin.CurrentSetting.AutoDock = false;
                MainWin.UnDock();
                MainWin.Topmost = false;
            }
        }

        private void Button_ResetFormats_Click(object sender, RoutedEventArgs e)
        {
            //clear custom formats
            var tr = new TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd);
            tr.ClearAllProperties();

            //reset global font size
            RTB_Main.FontSize = MainWin.FontSize;

            //resetting paper color should be processed with Reset Settings
            //var cp = (Color)ColorConverter.ConvertFromString((string)MainWin.CurrentSetting.Properties["PaperColor"].DefaultValue);
            //MainWin.CurrentSetting.PaperColor = cp;
            //Rec_BG.Fill = new SolidColorBrush(cp);
        }

        private async void Button_ResetSet_Click(object sender, RoutedEventArgs e)
        {
            //create default setting with current note path
            await FadeOut(true);
            var newSetting = new Setting(Setting.NoteFlag.Existing | Setting.NoteFlag.IgnoreSettingsFromFile,
                path: MainWin.CurrentSetting.Doc_Location);
            MainWin.Close();
            new MainWindow(newSetting).Show();
            //var optwin = new Win_Options(win);
            //optwin.Show();
        }

        private void Button_About_Click(object sender, RoutedEventArgs e)
        {
            Close();
            System.Threading.Tasks.Task.Run(delegate
            {
                if (Helpers.MsgBox(body: string.Format((string)App.Res["msgbox_about"], System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()),
                                   button: MessageBoxButton.OKCancel,
                                   image: MessageBoxImage.Information) == MessageBoxResult.OK)
                    System.Diagnostics.Process.Start("https://github.com/changbowen/DesktopNote");
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //check auto dock
            if (MainWin.CurrentSetting.AutoDock == true) CB_AutoDock.IsChecked = true;

            //check auto start
            var run = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            string run_value = (string)run.GetValue(assname);

            if (!string.IsNullOrEmpty(run_value))
            {//update the exe location if Run contains assname.
                CB_AutoStart.IsChecked = true;
                if (run_value != System.Reflection.Assembly.GetExecutingAssembly().Location)
                    run.SetValue(assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
            }

            //set path
            TB_SavePath.Text = MainWin.CurrentSetting.Doc_Location;
            TB_SavePathTxt.Text = MainWin.CurrentSetting.Bak_Location;

        }

        private void TB_SavePath_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = Helpers.OpenFileDialog(this, true, MainWin.CurrentSetting.Doc_Location);
            if (path == null) return;

            MainWin.CurrentSetting.Doc_Location = path;
            TB_SavePath.Text = path;
            TB_SavePathTxt.Text = MainWin.CurrentSetting.Bak_Location;
        }

        //private void TB_SavePathTxt_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    var path = Helpers.OpenFileDialog(this, true, MainWin.CurrentSetting.Bak_Location, "DesktopNote Text Content|*.txt");
        //    if (path == null) return;

        //    MainWin.CurrentSetting.Bak_Location = path;
        //    TB_SavePathTxt.Text = path;
        //}
    }
}
