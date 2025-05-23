﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;
using System.Net;
using System.Diagnostics;
using System.Reflection;

namespace DesktopNote
{
    public partial class App : Application
    {
        public static readonly string AppRootDir = AppDomain.CurrentDomain.BaseDirectory;
        public static Assembly Assembly => Assembly.GetExecutingAssembly();
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
                var desiredAssembly = new AssemblyName(e.Name).Name;
                switch (desiredAssembly) {
                    case "Xceed.Wpf.Toolkit":
                    case "Hardcodet.Wpf.TaskbarNotification":
                        var ressourceName = "DesktopNote.Resources." + desiredAssembly + ".dll";
                        using (var stream = Assembly.GetManifestResourceStream(ressourceName)) {
                            byte[] assemblyData = new byte[stream.Length];
                            stream.Read(assemblyData, 0, assemblyData.Length);
                            return Assembly.Load(assemblyData);
                        }
                    default:
                        return null;
                }
            };

            //localization
            var lang = System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(0, 2);
            //check if stringresources.lang exist
            bool langadded = false;
            var resourceName = Assembly.GetName().Name + ".g";
            var resourceManager = new System.Resources.ResourceManager(resourceName, Assembly);
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

            // check for updates
            Task.Run(() => {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var localVer = Assembly.GetName().Version;
                var req = WebRequest.CreateHttp($@"https://api.github.com/repos/changbowen/{nameof(DesktopNote)}/releases/latest");
                req.ContentType = @"application/json; charset=utf-8";
                req.UserAgent = nameof(DesktopNote); // needed otherwise 403
                req.Timeout = 10000;
                using (var res = req.GetResponse())
                using (var stream = res.GetResponseStream())
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8)) {
                    var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(reader.ReadToEnd());
                    if (!dict.TryGetValue("tag_name", out var tagName)) return;
                    var remoteVer = Version.Parse(((string)tagName).TrimStart('v'));
                    if (localVer < remoteVer && MessageBox.Show(string.Format((string)Res["msgbox_new_version_avail"],
                        localVer, remoteVer), string.Empty, MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK) {
                        Process.Start(@"explorer", $@"https://github.com/changbowen/{nameof(DesktopNote)}/releases");
                    }
                }
            });

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
            Setting.NoteList.AddRange(MainWindows.Select(w => w.CurrentSetting.Doc_Location).ToArray());
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
