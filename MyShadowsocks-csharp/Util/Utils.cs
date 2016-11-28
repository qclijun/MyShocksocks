using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using NLog;
using System.Net.NetworkInformation;
using System.Net;

namespace MyShadowsocks.Util {
    class Utils {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _portableMode;
        private static string tempPath = null;

        public static bool IsPortableMode() {
            if(_portableMode) return true;
            _portableMode = File.Exists(Path.Combine(
                 Application.StartupPath, "shadowsocks_portable_mode.txt"));
            return _portableMode;
        }

        public static string GetTempPath() {
            if(tempPath == null) {
                if(IsPortableMode())
                    try {
                        Directory.CreateDirectory(Path.Combine(Application.StartupPath, "temp"));
                    } catch(Exception e) {
                        tempPath = Path.GetTempPath();
                        logger.Error(e.Message);
                    } finally {
                        tempPath = Path.Combine(Application.StartupPath, "temp");
                    } else
                    tempPath = Path.GetTempPath();

            }
            return tempPath;
        }

        public static string GetTempPath(string filename) {
            return Path.Combine(GetTempPath(), filename);
        }

 


        public static void ReleaseMemory(bool removePages) {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            if(removePages) {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle,
                    (UIntPtr)0xFFFFFFFF,
                    (UIntPtr)0xFFFFFFFF);
            }
        }
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessWorkingSetSize(IntPtr process,
            UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);
    }
}
