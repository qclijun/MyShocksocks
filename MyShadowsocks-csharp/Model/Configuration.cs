using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Shadowsocks.Controller;
using Newtonsoft.Json;

namespace Shadowsocks.Model
{
    [Serializable]
    public sealed class Configuration
    {
        public List<Server> ServerList
        {
            get; set;
        } = new List<Server>();

        public string Strategy { get; set; }
        public int Index { get; set; }
        public  bool Global { get; set; }
        public bool Enabled { get; set; }
        public bool ShareOverLan { get; set; }
        public bool IsDefault{ get; set; }

        public  int LocalPort { get; set; }
        public string PacUrl { get; set; }
        public bool UseOnlinePac { get; set; }
        public bool AvailabilityStatistics { get; set; }
        public bool AuthCheckUpdate { get; set; }
        public bool IsVerboseLogging { get; set; }

        public LogViewerConfiguration LogViewerConfig { get; set; }
        public ProxyConfiguration ProxyConfig { get; set; }
        public HotkeyConfiguration HotkeyConfig { get; set; }

        private const string CONFIG_FILE = "gui-config.json";

        
      

        public List<Server> GetServers() { return ServerList; }

        public Server GetCurrentServer()
        {
            if (Index >= 0 && Index < ServerList.Count)  
                return ServerList[Index];
            else
                return GetDefaultServer();
        }

        private static Configuration _instance = null;
        //singleton instance
        public static Configuration Instance
        {
            get
            {
                if (_instance == null) _instance = Load();
                return _instance;
            }
        }

        private Configuration() { }

        private static Configuration Load()
        {
            try
            {
                string configContent = File.ReadAllText(CONFIG_FILE);
                Configuration config = JsonConvert.DeserializeObject<Configuration>(configContent);
                config.IsDefault = false;
                if (config.LocalPort == 0) config.LocalPort = 1080;
                if (config.Index == -1 && config.Strategy == null)
                    config.Index = 0;
                if (config.LogViewerConfig == null)
                    config.LogViewerConfig = new LogViewerConfiguration();
                if (config.ProxyConfig == null)
                    config.ProxyConfig = new ProxyConfiguration();
                if (config.HotkeyConfig == null)
                    config.HotkeyConfig = new HotkeyConfiguration();
                if (config.ProxyConfig.ProxyType < ProxyConfiguration.PROXY_SOCKS5 || config.ProxyConfig.ProxyType > ProxyConfiguration.PROXY_HTTP)
                {
                    config.ProxyConfig.ProxyType = ProxyConfiguration.PROXY_SOCKS5;
                }
                return config;
            }
            catch (FileNotFoundException)
            {
                return GetDefaultConfig();


            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return GetDefaultConfig();
            }
        }

        public static void Save(Configuration config)
        {
            if (config.Index >= config.ServerList.Count)
                config.Index = config.ServerList.Count - 1;
            if (config.Index < -1) config.Index = -1;
            if (config.Index == -1 && config.Strategy == null)
                config.Index = 0;
            config.IsDefault = false;
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(CONFIG_FILE, FileMode.Create)))
                {
                    string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                    sw.Write(jsonString);
                    sw.Flush();
                }
            }
            catch (IOException e)
            {
                Logging.LogUsefulException(e);
            }
        }

        private static void CheckServer(Server server)
        {
            CheckPort(server.server_port);
            CheckPassword(server.password);
            CheckServer(server.server);
            CheckTimeout(server.timeout, Server.MaxServerTimeoutSec);
        }

        private static void CheckPort(int port)
        {
            if (port <= 0 || port > 65535)
                throw new ArgumentException(I18N.GetString("Port out of range"));
        }

        private static void CheckLocalPort(int port)
        {
            CheckPort(port);
            if (port == 8123)
                throw new ArgumentException(I18N.GetString("Port can't be 8123"));
        }

        private static void CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException(I18N.GetString("Password can not be blank"));
        }

        private static void CheckServer(string server)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentException(I18N.GetString("Server IP can not be blank"));
        }

        private static void CheckTimeout(int timeout, int maxTimeout)
        {
            if (timeout <= 0 || timeout > maxTimeout)
                throw new ArgumentException(string.Format(
                    I18N.GetString("Timeout is invalid, it should not exceed {0}"), maxTimeout));

        }

        private static Server GetDefaultServer()
        {
            return new Server();
        }

        private static Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Index = 0,
                IsDefault = true,
                LocalPort = 1080,
                AuthCheckUpdate = true,
                ServerList = new List<Server>()
                    {
                        GetDefaultServer()
                    }
            };
        }
    }
}
