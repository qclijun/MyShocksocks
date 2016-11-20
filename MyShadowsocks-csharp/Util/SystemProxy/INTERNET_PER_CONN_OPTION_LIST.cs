using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace MyShadowsocks.Util.SystemProxy
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    class INTERNET_PER_CONN_OPTION_LIST : IDisposable
    {
        public int Size;

        public IntPtr Connection;

        public int OptionCount;

        public int OptionError;

        public IntPtr pOptions;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connection != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Connection);
                    Connection = IntPtr.Zero;
                }
                if (pOptions != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pOptions);
                    pOptions = IntPtr.Zero;
                }
            }
        }
    }
}
