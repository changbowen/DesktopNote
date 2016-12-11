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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopNote
{
    public partial class MainWindow : Window
    {
        private object Lock_Save = new object();
        private static int CountDown = 0;
        string doc_loc = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.Doc_Location;
        string assname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        Point mousepos;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Docking
        public DockStatus lastdockstatus;
        private Rect currScrnRect;
        public enum DockStatus { None, Docking, Left, Right, Top, Bottom }
        public DockStatus DockedTo
        {
            get { return (DockStatus)GetValue(DockedToProperty); }
            set { SetValue(DockedToProperty, value); }
        }

        public static readonly DependencyProperty DockedToProperty =
            DependencyProperty.Register("DockedTo", typeof(DockStatus), typeof(MainWindow), new PropertyMetadata(DockStatus.None));

        private void DockToSide(bool changpos = false)
        {
            if (DockedTo == DockStatus.None)
            {
                double toval;
                DependencyProperty tgtpro;
                double pad = 15d;
                DockStatus dockto;
                if (changpos)
                {
                    currScrnRect = new GetCurrentMonitor().GetInfo();
                    if (Left <= currScrnRect.Left) //dock left
                    {
                        toval = currScrnRect.Left - ActualWidth + pad;
                        tgtpro = LeftProperty;
                        dockto = DockStatus.Left;
                    }

                    else if (Left + ActualWidth >= currScrnRect.Right) //dock right
                    {
                        toval = currScrnRect.Right - pad;
                        tgtpro = LeftProperty;
                        dockto = DockStatus.Right;
                    }

                    else if (Top <= currScrnRect.Top) //dock top
                    {
                        toval = currScrnRect.Top - ActualHeight + pad;
                        tgtpro = TopProperty;
                        dockto = DockStatus.Top;
                    }

                    else if (Top + ActualHeight >= currScrnRect.Bottom) //dock bottom
                    {
                        toval = currScrnRect.Bottom - pad;
                        tgtpro = TopProperty;
                        dockto = DockStatus.Bottom;
                    }
                    else
                    {
                        lastdockstatus = DockStatus.None;
                        Topmost = false;
                        return;
                    }
                    lastdockstatus = dockto;
                }
                else //'restore last docking position
                {
                    dockto = lastdockstatus;
                    switch (lastdockstatus)
                    {
                        case DockStatus.Left:
                            toval = currScrnRect.Left - ActualWidth + pad;
                            tgtpro = LeftProperty;
                            break;
                        case DockStatus.Right:
                            toval = currScrnRect.Right - pad;
                            tgtpro = LeftProperty;
                            break;
                        case DockStatus.Top:
                            toval = currScrnRect.Top - ActualHeight + pad;
                            tgtpro = TopProperty;
                            break;
                        case DockStatus.Bottom:
                            toval = currScrnRect.Bottom - pad;
                            tgtpro = TopProperty;
                            break;
                        default:
                            return;
                    }
                }

                Topmost = true;
                var anim_move = new DoubleAnimation(toval, new Duration(new TimeSpan(0, 0, 0, 0, 500)), FillBehavior.Stop);
                anim_move.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                var anim_fade = new DoubleAnimation(0.4, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                anim_fade.BeginTime = new TimeSpan(0, 0, 0, 0, 200);
                var anim_prop = new ObjectAnimationUsingKeyFrames();
                anim_prop.KeyFrames.Add(new DiscreteObjectKeyFrame(DockStatus.Docking, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                anim_prop.KeyFrames.Add(new DiscreteObjectKeyFrame(dockto, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 500))));
                BeginAnimation(tgtpro, anim_move);
                BeginAnimation(OpacityProperty, anim_fade);
                BeginAnimation(DockedToProperty, anim_prop);
            }
        }

        private void UnDock()
        {
            if (DockedTo != DockStatus.Docking && DockedTo != DockStatus.None)
            {
                double toval;
                DependencyProperty tgtpro;
                //double pad = 15d;
                DockStatus dockto;
                if (DockedTo == DockStatus.Left) //Me.Left = currScrnRect.left - Me.ActualWidth + pad Then
                {
                    toval = currScrnRect.Left;
                    tgtpro = LeftProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Right) //Me.Left = currScrnRect.right - pad Then
                {
                    toval = currScrnRect.Right - ActualWidth;
                    tgtpro = LeftProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Top) //Me.Top = currScrnRect.top - Me.ActualHeight + pad Then
                {
                    toval = currScrnRect.Top;
                    tgtpro = TopProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Bottom) //Me.Top = currScrnRect.bottom - pad Then
                {
                    toval = currScrnRect.Bottom - ActualHeight;
                    tgtpro = TopProperty;
                    dockto = DockStatus.None;
                }
                else
                    return;

                Topmost = true;
                var anim_move = new DoubleAnimation(toval, new Duration(new TimeSpan(0, 0, 0, 0, 300)), FillBehavior.Stop);
                anim_move.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                var anim_fade = new DoubleAnimationUsingKeyFrames();
                anim_fade.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                var anim_prop = new ObjectAnimationUsingKeyFrames();
                anim_prop.KeyFrames.Add(new DiscreteObjectKeyFrame(DockStatus.Docking, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                anim_prop.KeyFrames.Add(new DiscreteObjectKeyFrame(dockto, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 500))));
                BeginAnimation(tgtpro, anim_move);
                BeginAnimation(OpacityProperty, anim_fade);
                BeginAnimation(DockedToProperty, anim_prop);
            }
        }

        private void Win_Main_MouseEnter(object sender, MouseEventArgs e)
        {
            //undocking
            if (Properties.Settings.Default.AutoDock) UnDock();
        }

        private void Win_Main_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Properties.Settings.Default.AutoDock && Application.Current.Windows.Count == 1 && !RTB_Main.IsKeyboardFocused && !RTB_Main.ContextMenu.IsOpen)
                DockToSide();
        }
        #endregion


    }
}
