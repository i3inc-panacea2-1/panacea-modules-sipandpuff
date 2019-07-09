using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.SipAndPuff
{
    static class ScreenReader
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(int uiAction, int uiParam, ref bool pvParam, int fWinIni);

        const int SPI_GETSCREENREADER = 0x0046;
        const int SPI_SETSCREENREADER = 0x0047;


        public static void Activate()
        {
            bool bScreenReader = true;
            bool retVal;

            retVal = SystemParametersInfo(SPI_SETSCREENREADER, 100, ref bScreenReader, 0x02);
        }


        public static bool IsScreenReaderRunning()
        {
            bool bScreenReader = false;
            bool retVal;

            retVal = SystemParametersInfo(SPI_GETSCREENREADER, 0, ref bScreenReader, 0);

            //uint iParam = 0;
            //uint iUpdate = 0;
            //bool result = false;
            //bool bReturn = SystemParametersInfo(SPI_GETSCREENREADER, iParam, &bScreenReader, iUpdate);
            return bScreenReader;
        }

        public static void Deactivate()
        {
            bool bScreenReader = false;
            bool retVal;

            retVal = SystemParametersInfo(SPI_SETSCREENREADER, 100, ref bScreenReader, 0x02);
        }
    }
}
