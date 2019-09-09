using SharpDX.DirectInput;
using SharpDX.Multimedia;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace Panacea.Modules.SipAndPuff
{
    class SharpDxHelper : ISipAndPuffInput
    {
        bool _stopped;

        public void Dispose()
        {

        }

        private readonly int _doubleStrokeDuration = 200;

        Thread _thread;
        private CancellationTokenSource _keyCts;
        public event EventHandler SipUp;
        public event EventHandler SipDown;
        public event EventHandler PuffUp;
        public event EventHandler PuffDown;

        public void Start()
        {

            _stopped = false;
            _thread = new Thread(async () =>
            {
                while (!_stopped)
                {
                    try
                    {

                        var directInput = new DirectInput();

                        // Find a Joystick Guid
                        var joystickGuid = Guid.Empty;

                        foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                            joystickGuid = deviceInstance.InstanceGuid;

                        // If Gamepad not found, look for a Joystick
                        if (joystickGuid == Guid.Empty)
                            foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                                joystickGuid = deviceInstance.InstanceGuid;

                        // If Joystick not found, throws an error
                        if (joystickGuid == Guid.Empty)
                        {
                            Debug.WriteLine("No joystick/Gamepad found.");
                            continue;
                        }

                        // Instantiate the joystick
                        var joystick = new Joystick(directInput, joystickGuid);

                        Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

                        // Query all suported ForceFeedback effects
                        var allEffects = joystick.GetEffects();
                        foreach (var effectInfo in allEffects)
                            Console.WriteLine("Effect available {0}", effectInfo.Name);

                        // Set BufferSize in order to use buffered data.
                        joystick.Properties.BufferSize = 128;

                        // Acquire the joystick
                        joystick.Acquire();
                        while (!_stopped)
                        {
                            joystick.Poll();

                            var datas = joystick.GetBufferedData();
                            foreach (var state in datas)
                            {

                                if (state.RawOffset > 10)
                                {
                                    switch (state.Offset)
                                    {
                                        case JoystickOffset.Buttons0:
                                            if (state.Value == 0)
                                            {
                                                SipUp?.Invoke(this, null);
                                            }
                                            else
                                            {
                                                SipDown?.Invoke(this, null);
                                            }
                                            break;
                                        case JoystickOffset.Buttons1:
                                            if (state.Value == 0)
                                            {
                                                PuffUp?.Invoke(this, null);
                                            }
                                            else
                                            {
                                                PuffDown?.Invoke(this, null);
                                            }
                                            break;
                                    }
                                }
                            }
                            Thread.Sleep(30);
                        }
                    }
                    catch { }
                    await Task.Delay(3000);
                }
            })
            {
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };
            _thread.Start();
            // Poll events from joystick

        }

        public void Stop()
        {
            if (_thread == null) return;
            _stopped = true;
            _thread.Join();
            _thread = null;
        }
    }
}
