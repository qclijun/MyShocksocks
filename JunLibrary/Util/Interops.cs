﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Jun.Util {
    public static class Interops {

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string path);
    }
}
