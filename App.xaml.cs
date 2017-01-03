using System;
using System.Runtime.InteropServices;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Linq;
//using System.Threading.Tasks;
using System.Windows;

namespace DesktopNote
{
    public partial class App : Application
    {
        public static MainWindow mainwin;
        public static Win_Format fb;
        internal const int MaxWindowCount = 2;//need to set this to 4 while debugging if you use live debug toolbar in vs2015.
        public const string MutexString = @"{39622D35-176E-453D-B1FD-E4EC1EAF31DC}";

        [DllImport("shlwapi.dll")]
        private static extern bool PathIsNetworkPath(string pszPath);

        private void RunCheck(object sender1, StartupEventArgs e1)
        {
            if (SingleInstance.CheckExist(MutexString))
            {asdfasfasdf
                SingleInstance.SendNotifyMessage(SingleInstance.HWND_BROADCAST, SingleInstance.RegisteredMsg, IntPtr.Zero, IntPtr.Zero);
                Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs e) =>
              {
                  var desiredAssembly = new System.Reflection.AssemblyName(e.Name).Name;
                  if (desiredAssembly == "Xceed.Wpf.Toolkit")
                  {
                      var ressourceName = "DesktopNote.Resources." + desiredAssembly + ".dll";
                      using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(ressourceName))
                      {
                          byte[] assemblyData = new byte[stream.Length];
                          stream.Read(assemblyData, 0, assemblyData.Length);
                          return System.Reflection.Assembly.Load(assemblyData);
                      }
                  }
                  else
                      return null;
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
                        var dict = new ResourceDictionary();
                        dict.Source = new Uri(@"Resources\StringResources." + lang + ".xaml", UriKind.Relative);
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

            mainwin = new MainWindow();
            Current.MainWindow = mainwin;
            mainwin.Show();
        }

        
    }
}
