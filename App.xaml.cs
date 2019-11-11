using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using System.Configuration;
//using System.Data;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows;

namespace DesktopNote
{
    public partial class App : Application
    {
        public static readonly string AppRootDir = AppDomain.CurrentDomain.BaseDirectory;
        public static List<MainWindow> MainWindows = new List<MainWindow>();
        public static ResourceDictionary Res;
        public static Win_Format FormatWindow;
        public static Win_Search SearchWindow;
        public static Win_Options OptionsWindow;
        public static Rect CurrScrnRect;
        //internal const int MaxWindowCount = 2;//need to set this to 4 while debugging if you use live debug toolbar in vs2015.
        public const string MutexString = @"{39622D35-176E-453D-B1FD-E4EC1EAF31DC}";
        private System.Threading.Mutex mtx;
        public static Hardcodet.Wpf.TaskbarNotification.TaskbarIcon TrayIcon;

        [DllImport("shlwapi.dll")]
        private static extern bool PathIsNetworkPath(string pszPath);

        private void RunCheck(object sender1, StartupEventArgs e1)
        {
            //check if app is already running
            if (SingleInstance.CheckExist(MutexString, ref mtx)) {
                SingleInstance.SendNotifyMessage(SingleInstance.HWND_BROADCAST, SingleInstance.RegisteredWM, IntPtr.Zero, IntPtr.Zero);
                Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs e) => {
                var desiredAssembly = new System.Reflection.AssemblyName(e.Name).Name;
                switch (desiredAssembly) {
                    case "Xceed.Wpf.Toolkit":
                    case "Hardcodet.Wpf.TaskbarNotification":
                        var ressourceName = "DesktopNote.Resources." + desiredAssembly + ".dll";
                        using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(ressourceName)) {
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
            try {
                var resourceSet = resourceManager.GetResourceSet(System.Threading.Thread.CurrentThread.CurrentCulture, true, true);
                foreach (System.Collections.DictionaryEntry resource in resourceSet) {
                    if ((string)resource.Key == @"resources/stringresources." + lang + ".baml") {
                        var dict = new ResourceDictionary {
                            Source = new Uri(@"Resources\StringResources." + lang + ".xaml", UriKind.Relative)
                        };
                        Resources.MergedDictionaries.Add(dict);
                        langadded = true;
                        break;
                    }
                }
            }
            finally {
                resourceManager.ReleaseAllResources();
            }
            //set english as fallback language
            if (!langadded)
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"Resources\StringResources.en.xaml", UriKind.Relative) });

            //other run checks
            if (PathIsNetworkPath(AppDomain.CurrentDomain.BaseDirectory)) {
                Helpers.MsgBox("msgbox_run_from_network", button: MessageBoxButton.OK, image: MessageBoxImage.Exclamation);
                Current.Shutdown();
                return;
            }
            Res = Current.Resources;

            //load notes
            foreach (var path in Setting.NoteList.Cast<string>().ToArray()) {
                Helpers.OpenNote(path)?.Show();
            }

            if (MainWindows.Count == 0) Helpers.NewNote();
        }

        public async static void Quit()
        {
            //update notelist
            Setting.NoteList.Clear();
            Setting.NoteList.AddRange(App.MainWindows.Select(w => w.CurrentSetting.Doc_Location).ToArray());
            Setting.Save();

            foreach (var win in MainWindows.ToArray()) {
                await win.Close();
            }

            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            mtx?.Close();
        }
    }
}
