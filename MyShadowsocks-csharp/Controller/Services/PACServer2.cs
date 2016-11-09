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
using Newtonsoft.Json;
using Shadowsocks.Util;

namespace Shadowsocks.Controller
{
    public class PACServer2 : Service
    {
        public const string PAC_FILE = "pac.txt";
        public const string USER_RULE_FILE = "user-rule.txt";
        public const string USER_ABP_FILE = "abp.txt";

        FileSystemWatcher PACFileWatcher;
        FileSystemWatcher UserRuleFileWatcher;
        private Configuration _config;
        private GfwUpdater _gfwUpdater;

        public event EventHandler<FileSystemEventArgs> PACFileChanged;
        

        public PACServer2()
        {
            _gfwUpdater = new GfwUpdater();
            _gfwUpdater.GfwFileChanged += OnProxyRulesChanged;
            _config = Configuration.Instance;
            this.WatchPacFile();
            this.WatchUserRuleFile();

        }

        public GfwUpdater GetGfwUpdater()
        {
            return _gfwUpdater;
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
                FileManager.UncompressFile(PAC_FILE, Resources.proxy_pac_txt);
            return PAC_FILE;

        }

        private string TouchUserRuleFile()
        {
            if (!File.Exists(USER_RULE_FILE))
                File.WriteAllText(USER_RULE_FILE, Resources.user_rule);
            return USER_RULE_FILE;
        }

        private string GetAbpFileContent()
        {
            string abpContent;
            if (File.Exists(USER_ABP_FILE))
            {
                abpContent = File.ReadAllText(USER_ABP_FILE, Encoding.UTF8);
            }
            else
            {
                abpContent = Utils.UnGzip(Resources.abp_js);
            }
            return abpContent;
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
            }
            catch (Exception e)
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
            catch {
                //TODO: exception handler
            }
        }


