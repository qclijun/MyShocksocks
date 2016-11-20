using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Net;
using System.Runtime.InteropServices;

using MyShadowsocks.Properties;
using MyShadowsocks.Util.ProcessManagement;
using MyShadowsocks.Util;
using MyShadowsocks.Model;
using NLog;

namespace MyShadowsocks.Controller
{
    class PrivoxyRunner
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static int Uid;
        private static string UniqueConfigFile;
        private static Job PrivoxyJob;

        private Process _process;
        private int _runningPort;

        static PrivoxyRunner()
        {
            try
            {
                Uid = Application.StartupPath.GetHashCode();
                UniqueConfigFile = $"prioxy_{Uid}.conf";
                PrivoxyJob = new Job();

                FileManager.UncompressFile(Utils.GetTempPath("ss_privoxy.exe"), Resources.privoxy_exe);
                FileManager.UncompressFile(Utils.GetTempPath("mgwz.dll"), Resources.mgwz_dll);
            }catch(IOException ex)
            {
                logger.Error(ex.Message);
            }
        }


        public int RunningPort
        {
            get { return _runningPort; }
        }

        public void Start(Configuration config)
        {
            Server server = config.GetCurrentServer();
            if (_process == null)
            {
                Process[] existingPrivoxy = Process.GetProcessesByName("ss_privoxy");
                foreach(Process p in existingPrivoxy)
                {
                    KillProcess(p);
                }
                StringBuilder privoxyConfig = new StringBuilder(Resources.privoxy_conf);
                _runningPort = this.GetFreePort();
                privoxyConfig.Replace("__SOCKS_PORT__", config.LocalPort.ToString());
                privoxyConfig.Replace("__PRIVOXY_BIND_PORT__", _runningPort.ToString());
                privoxyConfig.Replace("__PRIVOXY_BIND_IP__", config.ShareOverLan ? "0.0.0.0" : "127.0.0.1");
                FileManager.ByteArrayToFile(Utils.GetTempPath(UniqueConfigFile),
                    Encoding.UTF8.GetBytes(privoxyConfig.ToString()));

                _process = new Process();
                _process.StartInfo.FileName = "ss_privoxy.exe";
                _process.StartInfo.Arguments = UniqueConfigFile;
                _process.StartInfo.WorkingDirectory = Utils.GetTempPath();
                _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _process.StartInfo.UseShellExecute = true;
                _process.StartInfo.CreateNoWindow = true;
                _process.Start();

                PrivoxyJob.AddProcess(_process.Handle);
            }
            RefreshTrayArea();
        }

        public void Stop()
        {
            if (_process != null)
            {
                KillProcess(_process);
                _process = null;
            }
            RefreshTrayArea();
        }

        private static void KillProcess(Process p)
        {
            try
            {
                p.CloseMainWindow();
                p.WaitForExit(100);
                if (!p.HasExited)
                {
                    p.Kill();
                    p.WaitForExit();
                }
            }catch(Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private static bool IsChildProcess(Process process)
        {
            if(Utils.IsPortableMode())
            {
                try
                {
                    string path = process.MainModule.FileName;
                    return Utils.GetTempPath("ss_privoxy.exe").Equals(path);
                }catch(Exception ex)
                {
                    logger.Error(ex.Message);
                    return false;
                }
            }
            else
            {
                try
                {
                    var cmd = process.GetCommandLine();
                    return cmd.Contains(UniqueConfigFile);
                }catch(Win32Exception ex)
                {
                    if ((uint)ex.ErrorCode != 0x80004005)
                    {
                        throw;
                    }
                }
                return false;
            }
            
        }

        private int GetFreePort()
        {
            int defaultPort = 8123;
            try
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
                HashSet<int> usedPorts = new HashSet<int>();
                foreach (var ep in tcpEndPoints) usedPorts.Add(ep.Port);
                for(int port= defaultPort; port <= 65535; ++port)
                {
                    if (!usedPorts.Contains(port)) return port;
                }
                
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
                return defaultPort;
            }
            throw new Exception("No free port found.");
        }

        
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter,
            string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hwnd, uint msg, int wParam, int lParam);

        private static void RefreshTrayArea()
        {
            IntPtr systemTrayContainerHandle = FindWindow("shell_TrayWnd", null);
            IntPtr systemTrayHandle = FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr sysPagerHandle = FindWindowEx(systemTrayHandle, IntPtr.Zero, "SysPager", null);
            IntPtr notificationAreaHandle = FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", "Notification Area");
            if (notificationAreaHandle == IntPtr.Zero)
            {
                notificationAreaHandle = FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", "User Promoted Notification Area");
                IntPtr notifyIconOverflowWindowHandle = FindWindow("NotifyIconOverflowWindow", null);
                IntPtr overflowNotificationAreaHenale = FindWindowEx(notifyIconOverflowWindowHandle, IntPtr.Zero, "ToolbarWindow32", "Overflow Notification Area");
                RefreshTrayArea(overflowNotificationAreaHenale);
            }
        }
        private static void RefreshTrayArea(IntPtr windowHandle)
        {
            const int wmMousemove = 0x0200;
            RECT rect;
            GetClientRect(windowHandle, out rect);
            for(var x = 0; x < rect.right; x += 5)
            {
                for(var y = 0; y < rect.bottom; y += 5)
                {
                    SendMessage(windowHandle, wmMousemove, 0, (y << 16) + x);
                }
            }
        }
    }
}
