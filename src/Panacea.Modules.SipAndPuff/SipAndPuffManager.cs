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
            _mouseTimer.Interval = 18;

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

        private void _mouseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var p = Cursor.Position;
            Cursor.Position = new System.Drawing.Point(p.X + x, p.Y + y);
            if (_stopwatch.ElapsedMilliseconds > 1000)
            {
                _stopwatch.Reset();
                if (x != 0 && Math.Abs(x) < _max)
                {
                    if (x > 0)
                        x++;
                    else
                        x--;

                }
                if (y != 0 && Math.Abs(y) < _max)
                {
                    if (y > 0)
                        y++;
                    else
                        y--;
                }
                _stopwatch.Start();
            }
        }

        private void _sharpDx_SipUp(object sender, EventArgs e)
        {
            _sipDown = false;
            _mouseTimer.Stop();
            _scrollTimer.Stop();
        }

        private async void _sharpDx_SipDown(object sender, EventArgs e)
        {
            _sipDown = true;
            _sipDownCount++;
            var count = _sipDownCount;

            await Task.Delay(1000);

            if (count == _sipDownCount)
            {
                if (count == 1 && !_sipDown)
                {
                    DoMouseClick();
                    _puffDownCount = 0;
                }
                else if (_sipDownCount == 1 && _sipDown)
                {
                    x = -_step;
                    y = 0;
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    _mouseTimer.Start();
                }
                else if (_sipDownCount == 2 && _sipDown)
                {
                    x = 0;
                    y = -_step;
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    _mouseTimer.Start();
                }
                else if (_sipDown)
                {
                    dx = -1;
                    _scrollTimer.Start();
                }
                _sipDownCount = 0;
            }
        }

        bool _puffDown = false, _sipDown = false;
        private void _sharpDx_PuffUp(object sender, EventArgs e)
        {
            _puffDown = false;
            _mouseTimer.Stop();
            _scrollTimer.Stop();
        }

        int _puffDownCount, _sipDownCount;
        int x, y, dx, dy;
        private async void _sharpDx_PuffDown(object sender, EventArgs e)
        {
            _puffDown = true;
            _puffDownCount++;
            var count = _puffDownCount;

            await Task.Delay(1000);

            if (count == _puffDownCount)
            {
                if (count == 1 && !_puffDown)
                {
                    DoMouseClick();
                    _puffDownCount = 0;
                }
                else if (_puffDownCount == 1 && _puffDown)
                {
                    x = _step;
                    y = 0;
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    _mouseTimer.Start();
                }
                else if (_puffDownCount == 2 && _puffDown)
                {
                    x = 0;
                    y = _step;
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    _mouseTimer.Start();
                }
                else if (_puffDown)
                {
                    dx = 1;
                    _scrollTimer.Start();
                }
                _puffDownCount = 0;
            }
        }

        public void Start()
        {
            ScreenReader.Activate();
            Debug.WriteLine(ScreenReader.IsScreenReaderRunning());
            //_automationHelper.Start();
            _sharpDx.Start();

            //_keyboardHelper.Start();
        }

        public void Stop()
        {
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
