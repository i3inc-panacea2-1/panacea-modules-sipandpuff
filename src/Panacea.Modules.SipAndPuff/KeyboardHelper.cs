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
    class KeyboardHelper:ISipAndPuffInput
    {
        private readonly Keys _forwardKey, _selectKey;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private CancellationTokenSource _keyCts;
        private IKeyboardMouseEvents m_GlobalHook;
        private readonly int _doubleStrokeDuration;

        public event EventHandler SipUp;
        public event EventHandler SipDown;
        public event EventHandler PuffUp;
        public event EventHandler PuffDown;

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

        private void GlobalHookKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _forwardKey)
            {
                SipDown?.Invoke(this, null);
                e.Handled = true;
            }
            else if (e.KeyCode == _selectKey)
            {
                PuffDown?.Invoke(this, null);
                e.Handled = true;
            }
        }

        public void Stop()
        {
            m_GlobalHook.Dispose();
        }

       

        private void M_GlobalHook_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _forwardKey)
            {
                SipUp?.Invoke(this, null);
                e.Handled = true;
            }
            else if (e.KeyCode == _selectKey)
            {
                PuffUp?.Invoke(this, null);
                e.Handled = true;
            }
        }

        public void Dispose()
        {
           
        }
    }
}
