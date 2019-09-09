using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace Panacea.Modules.SipAndPuff
{

    internal class SipAndPuffManager : INotifyPropertyChanged
    {
        int _step = 1;
        int _max = 8;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        System.Timers.Timer _mouseTimer = new System.Timers.Timer();
        System.Timers.Timer _scrollTimer = new System.Timers.Timer();
        private const Keys KEY_MOVE = Keys.Up;
        private const Keys KEY_SELECT = Keys.Down;
        private static readonly object _lock = new object();
        //private readonly KeyboardHelper _keyboardHelper = new KeyboardHelper(KEY_MOVE, KEY_SELECT, 500);
        //private readonly AutomationHelper _automationHelper = new AutomationHelper();
        private OverlayWindow _overlay = new OverlayWindow();

        Tree<InterfaceCommand> _selectedCommand;
        public Tree<InterfaceCommand> SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                _selectedCommand = value;

                OnPropertyChanged();
            }
        }

        Tree<InterfaceCommand> _selectedItem;
        public Tree<InterfaceCommand> SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                _overlay.Commands = value;
                OnPropertyChanged();
            }
        }

        ISipAndPuffInput _sharpDx;
        public SipAndPuffManager()
        {
            _mouseTimer.Elapsed += _mouseTimer_Elapsed;
            _mouseTimer.Interval = 30;

            _scrollTimer.Elapsed += _scrollTimer_Elapsed;
            _scrollTimer.Interval = 300;
            _sharpDx = new SharpDxHelper();
            _sharpDx.PuffDown += _sharpDx_PuffDown;
            _sharpDx.PuffUp += _sharpDx_PuffUp;
            _sharpDx.SipDown += _sharpDx_SipDown;
            _sharpDx.SipUp += _sharpDx_SipUp;

            //_automationHelper.ElementTreeChanged += _automationHelper_ElementTreeChanged;
            //_automationHelper.FocusedWindowChanged += _automationHelper_FocusedWindowChanged;

        }

        private void _scrollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)(dx > 0 ? -120 : 120), 0);
        }

        int _elapsed;

        const double power = 1.6;
        private void _mouseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _mouseTimer.Stop();
            var p = Cursor.Position;

            mouse_event(MOUSEEVENTF_MOVE, x, y, 0, UIntPtr.Zero);
            //Cursor.Position = new System.Drawing.Point(p.X + x, p.Y + y);
            if (_stopwatch.ElapsedMilliseconds > 800)
            {
                _elapsed++;
                var first = (double)_elapsed / 10.0;
                //_stopwatch.Reset();
                if (x != 0)
                {
                    if (x > 0)
                        x = (int)Math.Ceiling(Math.Pow(first, power));
                    else
                        x = -(int)Math.Ceiling(Math.Pow(first, power)); ;

                }
                if (y != 0)
                {
                    if (y > 0)
                        y = (int)Math.Ceiling(Math.Pow(first, power));
                    else
                        y = -(int)Math.Ceiling(Math.Pow(first, power)); ;
                }
                //_stopwatch.Start();
            }
            _mouseTimer.Start();
        }

        private void _sharpDx_SipUp(object sender, EventArgs e)
        {
            _hints.SipUp();
            _sipDown = false;
            _mouseTimer.Stop();
            _scrollTimer.Stop();
        }

        CancellationTokenSource _sipCts, _puffCts;
        private async void _sharpDx_SipDown(object sender, EventArgs e)
        {

            ShowCursor();

            _sipDown = true;
            _sipDownCount++;
            _hints.SipDown();

            _sipCts?.Cancel();
            var cts = new CancellationTokenSource();
            _sipCts = cts;
            if (_sipDownCount == 1)
            {
                _hints.SetIcon("arrow_back");
            }
            else if (_sipDownCount == 2)
            {
                _hints.SetIcon("arrow_upward");
            }
            else if (_sipDownCount == 3)
            {
                _hints.SetIcon("vertical_align_top");
            }
            if (_sipDownCount < 3)
            {
                await Task.Delay(1000);
            }
            if (cts.IsCancellationRequested) return;


            else if (_sipDownCount == 1 && _sipDown)
            {
                _hints.SetIcon("arrow_back");
                x = -_step;
                y = 0;
                _elapsed = 0;
                _stopwatch.Reset();
                _stopwatch.Start();
                _mouseTimer.Start();
            }
            else if (_sipDownCount == 2 && _sipDown)
            {
                _hints.SetIcon("arrow_upward");
                x = 0;
                y = -_step;
                _elapsed = 0;
                _stopwatch.Reset();
                _stopwatch.Start();
                _mouseTimer.Start();
            }
            else if (_sipDown)
            {
                _hints.SetIcon("vertical_align_top");
                dx = -1;
                _scrollTimer.Start();
            }
            _sipDownCount = 0;
        }

        bool _puffDown = false, _sipDown = false;
        private void _sharpDx_PuffUp(object sender, EventArgs e)
        {
            _hints.PuffUp();
            _puffDown = false;
            _mouseTimer.Stop();
            _scrollTimer.Stop();
        }

        int _puffDownCount, _sipDownCount;
        int x, y, dx, dy;
        private async void _sharpDx_PuffDown(object sender, EventArgs e)
        {
            ShowCursor();

            _puffCts?.Cancel();
            var cts = new CancellationTokenSource();
            _puffCts = cts;
            _puffDown = true;
            _puffDownCount++;
            _hints.PuffDown();
            if (_puffDownCount == 1)
            {
                _hints.SetIcon("mouse");
            }
            else if (_puffDownCount == 2)
            {
                _hints.SetIcon("arrow_downward");
            }
            else if (_puffDownCount == 3)
            {
                _hints.SetIcon("vertical_align_bottom");
            }
            if (_puffDownCount < 3)
            {
                await Task.Delay(1000);
            }
            if (cts.IsCancellationRequested) return;
            if (_puffDownCount == 1 && !_puffDown)
            {
                DoMouseClick();
                _puffDownCount = 0;
            }
            else if (_puffDownCount == 1 && _puffDown)
            {
                _hints.SetIcon("arrow_forward");
                x = _step;
                y = 0;
                _elapsed = 0;
                _stopwatch.Reset();
                _stopwatch.Start();
                _mouseTimer.Start();
            }
            else if (_puffDownCount == 2 && _puffDown)
            {
                _hints.SetIcon("arrow_downward");
                x = 0;
                y = _step;
                _elapsed = 0;
                _stopwatch.Reset();
                _stopwatch.Start();
                _mouseTimer.Start();
            }
            else if (_puffDown)
            {
                dx = 1;
                _hints.SetIcon("vertical_align_bottom");
                _scrollTimer.Start();
            }
            _puffDownCount = 0;

        }

        HintWindow _hints;
        public void Start()
        {
            ScreenReader.Activate();
            Debug.WriteLine(ScreenReader.IsScreenReaderRunning());
            //_automationHelper.Start();
            _sharpDx.Start();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _hints = new HintWindow();

            });

            //_keyboardHelper.Start();
        }

        public void Stop()
        {
            _hints.Close();
            ScreenReader.Deactivate();
            Debug.WriteLine(ScreenReader.IsScreenReaderRunning());
            //_automationHelper.Stop();
            //_keyboardHelper.Stop();
            _sharpDx.Stop();
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        [DllImport("user32.dll")]
        static extern int ShowCursor(bool bShow);
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }
        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);
        void ShowCursor()
        {
            mouse_event(MOUSEEVENTF_MOVE, 1, 1, 0, UIntPtr.Zero);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_MOVE, -1, -1, 0, UIntPtr.Zero);
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);

        public void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        private void _automationHelper_FocusedWindowChanged(object sender, AutomationElement e)
        {
            //_currentPath.Clear();
            System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            {
                _overlay.Hide();
                await Task.Delay(100);
                _overlay.CurrentWindow = e;
            });
        }

        private void _automationHelper_ElementTreeChanged(object sender, Tree<AutomationElement> e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var commands = GetCommandsFromTree(e, null);
                if (commands != null)
                {
                    var selected = commands;
                    if (SelectedItem != null)
                    {
                        for (var i = _history.Count - 1; i >= 0; i--)
                        {
                            selected = FindSelected(commands, _history[i]);
                            if (selected == null) _history.RemoveAt(i);
                            else break;
                        }
                        if (selected == null)
                        {
                            selected = commands;
                            _history.Clear();
                        }
                    }
                    else
                    {
                        _history.Clear();
                    }
                    Debug.WriteLine("History Size: " + _history.Count);
                    SelectedCommand = selected?.Parent;
                    SelectedItem = selected;//


                    if (commands != null) _overlay.Show();
                }
            });
        }
        public Tree<InterfaceCommand> FindSelected(Tree<InterfaceCommand> commands, Tree<InterfaceCommand> selected)
        {
            if (commands.Value.Element.GetRuntimeId().SequenceEqual(selected.Value.Element.GetRuntimeId())) return commands;
            if (commands.IsLeaf) return null;
            foreach (var c in commands.Children)
            {
                var f = FindSelected(c, selected);
                if (f != null) return f;
            }
            return null;
        }



        private void Move(int count)
        {
            if (SelectedCommand == null) return;
            var index = SelectedCommand.IndexOfChild(SelectedItem);
            if (_history.Any() && _history.Last() == SelectedItem)
            {
                _history.RemoveAt(_history.Count - 1);
            }

            index += count;
            if (index >= SelectedCommand.Children.Count)
                index = 0;
            if (index < 0)
                index = SelectedCommand.Children.Count - 1;
            if (SelectedCommand.Children.Count > index && index >= 0)
                SelectedItem = SelectedCommand.Children[index];
            _history.Add(SelectedItem);
        }

        public AsyncObservableCollection<InterfaceCommand> Commands { get; set; } = new AsyncObservableCollection<InterfaceCommand>();

        InterfaceCommand GetCommand(AutomationElement element)
        {
            var comm = new InterfaceCommand(element);
            comm.Inspect += Comm_Inspect;
            return comm;
        }

        List<Tree<InterfaceCommand>> _history = new List<Tree<InterfaceCommand>>();

        private void Comm_Inspect(object sender, Tree<InterfaceCommand> e)
        {
            _history.Add(e.Children.FirstOrDefault());
            SelectedCommand = e;
            SelectedItem = e.Children.FirstOrDefault();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        Tree<InterfaceCommand> GetCommandsFromTree(Tree<AutomationElement> tree, Tree<InterfaceCommand> parent)
        {
            if (tree == null) return null;
            var comm = new InterfaceCommand(tree.Value);
            comm.Inspect += Comm_Inspect;
            var ret = new Tree<InterfaceCommand>(comm, parent);
            tree.Children.Select(c => GetCommandsFromTree(c, ret))
                .ToList()
                .ForEach((c) => ret.Add(c));
            return ret;
        }
    }
}
