using System;
using System.Collections.Generic;
using System.Linq;
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

        private AutomationElement GetTopLevelWindow(AutomationElement element)
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement elementParent;
            AutomationElement node = element;
            AutomationElement elementRoot = null;
            if (node == elementRoot) return node;
            do
            {
                try
                {
                    if (node == null) break;

                    elementParent = walker.GetParent(node);
                    if (elementParent == AutomationElement.RootElement) break;

                    node = elementParent;
                }
                catch (ElementNotAvailableException)
                { }
            }
            while (true);
            try
            {
                Console.WriteLine("Top Level: " + node.Current.ControlType?.ProgrammaticName.ToString());
            }
            catch { }
            return node;
        }

        private async void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            await Task.Run(async () =>
            {
                Console.WriteLine("focus changed");

                var focusedElement = sender as AutomationElement;
                if (focusedElement == null) return;
                var window = GetTopLevelWindow(focusedElement);
                if (window == null)
                {
                    Console.WriteLine("Null window");
                    return;
                }
                if (_focusedWindow == window)
                {
                    Console.WriteLine("Same window");
                    return;
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

            Console.WriteLine("structure");
        }
    }
}
