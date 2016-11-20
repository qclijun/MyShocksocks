using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace MyShadowsocks.Util
{
    public class Interops
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string path);
    }
}
