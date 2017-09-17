using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopNote
{
    public class Setting
    {
        public int SettingIndex;
        private Properties.Settings set;

        public string Doc_Location
        {
            get
            {
                return set.Doc_Location[SettingIndex];
            }
            set
            {
                set.Doc_Location[SettingIndex] = value;
            }
        }
        public string Bak_Location
        {
            get
            {
                return set.Bak_Location[SettingIndex];
            }
            set
            {
                set.Bak_Location[SettingIndex] = value;
            }
        }
        public System.Windows.Size Win_Size
        {
            get
            {
                return System.Windows.Size.Parse(set.Win_Size[SettingIndex]);
            }
            set
            {
                set.Win_Size[SettingIndex] = value.ToString();
            }
        }
        public System.Windows.Point Win_Pos
        {
            get
            {
                return System.Windows.Point.Parse(set.Win_Pos[SettingIndex]);
            }
            set
            {
                set.Win_Pos[SettingIndex] = value.ToString();
            }
        }
        public bool AutoDock
        {
            get
            {
                return bool.Parse(set.AutoDock[SettingIndex]);
            }
            set
            {
                set.Bak_Location[SettingIndex] = value.ToString();
            }
        }
        public int DockedTo
        {
            get
            {
                return int.Parse(set.DockedTo[SettingIndex]);
            }
            set
            {
                set.DockedTo[SettingIndex] = value.ToString();
            }
        }
        public string Font
        {
            get
            {
                return set.Font[SettingIndex];
            }
            set
            {
                set.Font[SettingIndex] = value;
            }
        }
        public Color FontColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(set.FontColor[SettingIndex]);
            }
            set
            {
                set.FontColor[SettingIndex] = value.ToString();
            }
        }
        public Color BackColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(set.BackColor[SettingIndex]);
            }
            set
            {
                set.BackColor[SettingIndex] = value.ToString();
            }
        }
        public Color PaperColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(set.PaperColor[SettingIndex]);
            }
            set
            {
                set.PaperColor[SettingIndex] = value.ToString();
            }
        }

        public Setting(int setidx)
        {
            SettingIndex = setidx;
            set = Properties.Settings.Default;
        }

        /// <summary>
        /// For now this is the same as calling Properties.Settings.Default.Save().
        /// </summary>
        public void Save()
        {
            set.Save();
        }

        /// <summary>
        /// For now this is the same as calling Properties.Settings.Default.Reset().
        /// </summary>
        public void Reset()
        {
            set.Reset();
        }
    }
}
