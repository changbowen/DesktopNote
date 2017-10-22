using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
//using System.Configuration;
//using System.Data;
//using System.Linq;
//using System.Threading.Tasks;
using System.Windows;

namespace DesktopNote
{
    public partial class App : Application
    {
        public static List<MainWindow> MainWindows = new List<MainWindow>();
        public static Win_Format FormatWindow;
        public static Win_Search SearchWindow;
        public static Rect CurrScrnRect;
        //internal const int MaxWindowCount = 2;//need to set this to 4 while debugging if you use live debug toolbar in vs2015.
        public const string MutexString = @"{39622D35-176E-453D-B1FD-E4EC1EAF31DC}";
        private System.Threading.Mutex mtx;
        public static Hardcodet.Wpf.TaskbarNotification.TaskbarIcon TrayIcon;

        [DllImport("shlwapi.dll")]
        private static extern bool PathIsNetworkPath(string pszPath);

        private void RunCheck(object sender1, StartupEventArgs e1)
        {
            if (SingleInstance.CheckExist(MutexString, ref mtx))
            {
                SingleInstance.SendNotifyMessage(SingleInstance.HWND_BROADCAST, SingleInstance.RegisteredWM, IntPtr.Zero, IntPtr.Zero);
                Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs e) =>
              {
                  var desiredAssembly = new System.Reflection.AssemblyName(e.Name).Name;
                  switch (desiredAssembly)
                  {
                      case "Xceed.Wpf.Toolkit":
                      case "Hardcodet.Wpf.TaskbarNotification":
                          var ressourceName = "DesktopNote.Resources." + desiredAssembly + ".dll";
                          using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(ressourceName))
                          {
                              byte[] assemblyData = new byte[stream.Length];
                              stream.Read(assemblyData, 0, assemblyData.Length);
                              return System.Reflection.Assembly.Load(assemblyData);
                          }
                      default:
                          return null;
                  }
              };

            //localization
            var lang = System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(0, 2);
            //check if stringresources.lang exist
            bool langadded = false;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new System.Resources.ResourceManager(resourceName, assembly);
            try
            {
                var resourceSet = resourceManager.GetResourceSet(System.Threading.Thread.CurrentThread.CurrentCulture, true, true);
                foreach (System.Collections.DictionaryEntry resource in resourceSet)
                {
                    if ((string)resource.Key == @"resources/stringresources." + lang + ".baml")
                    {
                        var dict = new ResourceDictionary
                        {
                            Source = new Uri(@"Resources\StringResources." + lang + ".xaml", UriKind.Relative)
                        };
                        Resources.MergedDictionaries.Add(dict);
                        langadded = true;
                        break;
                    }
                }
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
            //set english as fallback language
            if (!langadded)
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"Resources\StringResources.en.xaml", UriKind.Relative) });

            //other run checks
            if (PathIsNetworkPath(AppDomain.CurrentDomain.BaseDirectory))
            {
                MessageBox.Show((string)Resources["msgbox_run_from_network"], "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Current.Shutdown();
                return;
            }

            //remove deleted notes from settings
            var set = DesktopNote.Properties.Settings.Default;
            for (int i = set.Doc_Location.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(set.Doc_Location[i]))
                    foreach (System.Configuration.SettingsPropertyValue propval in set.PropertyValues)
                    {
                        if (propval.Property.PropertyType == typeof(StringCollection))
                            ((StringCollection)propval.PropertyValue).RemoveAt(i);
                    }
            }
            set.Save();

            //load notes
            for (int i = 0; i < set.Doc_Location.Count; i++)
            {
                var newmainwin = new MainWindow(i);
                MainWindows.Add(newmainwin);
                //Current.MainWindow = mainwin;
                newmainwin.Show();
            }

            if (MainWindows.Count == 0) Win_Format.NewNote();
        }

        public static void Quit(bool savesetting)
        {
            foreach (var win in MainWindows)
            {
                if (win == null) continue;
                win.SaveToXamlPkg();
                if (savesetting)
                {
                    win.CurrentSetting.Win_Pos = new Point(win.Left, win.Top);
                    win.CurrentSetting.Win_Size = new Size(win.Width, win.Height);
                    win.CurrentSetting.DockedTo = (int)win.lastdockstatus;
                }
            }
            DesktopNote.Properties.Settings.Default.Save();
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            mtx?.Close();
        }
    }
}
