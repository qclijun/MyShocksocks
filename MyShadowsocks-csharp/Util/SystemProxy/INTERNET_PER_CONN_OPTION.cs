using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace Shadowsocks.Util.SystemProxy
{
    enum INTERNET_PER_CONN_OptionEnum
    {
        INTERNET_PER_CONN_FLAGS = 1,
        INTERNET_PER_CONN_PROXY_SERVER = 2,
        INTERNET_PER_CONN_PROXY_BYPASS = 3,
        INTERNET_PER_CONN_AUTOCONFIG_URL = 4,
        INTERNET_PER_CONN_AUTODISCOVERY_FLAGS = 5,
        INTERNET_PER_CONN_AUTOCONFIG_SECONDARY_URL = 6,
        INTERNET_PER_CONN_AUTOCONFIG_RELOAD_DELAY_MINS = 7,
        INTERNET_PER_CONN_AUTOCONFIG_LAST_DETECT_TIME = 8,
        INTERNET_PER_CONN_AUTOCONFIG_LAST_DETECT_URL = 9,
        INTERNET_PER_CONN_FLAGS_UI = 10,
    }


    [Flags]
    enum INTERNET_OPTION_PER_CONN_FLAG
    {
        PROXY_TYPE_DIRECT = 0x00000001,
        PROXY_TYPE_PROXY = 0x00000002,
        PROXY_TYPE_AUTO_PROXY_URL = 0x00000004,
        PROXY_TYPE_AUTO_DETECT = 0x00000008,
    }


    [Flags]
    enum INTERNET_OPTION_PER_CONN_FLAGS_UI
    {
        PROXY_TYPE_DIRECT = 0x00000001,
        PROXY_TYPE_PROXY = 0x00000002,
        PROXY_TYPE_AUTO_PROXY_URL = 0x00000004,
        PROXY_TYPE_AUTO_DETECT = 0x00000008,
    }



    [StructLayout(LayoutKind.Explicit)]
    struct INTERNET_PER_CONN_OPTION_OPTIONUnion : IDisposable
    {
        [FieldOffset(0)]
        public int dwValue;
        [FieldOffset(0)]
        public IntPtr pszValue;
        [FieldOffset(0)]
        public System.Runtime.InteropServices.ComTypes.FILETIME ftValue;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (pszValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pszValue);
                    pszValue = IntPtr.Zero;
                }
            }
        }


    }

    struct INTERNET_PER_CONN_OPTION
    {
        public int dwOption;
        public INTERNET_PER_CONN_OPTION_OPTIONUnion Value;
    }
}
