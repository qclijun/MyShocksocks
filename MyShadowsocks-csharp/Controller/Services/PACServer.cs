using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

using Shadowsocks.Model;
using Shadowsocks.Properties;

namespace Shadowsocks.Controller
{
    class PACServer : Service
    {
        public const string PAC_FILE = "pac.txt";
        public const string USER_RULE_FILE = "user-rule.txt";
        public const string USER_ABP_FILE = "abp.txt";

        FileSystemWatcher PACFileWatcher;
        FileSystemWatcher UserRuleFileWatcher;
        private Configuration _config;

        public event EventHandler PACFileChanged;
        public event EventHandler UserRuleFileChanged;

        public PACServer()
        {
            this.WatchPacFile();
            this.WatchUserRuleFile();
        }

        private void UpdateConfiguration(Configuration config)
        {
            this._config = config;
        }

        public override bool Handle(byte[] firstPacket, int length, Socket socket, object state)
        {
            if (socket.ProtocolType != ProtocolType.Tcp) return false;
            try
            {
                string request = Encoding.UTF8.GetString(firstPacket, 0, length);
                string[] lines = request.Split('\r', '\n');
                bool hostMatch = false, pathMatch = false, useSocks = false;
                foreach (var line in lines)
                {
                    string[] kv = line.Split(new char[] { ':' }, 2);
                    if (kv.Length == 2)
                    {
                        if (kv[0] == "Host")
                        {
                            if (kv[1].Trim() == ((IPEndPoint)socket.LocalEndPoint).ToString())
                                hostMatch = true;
                        }
                    }
                    else if (kv.Length == 1)
                    {
                        if (line.IndexOf("pac", StringComparison.Ordinal) >= 0)
                            pathMatch = true;
                    }
                }
                if (hostMatch && pathMatch)
                {
                    SendRequest(firstPacket, length, socket, useSocks);
                    return true;
                }
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private string TouchPACFile()
        {
            if (!File.Exists(PAC_FILE))
                FileManager.UncompressFile(PAC_FILE,Resources.proxy_pac_txt);
            return PAC_FILE;

        }

        private string TouchUserRuleFile()
        {
            if (!File.Exists(USER_RULE_FILE))
                File.WriteAllText(USER_RULE_FILE, Resources.user_rule);
            return USER_RULE_FILE;
        }

        private string GetPACContent()
        {
            if (File.Exists(PAC_FILE))
                return File.ReadAllText(PAC_FILE, Encoding.UTF8);
            else
                return Shadowsocks.Util.Utils.UnGzip(Resources.proxy_pac_txt);
        }

        private string GetPACAddress(byte[] requestBuf, int length, IPEndPoint localEndPoint, bool useSocks)
        {
            return (useSocks ? "SOCKS5" : "PROXY ") + localEndPoint.Address + ":" + this._config.LocalPort + ";";
        }

        private void SendRequest(byte[] firstPacket, int length, Socket socket, bool useSocks)
        {
            try
            {
                string pac = GetPACContent();
                IPEndPoint localEndPoint = (IPEndPoint)socket.LocalEndPoint;
                string proxy = GetPACAddress(firstPacket, length, localEndPoint, useSocks);
                pac = pac.Replace("__PROXY__", proxy);
                string text = string.Format(@"HTTP/1.1 200 OK
Server: Shandowsocks
Content-Type: application/x-ns-proxy-autoconfig
Content-Length: {0}
Connection: Close

", Encoding.UTF8.GetBytes(pac).Length) + pac;
                byte[] response = Encoding.UTF8.GetBytes(text);
                socket.BeginSend(response, 0, response.Length, 0, new AsyncCallback(SendCallback), socket);
                Util.Utils.ReleaseMemory(true);
            }catch(Exception e)
            {
                Logging.LogUsefulException(e);
                socket.Close();
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket conn = (Socket)ar.AsyncState;
            try
            {
                conn.Shutdown(SocketShutdown.Send);
            }
            catch { }
        }

        private void WatchPacFile()
        {
            if (PACFileWatcher != null) PACFileWatcher.Dispose();
            PACFileWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory());
            PACFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            PACFileWatcher.Changed += PACFileWatcher_Changed;
            PACFileWatcher.Created += PACFileWatcher_Changed;
            PACFileWatcher.Deleted += PACFileWatcher_Changed;
            PACFileWatcher.Renamed += PACFileWatcher_Changed;
            PACFileWatcher.EnableRaisingEvents = true;
        }

        private void WatchUserRuleFile()
        {
            if (UserRuleFileWatcher != null) UserRuleFileWatcher.Dispose();
            UserRuleFileWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory());
            UserRuleFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            UserRuleFileWatcher.Changed += UserRuleFileWatcher_Changed;
            UserRuleFileWatcher.Created += UserRuleFileWatcher_Changed;
            UserRuleFileWatcher.Deleted += UserRuleFileWatcher_Changed;
            UserRuleFileWatcher.Renamed += UserRuleFileWatcher_Changed;
            UserRuleFileWatcher.EnableRaisingEvents = true;
        }

        private static Dictionary<string, DateTime> fileChangedTime = new Dictionary<string, DateTime>();

        private void UserRuleFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath.ToString();
            var currentLastWriteTime = File.GetLastWriteTime(e.FullPath);

            DateTime value;
            if (!fileChangedTime.TryGetValue(path, out value) || value != currentLastWriteTime)
            {
                if (UserRuleFileChanged != null)
                {
                    Logging.Info($"Detected: User Rule file '{e.Name}' was {e.ChangeType.ToString().ToLower()}.");
                    UserRuleFileChanged(this, new EventArgs());
                }
                fileChangedTime[path] = currentLastWriteTime;
            }
        }

       

        private void PACFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath.ToString();
            var currentLastWriteTime = File.GetLastWriteTime(e.FullPath);

            DateTime value;
            if(!fileChangedTime.TryGetValue(path,out value)||value!=currentLastWriteTime)
            {
                if (PACFileChanged != null)
                {
                    Logging.Info($"Detected: PAC file '{e.Name}' was {e.ChangeType.ToString().ToLower()}.");
                    PACFileChanged(this, new EventArgs());
                }
                fileChangedTime[path] = currentLastWriteTime;
            }
            
        }

       
    }
}
