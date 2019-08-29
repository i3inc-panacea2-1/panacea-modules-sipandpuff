using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.WriteLine(Left + " - " + Top);
        }

        public void PuffDown(int count)
        {
            Dispatcher.Invoke(() =>
            {
                puff.Visibility = Visibility.Visible;
                text.Text = count.ToString();
                text.Visibility = Visibility.Visible;
            });

        }

        public void PuffUp()
        {
            Dispatcher.Invoke(() =>
            {
                puff.Visibility = Visibility.Collapsed;
                text.Visibility = Visibility.Collapsed;
            });
        }

        public void SipDown(int count)
        {
            Dispatcher.Invoke(() =>
            {
                sip.Visibility = Visibility.Visible;
                text.Text = count.ToString();
                text.Visibility = Visibility.Visible;
            });
        }

        public void SipUp()
        {
            Dispatcher.Invoke(() =>
            {
                sip.Visibility = Visibility.Collapsed;
                text.Visibility = Visibility.Collapsed;
            });
        }
    }
}
