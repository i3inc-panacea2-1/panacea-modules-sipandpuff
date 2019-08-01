using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.SipAndPuff
{
    public interface ISipAndPuffInput : IDisposable
    {
        event EventHandler SipUp;

        event EventHandler SipDown;

        event EventHandler PuffUp;

        event EventHandler PuffDown;

        void Start();

        void Stop();

    }
}
