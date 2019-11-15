using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopNote
{
    public partial class Win_Reminder : RoundedWindow
    {
        private MainWindow MainWin;

        public Reminder Reminder
        {
            get { return (Reminder)GetValue(ReminderProperty); }
            set { SetValue(ReminderProperty, value); }
        }
        public static readonly DependencyProperty ReminderProperty =
            DependencyProperty.Register("Reminder", typeof(Reminder), typeof(Win_Reminder), new PropertyMetadata(null));


        public Win_Reminder(MainWindow mainwin, Reminder rmd)
        {
            MainWin = mainwin;
            Reminder = rmd;
            InitializeComponent();
        }
    }
}
