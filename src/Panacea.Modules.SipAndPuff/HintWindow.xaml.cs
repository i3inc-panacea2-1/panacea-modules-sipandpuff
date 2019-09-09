using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Panacea.Modules.SipAndPuff
{
    /// <summary>
    /// Interaction logic for HintWindow.xaml
    /// </summary>
    public partial class HintWindow : Window
    {
        public HintWindow()
        {
            InitializeComponent();
            Loaded += HintWindow_Loaded;
            sip.Visibility = puff.Visibility = Visibility.Collapsed;
        }

        private void HintWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _events = Hook.GlobalEvents();
            _events.MouseMove += _events_MouseMove;
            var pos = GetMousePosition();
            this.Left = pos.X + 10;
            this.Top = pos.Y - 10;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        IKeyboardMouseEvents _events;

        protected override void OnClosed(EventArgs e)
        {
            _events.MouseMove -= _events_MouseMove;
            _events?.Dispose();
        }


        private void _events_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Left = e.X + 10;
            this.Top = e.Y - 10;
        }

        public void SetIcon(string icon)
        {
            Dispatcher.Invoke(() =>
            {
                puff.Visibility = Visibility.Visible;
                Icon.Icon = icon;
            });
        }

        public void PuffDown()
        {
            Dispatcher.Invoke(() =>
            {
                puff.Visibility = Visibility.Visible;
                Show();
            });

        }

        public void PuffUp()
        {
            Dispatcher.Invoke(() =>
            {
                Icon.Icon = "none";
                puff.Visibility = Visibility.Collapsed;
                Hide();
            });
        }

        public void SipDown()
        {
            Dispatcher.Invoke(() =>
            {
                sip.Visibility = Visibility.Visible;
                Show();
            });
        }

        public void SipUp()
        {
            Dispatcher.Invoke(() =>
            {
                Icon.Icon = "none";
                sip.Visibility = Visibility.Collapsed;
                Hide();
            });
        }
    }
}
