using Panacea.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
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
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : NonFocusableWindow
    {


        public AutomationElement CurrentWindow
        {
            get { return (AutomationElement)GetValue(CurrentWindowProperty); }
            set { SetValue(CurrentWindowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentWindow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentWindowProperty =
            DependencyProperty.Register("CurrentWindow", typeof(AutomationElement), typeof(OverlayWindow), new PropertyMetadata(null, OnCurrentWindowChanged));

        private static async void OnCurrentWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var o = d as OverlayWindow;
                if (o == null)
                {
                    return;
                }
                var w = e.NewValue as AutomationElement;
                await o.SetWindow(w);
            }
            catch (ElementNotAvailableException)
            {

            }
        }

        async Task SetWindow(AutomationElement w)
        {
            if (w == null)
            {
                Hide();
                return;
            }
            double top = 0, left = 0, width = 0, height = 0;
            await Task.Run(() =>
            {
                top = w.Current.BoundingRectangle.Top - 60;
                left = w.Current.BoundingRectangle.Left - 60;
                width = w.Current.BoundingRectangle.Width + 120;
                height = w.Current.BoundingRectangle.Height + 120;
            });
            Top = top;
            Left = left;
            Width = width;
            Height = height;
        }

        internal Tree<InterfaceCommand> Commands
        {
            get { return (Tree<InterfaceCommand>)GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Commands.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandsProperty =
            DependencyProperty.Register("Commands", typeof(Tree<InterfaceCommand>), typeof(OverlayWindow), new PropertyMetadata(null, OnCommandsChanged));

        private static async void OnCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var w = d as OverlayWindow;
            if (w == null) return;
            w.Root.Children.Clear();
            await w.CreateBorders(w.Commands);
            w.Show();
        }

        private async Task CreateBorders(Tree<InterfaceCommand> command, int depth = 1)
        {
            await SetWindow(CurrentWindow);
            if (depth > 2) return;
            if (command == null) return;
            int borderWidth = 10;
            int margin = 0;
            var b = new Border()
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb((byte)(255 / depth), 64, 58, 252)),
                BorderThickness = new Thickness(borderWidth / depth)
            };
            try
            {
                Rect point = new Rect();
                await Task.Run(() =>
                {
                    point = command.Value.Element.Current.BoundingRectangle;
                });
                b.SetValue(Canvas.LeftProperty, point.X - borderWidth - Left - margin);
                b.SetValue(Canvas.TopProperty, point.Y - borderWidth - Top - margin);

                b.Width = point.Width + 2 * borderWidth + margin * 2;
                b.Height = point.Height + 2 * borderWidth + margin * 2;
                //Console.WriteLine(TreeWalker.ControlViewWalker.GetParent(command.Node.Element).Current.ControlType.ProgrammaticName);
                Root.Children.Add(b);
            }
            catch (ElementNotAvailableException)
            {

            }
            catch (COMException)
            {

            }
            foreach (var c in command.Children)
            {
                CreateBorders(c, depth + 1);
            }
        }

        public OverlayWindow()
        {
            InitializeComponent();
        }
    }
}
