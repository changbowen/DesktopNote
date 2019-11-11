using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Linq;

namespace DesktopNote
{
    internal static class Helpers
    {
        internal static string OpenFileDialog(Window owner, bool save, string path = null, string filter = null)
        {
            if (owner == null || PresentationSource.FromVisual(owner) == null)
                throw new Exception("ShowDialog needs a loaded window to attach itself.");

            var dlg = new OpenFileDialog {
                Title = save ? (string)App.Res["menu_savenote"] : (string)App.Res["menu_opennote"],
                CheckFileExists = save ? false : true,
                CheckPathExists = save ? false : true,
            };
            dlg.Filter = filter ?? @"DesktopNote Content|*";
            if (!string.IsNullOrEmpty(path)) dlg.InitialDirectory = Path.GetDirectoryName(path);

            if (dlg.ShowDialog(owner) == true) return dlg.FileName;
            return null;
        }

        /// <summary>
        /// Wrapper around MessageBox.Show() with default app name as title. If resourceKey is passed, body will be ignored. Vice versa.
        /// </summary>
        internal static MessageBoxResult MsgBox(string resourceKey = null, string body = null, string title = null,
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information)
        {
            if (title == null) title = (string)App.Res["desktopnote"];
            if (!string.IsNullOrWhiteSpace(resourceKey))
                return MessageBox.Show((string)App.Res[resourceKey], title, button, image);
            else
                return MessageBox.Show(body, title, button, image);
        }


        internal static MainWindow NewNote(Setting refSetting = null)
        {
            var win = new MainWindow(new Setting(Setting.NoteFlag.CreateNew, refSetting));
            win.Show();
            win.Top += 20d; win.Left += 20d;
            SaveNote(win);
            return win;
        }

        /// <summary>
        /// Perform checks before instantiating MainWindow.
        /// When path file exists, note will be loaded in a new window. If refSetting is passed, settings in note file will be ignored.
        /// When path file does not exist, a new note window will be created with the path specified.
        /// Returns null or MainWindow.
        /// </summary>
        internal static MainWindow OpenNote(string path, Setting refSetting = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            //open if exists. otherwise create
            if (File.Exists(path)) {
                //check if note is already loaded in another window
                var win = App.MainWindows.Find(w => w.CurrentSetting.Doc_Location == path);
                if (win != null) {
                    MsgBox("msgbox_file_already_opened", button: MessageBoxButton.OK, image: MessageBoxImage.Information);
                    win.UnDock(); win.Activate(); win.RTB_Main.Focus();
                    return null;
                }
                return new MainWindow(new Setting(Setting.NoteFlag.Existing, refSetting, path));
            }
            else
                return new MainWindow(new Setting(Setting.NoteFlag.CreateNew, refSetting, path));
        }

        internal static bool LoadNote(MainWindow win)
        {
            FileStream fs = null;
            try {
                Monitor.Enter(win.lock_save);
                fs = new FileStream(win.CurrentSetting.Doc_Location, FileMode.Open);

                //load setting from file when CurrentSetting is null
                if (!win.CurrentSetting.Flags.HasFlag(Setting.NoteFlag.IgnoreSettingsFromFile)) {
                    using (var sr = new StreamReader(fs, Encoding.UTF8, false, 1024, true)) {
                        string settingStr = null;
                        while (!sr.EndOfStream) {
                            var line = sr.ReadLine();
                            if (line == Setting.SettingBeginMark) {
                                settingStr = sr.ReadLine();
                            }
                            sr.ReadLine();
                            break;
                        }
                        if (settingStr != null) win.CurrentSetting.Parse(settingStr);
                    }
                }

                //load contents from file
                fs.Position = 0;
                long startPos = -1;
                //try get content start position
                using (var sr = new StreamReader(fs, Encoding.UTF8, false, 1024, true)) {
                    while (!sr.EndOfStream) {
                        var line = sr.ReadLine();
                        if (line == Setting.ContentBeginMark) { startPos = sr.GetPosition(); break; }
                    }
                }

                var tr = new TextRange(win.RTB_Main.Document.ContentStart, win.RTB_Main.Document.ContentEnd);
                if (startPos == -1) {
                    //try reading entire file as XamlPackage for backward compatibility
                    tr.Load(fs, DataFormats.XamlPackage);
                }
                else {
                    fs.Position = startPos;
                    using (var ms = new MemoryStream()) {
                        fs.CopyTo(ms);
                        tr.Load(ms, DataFormats.XamlPackage);
                    }
                }

                return true;
            }
            catch (Exception ex) {
                MsgBox(body: string.Format((string)App.Res["msgbox_load_error"], win.CurrentSetting.Doc_Location, win.CurrentSetting.Bak_Location) + "\r\n\r\n" + ex.Message,
                    title: (string)App.Res["msgbox_title_load_error"], button: MessageBoxButton.OK, image: MessageBoxImage.Stop);
                return false;
            }
            finally {
                fs?.Close();
                fs?.Dispose();
                Monitor.Exit(win.lock_save);
            }
        }

