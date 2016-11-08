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

namespace Shadowsocks.Util
{
    class Utils
    {
        private static bool _portableMode;
        private static string tempPath = null;

        public static bool IsPortableMode()
        {
            if (_portableMode) return true;
               _portableMode = File.Exists(Path.Combine(
                    Application.StartupPath, "shadowsocs_portable_mode.txt"));
            return _portableMode;
        }

        public static string GetTempPath()
        {
            if (tempPath == null)
            {
                if (IsPortableMode())
                    try
                    {
                        Directory.CreateDirectory(Path.Combine(Application.StartupPath, "temp"));
                    }
                    catch (Exception e)
                    {
                        tempPath = Path.GetTempPath();
                        Shadowsocks.Controller.Logging.LogUsefulException(e);
                    }
                    finally
                    {
                        tempPath = Path.Combine(Application.StartupPath, "temp");
                    }
                else
                    tempPath = Path.GetTempPath();
                
            }
            return tempPath;
        }

        public static string GetTempPath(string filename)
        {
            return Path.Combine(GetTempPath(), filename);
        }

        public static string UnGzip(byte[] buf)
        {
            using (MemoryStream sb = new MemoryStream())
            using (var input = new System.IO.Compression.GZipStream(
                new MemoryStream(buf),
                System.IO.Compression.CompressionMode.Decompress,
                    false)){
                input.CopyTo(sb);
                return System.Text.Encoding.UTF8.GetString(sb.ToArray());
            }
            
        }



        public static void ReleaseMemory(bool removePages)
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            if (removePages)
            {
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
