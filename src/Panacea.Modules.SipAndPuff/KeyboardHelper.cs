using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Panacea.Modules.SipAndPuff
{
    class KeyboardHelper
    {
        private readonly Keys _forwardKey, _selectKey;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private CancellationTokenSource _keyCts;
        private IKeyboardMouseEvents m_GlobalHook;
        private readonly int _doubleStrokeDuration;
        public KeyboardHelper(Keys forward, Keys select, int doubleStrokeDuration = 200)
        {
            _forwardKey = forward;
            _selectKey = select;
            _doubleStrokeDuration = doubleStrokeDuration;
        }

        public void Start()
        {
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.KeyDown += GlobalHookKeyPress;
            m_GlobalHook.KeyUp += M_GlobalHook_KeyUp;
        }

        public void Stop()
        {
            m_GlobalHook.Dispose();
        }

        private void GlobalHookKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _forwardKey)
            {
                _stopwatch.Start();
                e.Handled = true;
            }
            else if (e.KeyCode == _selectKey)
            {
                Select?.Invoke(this, null);
                e.Handled = true;
            }
        }

        private async void M_GlobalHook_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _forwardKey)
            {
                _stopwatch.Stop();
                if (_stopwatch.ElapsedMilliseconds > 700)
                {
                    Back?.Invoke(this, null);

                }
                else
                {
                    var doubleTap = false;
                    if (_keyCts != null)
                    {
                        _keyCts.Cancel();
                        doubleTap = true;
                    }
                    _keyCts = new CancellationTokenSource();
                    var localCts = _keyCts;
                    await Task.Delay(_doubleStrokeDuration);
                    if (localCts.IsCancellationRequested) return;
                    _keyCts = null;

                    if (!doubleTap)
                    {
                        Next?.Invoke(this, null);
                    }
                    else
                    {
                        Previous?.Invoke(this, null);
                    }
                }
                _stopwatch.Reset();
            }
            else if (e.KeyCode == _selectKey)
            {
                e.Handled = true;
            }
        }

        public event EventHandler Back;

        public event EventHandler Previous;

        public event EventHandler Next;

        public event EventHandler Select;
    }
}