        private void WatchPacFile()
        {
            if (PACFileWatcher != null) PACFileWatcher.Dispose();
            PACFileWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory());
            PACFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            PACFileWatcher.Filter = PAC_FILE;
            PACFileWatcher.IncludeSubdirectories = false;
            PACFileWatcher.Changed += OnPacFileWatcher_Changed;
            PACFileWatcher.Created += OnPacFileWatcher_CreateDeleteRename;
            PACFileWatcher.Deleted += OnPacFileWatcher_CreateDeleteRename;
            PACFileWatcher.Renamed += OnPacFileWatcher_CreateDeleteRename;
            PACFileWatcher.EnableRaisingEvents = true;
        }

        private void WatchUserRuleFile()
        {
            if (UserRuleFileWatcher != null) UserRuleFileWatcher.Dispose();
            UserRuleFileWatcher = new FileSystemWatcher(Directory.GetCurrentDirectory());
            UserRuleFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            UserRuleFileWatcher.Filter = USER_RULE_FILE;
            UserRuleFileWatcher.Changed += UserRuleFileWatcher_Changed;
            UserRuleFileWatcher.Created += UserRuleFileWatcher_CreateDeleteRenamed;
            UserRuleFileWatcher.Deleted += UserRuleFileWatcher_CreateDeleteRenamed;
            UserRuleFileWatcher.Renamed += UserRuleFileWatcher_CreateDeleteRenamed;
            UserRuleFileWatcher.EnableRaisingEvents = true;
        }

        private static Dictionary<string, DateTime> fileChangedTime = new Dictionary<string, DateTime>();


        private void UserRuleFileWatcher_CreateDeleteRenamed(object sender, FileSystemEventArgs e)
        {
 
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Renamed:
                    RenamedEventArgs re = (RenamedEventArgs)e;
                    Logging.Info($"Detected: User rule file '{re.OldName}' renamed to {re.Name}.");
                    break;
                default:
                    Logging.Info($"Detected: User rule file '{e.Name}' {e.ChangeType.ToString().ToLower()}.");
                    break;
            }


            OnProxyRulesChanged(sender, e);
        }

        private void UserRuleFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {


            string path = e.FullPath;
            var currentLastWriteTime = File.GetLastWriteTime(e.FullPath);

            DateTime value;
            if (!fileChangedTime.TryGetValue(path, out value) || ( currentLastWriteTime- value).TotalSeconds>=1 )
            {
                Logging.Info($"Detected: User rule file '{e.Name}' modified.");
                
                fileChangedTime[path] = currentLastWriteTime;
                OnProxyRulesChanged(sender, e);
            }
        }

        //user-rule and gfwlist may both cause proxyRulesChanged
        private void OnProxyRulesChanged(object sender, EventArgs e)
        {
            // 

            if (!File.Exists(GfwUpdater.GfwListFilePath))
            {
                _gfwUpdater.UpdateGfwListFromUri(); // event transferred to GfwUpdater
                return;
            }

            string usrRuleFileName = USER_RULE_FILE;
            StringBuilder abpContent = new StringBuilder(GetAbpFileContent());
            string newPac = BuildPacFile(abpContent, usrRuleFileName, GfwUpdater.GfwListFilePath);
            if (!File.Exists(PAC_FILE) || File.ReadAllText(PAC_FILE) != newPac)
            {
                File.WriteAllText(PAC_FILE, newPac); // 
                // raise PACFileWatcher.Changed
            }


        }

        private const string IgnoredLineBegins = "![";

        private static List<string> ParseProxyRulesFromUsr(string usrRuleFileName)
        {
            List<string> valid_lines = new List<string>();
            if (File.Exists(usrRuleFileName))
            {
                using (var sr = new StreamReader(File.OpenRead(usrRuleFileName)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line == "" || line.IsWhiteSpace()) continue;
                        if (line.BeginWithAny(IgnoredLineBegins)) continue;
                        valid_lines.Add(line);
                    }
                }
            }
            return valid_lines;
        }

        public static List<string> ParseProxyRulesFromGfw(string gfwListFileName)
        {
            byte[] bytes = Convert.FromBase64String(File.ReadAllText(gfwListFileName));
            //string content = Encoding.ASCII.GetString(bytes);
            List<string> valid_lines = new List<string>();
            using (var sr = new StreamReader(new MemoryStream(bytes), Encoding.ASCII))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {

                    if (line == "" || line.IsWhiteSpace()) continue;
                    if (line.BeginWithAny(IgnoredLineBegins)) continue;
                    //if (IgnoredLineBegins.Contains(line[0])) continue;
                    valid_lines.Add(line);
                }
            }
            return valid_lines;
        }


        private static List<string> ParseProxyRules(string usrRuleFileName, string gfwListFileName)
        {
            List<string> valid_lines = ParseProxyRulesFromGfw(gfwListFileName);
            valid_lines.AddRange(ParseProxyRulesFromUsr(usrRuleFileName));
            return valid_lines;
        }


        private static string BuildPacFile(StringBuilder abpContent, string usrRuleFileName, string gfwListFileName)
        {
            List<string> ruleList = ParseProxyRules(usrRuleFileName, gfwListFileName);
            abpContent.Replace("__RULES__", JsonConvert.SerializeObject(ruleList, Formatting.Indented));
            return abpContent.ToString();
        }


        private void OnPacFileWatcher_CreateDeleteRename(object sender, FileSystemEventArgs e)
        {
            //Console.WriteLine($"{e.Name}: {e.ChangeType.ToString()}");
            OnPacFileChanged(sender, e);
        }



        private void OnPacFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            //Console.WriteLine($"{e.Name}: {e.ChangeType.ToString()}");
            string path = e.FullPath;
            var currentLastWriteTime = File.GetLastWriteTime(e.FullPath);
            DateTime value;
            if (!fileChangedTime.TryGetValue(path, out value) || (currentLastWriteTime - value).TotalSeconds >= 1) //FileSystemWatcher may raise many events
            {
                OnPacFileChanged(sender, e);
                fileChangedTime[path] = currentLastWriteTime;
            }
        }

        private void OnPacFileChanged(object sender, FileSystemEventArgs e)
        {
            var handler = PACFileChanged;
            if (handler != null)
            {
                Logging.Info($"Detected: PAC file '{e.Name}' was {e.ChangeType.ToString().ToLower()}.");
                handler(sender, e);
            }
        }

    }
}
