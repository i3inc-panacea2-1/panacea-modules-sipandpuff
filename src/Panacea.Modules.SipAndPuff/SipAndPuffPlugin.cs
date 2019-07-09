using Panacea.Modularity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.SipAndPuff
{
    public class SipAndPuffPlugin : IPlugin
    {
        public Task BeginInit()
        {
            _manager = new SipAndPuffManager();
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        SipAndPuffManager _manager;

        public Task EndInit()
        {
            _manager.Start();
            return Task.CompletedTask;
        }

        public Task Shutdown()
        {
            _manager.Stop();
            return Task.CompletedTask;
        }
    }
}
