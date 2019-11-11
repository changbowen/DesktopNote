using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace DesktopNote
{
    public class Setting : INotifyPropertyChanged
    {
        internal MainWindow MainWin;
        internal const string SettingBeginMark = "-----SETTINGS_BEGIN-----";
        internal const string ContentBeginMark = "-----CONTENTS_BEGIN-----";
        internal const string SettingEndMark = "-----SETTINGS_END-----";
        internal const string ContentEndMark = "-----CONTENTS_END-----";
        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        public event PropertyChangedEventHandler PropertyChanged;

        internal readonly NoteFlag Flags;
        [Flags]
        internal enum NoteFlag
        {
            CreateNew,
            Existing,
            IgnoreSettingsFromFile,
        }


        //mapping to app settings for convenience
        internal static StringCollection NoteList => Properties.Settings.Default.NoteList;
        internal static bool UpgradeFlag
        {
            get => Properties.Settings.Default.UpgradeFlag;
            set => Properties.Settings.Default.UpgradeFlag = value;
        }


        #region Per-note setting properties
        private string doc_location = App.AppRootDir + "DesktopNoteContent";
        internal string Doc_Location
        {
            get => doc_location;
            set {
                if (string.IsNullOrWhiteSpace(Path.GetFileName(value))) return;
                if (doc_location == value) return;
                //convert relative path to absolute path and check again
                value = Path.GetFullPath(value);
                if (doc_location == value) return;
                if (MainWin != null && MainWin.IsLoaded) {
                    if (File.Exists(doc_location) && !File.Exists(value) &&
                        File.Exists(doc_location + ".txt") && !File.Exists(value + ".txt")) {
                        File.Move(doc_location, value);
                        File.Move(doc_location + ".txt", value + ".txt");
                        doc_location = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Doc_Location)));
                    }
                }
                else {
                    doc_location = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Doc_Location)));
                }
            }
        }
        internal string Bak_Location => Doc_Location + ".txt";
        internal string Doc_FileName => Path.GetFileName(Doc_Location);


        private Size win_size;
        [NoteSetting]
        internal Size Win_Size
        {
            get => win_size;
            set {
                if (win_size == value) return;
                win_size = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Win_Size)));
            }
        }

        private Point win_pos;
        [NoteSetting]
        internal Point Win_Pos
        {
            get => win_pos;
            set {
                if (win_pos == value) return;
                win_pos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Win_Pos)));
            }
        }


        private bool autodock;
        [NoteSetting]
        internal bool AutoDock
        {
            get => autodock;
            set {
                if (autodock == value) return;
                autodock = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoDock)));
            }
        }


        private int dockedto;
        [NoteSetting]
        internal int DockedTo
        {
            get => dockedto;
            set {
                if (dockedto == value) return;
                dockedto = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DockedTo)));
            }
        }


        private string font;
        [NoteSetting]
        internal string Font
        {
            get => font;
            set {
                if (font == value) return;
                font = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Font)));
            }
        }


        private Color fontcolor;
        [NoteSetting]
        internal Color FontColor
        {
            get => fontcolor;
            set {
                if (fontcolor == value) return;
                fontcolor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontColor)));
            }
        }


        private Color backcolor;
        [NoteSetting]
        internal Color BackColor
        {
            get => backcolor;
            set {
                if (backcolor == value) return;
                backcolor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackColor)));
            }
        }


        private Color papercolor;
        [NoteSetting]
        internal Color PaperColor
        {
            get => papercolor;
            set {
                if (papercolor == value) return;
                papercolor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PaperColor)));
            }
        }
        #endregion

        /// <summary>
        /// Create new instance of Setting. If refSetting is null, default values will be used. Otherwise values from refSetting will be used.
        /// Specify path to override default Doc_Location which is DesktopNoteContent_CurrentDateTime at application root.
        /// </summary>
        internal Setting(NoteFlag flags, Setting refSetting = null, string path = null)
        {
            Flags = flags;
            Doc_Location = path ?? $"{App.AppRootDir}DesktopNoteContent_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            if (refSetting != null) {
                Win_Size = refSetting.Win_Size;
                Win_Pos = refSetting.Win_Pos;
                AutoDock = refSetting.AutoDock;
                DockedTo = refSetting.DockedTo;
                Font = refSetting.Font;
                FontColor = refSetting.FontColor;
                BackColor = refSetting.BackColor;
                PaperColor = refSetting.PaperColor;
            }
            else Reset();
        }

        internal static void Upgrade()
        {
            if (UpgradeFlag) {
                Properties.Settings.Default.Upgrade();
                UpgradeFlag = false;
                Properties.Settings.Default.Save();
            }
        }

        //Shortcut to Properties.Settings.Default.Save().
        internal static void Save()
        {
            Properties.Settings.Default.Save();
        }

        internal void Reset()
        {
            Win_Size = new Size(300, 350);
            Win_Pos = new Point(0, 0);
            AutoDock = true;
            DockedTo = 0;
            Font = "Segoe Print";
            FontColor = (Color)ColorConverter.ConvertFromString("#FF000000");
            BackColor = (Color)ColorConverter.ConvertFromString("#00FFFFFF");
            PaperColor = (Color)ColorConverter.ConvertFromString("#FFFFF7C5");
        }

        internal string Serialize()
        {
            var root = new XElement("Setting");
            foreach (var info in typeof(Setting).GetProperties(flags).Where(p => Attribute.IsDefined(p, typeof(NoteSettingAttribute)))) {
                var ele = new XElement(info.Name, info.GetValue(this).ToString());
                root.Add(ele);
            }
            return root.ToString(SaveOptions.DisableFormatting);
        }

        internal void Parse(string content)
        {
            var root = XElement.Parse(content);
            foreach (var ele in root.Elements()) {
                var info = typeof(Setting).GetProperty(ele.Name.LocalName, flags);
                if (info == null) continue;
                switch (ele.Name.LocalName) {
                    case nameof(Win_Size):
                        info.SetValue(this, Size.Parse(ele.Value));
                        break;
                    case nameof(Win_Pos):
                        info.SetValue(this, Point.Parse(ele.Value));
                        break;
                    case nameof(AutoDock):
                        info.SetValue(this, bool.Parse(ele.Value));
                        break;
                    case nameof(DockedTo):
                        info.SetValue(this, int.Parse(ele.Value));
                        break;
                    case nameof(FontColor):
                    case nameof(BackColor):
                    case nameof(PaperColor):
                        info.SetValue(this, ColorConverter.ConvertFromString(ele.Value));
                        break;
                    case nameof(Font):
                        info.SetValue(this, ele.Value);
                        break;
                }
            }
        }

        //      /// <summary>
        //      /// Leaving win to null will not save per-note settings.
        //      /// Returns status message from SaveNote if win is passed. Otherwise null.
        //      /// </summary>
        //public static string Save(MainWindow win = null)
        //      {
        //          ////update note list (moved to Window.Closing event)
        //          //Properties.Settings.Default.NoteList.Clear();
        //          //Properties.Settings.Default.NoteList.AddRange(App.MainWindows.Select(w => w.CurrentSetting.Doc_Location).ToArray());

        //          //save settings
        //          Properties.Settings.Default.Save();
        //          //save per-note settings
        //          if (win != null) return Helpers.SaveNote(win);
        //          return null;
        //      }

        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
        sealed class NoteSettingAttribute : Attribute
        {
           
        }
    }
}
