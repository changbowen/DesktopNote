using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows;

namespace DesktopNote
{
    public partial class App : Application
    {
        //Private Declare Unicode Function PathIsNetworkPath Lib "shlwapi" Alias "" (ByVal pszPath As String) As Boolean
        [System.Runtime.InteropServices.DllImport("shlwapi.dll")]
        private static extern bool PathIsNetworkPathW(string pszPath);

        private void RunCheck(object sender1, StartupEventArgs e1)
        {
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
            if (PathIsNetworkPathW(System.AppDomain.CurrentDomain.BaseDirectory))
            {
                MessageBox.Show((string)Resources["msgbox_run_from_network"], "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Current.Shutdown();
                return;
            }

            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show((string)Resources["msgbox_one_inst"], "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Current.Shutdown();
                return;
            }

            var mainwin = new MainWindow();
            mainwin.Show();
        }
    }
}
