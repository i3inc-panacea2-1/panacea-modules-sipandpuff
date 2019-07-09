using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Windows.Input;

namespace Panacea.Modules.SipAndPuff
{

    internal class SipAndPuffManager : INotifyPropertyChanged
    {
        private const Keys KEY_MOVE = Keys.Up;
        private const Keys KEY_SELECT = Keys.Down;
        private static readonly object _lock = new object();
        private readonly KeyboardHelper _keyboardHelper = new KeyboardHelper(KEY_MOVE, KEY_SELECT, 500);
        private readonly AutomationHelper _automationHelper = new AutomationHelper();
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

        public SipAndPuffManager()
        {
            _keyboardHelper.Back += _keyboardHelper_Back;
            _keyboardHelper.Next += _keyboardHelper_Next;
            _keyboardHelper.Previous += _keyboardHelper_Previous;
            _keyboardHelper.Select += _keyboardHelper_Select;
           

            _automationHelper.ElementTreeChanged += _automationHelper_ElementTreeChanged;
            _automationHelper.FocusedWindowChanged += _automationHelper_FocusedWindowChanged;
            
        }

        public void Start()
        {
            ScreenReader.Activate();
            Debug.WriteLine(ScreenReader.IsScreenReaderRunning());
            _automationHelper.Start();
            _keyboardHelper.Start();
        }

        public void Stop()
        {
            ScreenReader.Deactivate();
            Debug.WriteLine(ScreenReader.IsScreenReaderRunning());
            _automationHelper.Stop();
            _keyboardHelper.Stop();
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

        private void _keyboardHelper_Back(object sender, EventArgs e)
        {
            var comm = SelectedCommand;
            _history.Remove(comm);
            SelectedCommand = SelectedCommand.Parent ?? SelectedCommand;
            SelectedItem = comm;
        }

        private void _keyboardHelper_Select(object sender, EventArgs e)
        {
            if (SelectedItem == null) return;
            SelectedItem?.Value?.Command?.Execute(SelectedItem);
        }

        private void _keyboardHelper_Previous(object sender, EventArgs e)
        {
            Move(-1);
        }

        private void _keyboardHelper_Next(object sender, EventArgs e)
        {
            Move(1);
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