        /// <summary>
        /// Write per-note settings and note content to file. And save app settings.
        /// Returns null if succeeded. Otherwise the error message.
        /// </summary>
        internal static string SaveNote(MainWindow win)
        {
            string statusText, exMsg = null;
            try {
                Monitor.Enter(win.lock_save);
                var tr = new TextRange(win.RTB_Main.Document.ContentStart, win.RTB_Main.Document.ContentEnd);
                using (var fs = new FileStream(win.CurrentSetting.Doc_Location, FileMode.Create)) {
                    // write settings to file
                    var settingStr = win.CurrentSetting.Serialize().Trim();
                    var data = Encoding.UTF8.GetBytes(Setting.SettingBeginMark + "\r\n" + settingStr + "\r\n" + Setting.SettingEndMark + "\r\n");
                    fs.Write(data, 0, data.Length);

                    // write contents to file
                    data = Encoding.UTF8.GetBytes(Setting.ContentBeginMark + "\r\n");
                    fs.Write(data, 0, data.Length);
                    using (var ms = new MemoryStream()) {
                        tr.Save(ms, DataFormats.XamlPackage, true);
                        ms.WriteTo(fs);
                    }
                    fs.Flush();
                }

                //write text content to file
                File.WriteAllText(win.CurrentSetting.Bak_Location, tr.Text);
                statusText = (string)App.Res["status_saved"];

                //save app settings
                Setting.Save();
            }
            catch (Exception ex) {
                statusText = (string)App.Res["status_save_failed"];
                exMsg = ex.ToString();
            }
            finally {
                Monitor.Exit(win.lock_save);
            }

            win.TB_Status.Text = statusText;
            win.TB_Status.Visibility = Visibility.Visible;
            return exMsg;
        }


    }

    public static class StreamReaderExtensions
    {
        readonly static FieldInfo charPosField = typeof(StreamReader).GetField("charPos", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo byteLenField = typeof(StreamReader).GetField("byteLen", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        readonly static FieldInfo charBufferField = typeof(StreamReader).GetField("charBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        public static long GetPosition(this StreamReader reader)
        {
            //shift position back from BaseStream.Position by the number of bytes read
            //into internal buffer.
            int byteLen = (int)byteLenField.GetValue(reader);
            var position = reader.BaseStream.Position - byteLen;

            //if we have consumed chars from the buffer we need to calculate how many
            //bytes they represent in the current encoding and add that to the position.
            int charPos = (int)charPosField.GetValue(reader);
            if (charPos > 0) {
                var charBuffer = (char[])charBufferField.GetValue(reader);
                var encoding = reader.CurrentEncoding;
                var bytesConsumed = encoding.GetBytes(charBuffer, 0, charPos).Length;
                position += bytesConsumed;
            }

            return position;
        }

        public static void SetPosition(this StreamReader reader, long position)
        {
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }
    }
}
