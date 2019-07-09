using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace Panacea.Modules.SipAndPuff
{
    class AutomationHelper
    {
        AutomationElement _focusedWindow;
        CancellationTokenSource _cts;
        public event EventHandler<AutomationElement> FocusedWindowChanged;
        public event EventHandler<Tree<AutomationElement>> ElementTreeChanged;

        public void Start()
        {
            Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);
        }

        public void Stop()
        {
            Automation.RemoveAutomationFocusChangedEventHandler(OnFocusChanged);
        }

        Task<Tree<AutomationElement>> GetTreeAsync(AutomationElement element)
        {
            return Task.Run(async () =>
            {
                if (element == null) return null;
                var tree = new Tree<AutomationElement>(element, null);

                var condIsEnabled = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
                var condIsOffscreen = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);
                var cond = new AndCondition(condIsEnabled,
                    condIsOffscreen);
                var scope = TreeScope.Children;
                try
                {
                    var res = element.FindAll(scope, cond);
                    if (res == null) return tree;
                    foreach (AutomationElement item in res)
                    {
                        var tree2 = await GetTreeAsync(item);
                        if (tree2 != null)
                            tree.Add(tree2);
                    }
                }
                catch (ElementNotAvailableException)
                {

                }
                //Console.WriteLine(tree.Print(AutomationElementToString));
                return tree;
            });
        }

        CancellationTokenSource _updateCts;
        async Task Update()
        {
            if (_updateCts != null)
            {
                _updateCts.Cancel();
            }
            _updateCts = new CancellationTokenSource();
            //ElementTreeChanged?.Invoke(this, null);
            var cts = _updateCts;
            Tree<AutomationElement> tree = null;
            await Task.Run(async() =>
            {
                tree = (await GetTreeAsync(_focusedWindow)).Reduce((element) =>
                {
                    try
                    {
                        return element.Value.Current.ControlType == ControlType.Button
                        || element.Value.Current.ControlType == ControlType.ListItem
                        || element.IsNode;
                    }
                    catch (ElementNotAvailableException)
                    {
                        return false;
                    }
                });
            });
            if (cts.IsCancellationRequested) return;
            _updateCts = null;
            //if (tree == null) return;
            ElementTreeChanged?.Invoke(this, tree);
        }

        static readonly object _lock = new object();
        private AutomationElement GetTopLevelWindow(AutomationElement element, CancellationTokenSource cts)
        {
            lock (_lock)
            {
                TreeWalker walker = TreeWalker.ControlViewWalker;


                while (!cts.IsCancellationRequested && element != null && element.Current.ControlType.ProgrammaticName != ControlType.Window.ProgrammaticName)
                {
                    try
                    {
                        element = walker.GetParent(element);
                    }
                    catch (ElementNotAvailableException)
                    { }
                    catch (NullReferenceException)
                    {

                    }
                }

                try
                {
                    Debug.WriteLine("Top Level: " + element.Current.Name);
                }
                catch { }
                return element;
            }
        }

        CancellationTokenSource _focusSource;
        private async void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            _focusSource?.Cancel();
            var source = new CancellationTokenSource();
            _focusSource = source;
            await Task.Run(async () =>
            {
                try
                {
                    if (source.IsCancellationRequested) return;
                    Debug.WriteLine("focus changed");
                   
                    var focusedElement = sender as AutomationElement;
                    if (focusedElement == null) return;
                    var window = GetTopLevelWindow(focusedElement, source);
                    if (source.IsCancellationRequested) return;
                    if (window == null)
                    {
                        Debug.WriteLine("Null window");
                        return;
                    }
                    if (_focusedWindow == window)
                    {
                        Debug.WriteLine("Same window");
                        //return;
                    }
                    try
                    {
                        if (_focusedWindow != null)
                            Automation.RemoveStructureChangedEventHandler(_focusedWindow, OnStructureChanged);

                    }
                    catch (ArgumentException ex)
                    {

                    }

                    _focusedWindow = window;
                    FocusedWindowChanged?.Invoke(this, _focusedWindow);

                    await Update();
                    Automation.AddStructureChangedEventHandler(_focusedWindow, TreeScope.Subtree, OnStructureChanged);
                }
                catch (COMException)
                {

                }
            });

        }


        private async void OnStructureChanged(object sender, StructureChangedEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
            _cts = new CancellationTokenSource();
            var cts = _cts;
            await Task.Delay(100);
            if (cts.IsCancellationRequested) return;

            await Update();

            Debug.WriteLine("structure");
        }
    }
}
