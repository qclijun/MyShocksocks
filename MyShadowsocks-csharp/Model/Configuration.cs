using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using MyShadowsocks.Controller;
using Newtonsoft.Json;
using NLog;

namespace MyShadowsocks.Model
{
    [Serializable]
    public sealed class Configuration
    {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public List<Server> ServerList
        {
            get; set;
        } = new List<Server>();

        public string Strategy { get; set; } = "";
        public int Index { get; set; } = 0;
        public bool Global { get; set; } = false; //global or pac
        public bool Enabled { get; set; } = true;
        public bool ShareOverLan { get; set; } = false;
        public bool IsDefault { get; set; } = true;

        public int LocalPort { get; set; } = 1080;
        public string PacUrl { get; set; } = "";
        public bool UseOnlinePac { get; set; } = false;
        public bool AvailabilityStatistics { get; set; } = false;
        public bool AuthCheckUpdate { get; set; } = true;
        public bool IsVerboseLogging { get; set; } = false;

        public LogViewerConfiguration LogViewerConfig { get; set; } = new LogViewerConfiguration();
        public ProxyConfiguration ProxyConfig { get; set; } = new ProxyConfiguration();
        public HotkeyConfiguration HotkeyConfig { get; set; } = new HotkeyConfiguration();

        private const string CONFIG_FILE = "gui-config.json";


        private static Configuration _instance = null;
        
        public static Configuration Instance
        {
            get
            {
                if (_instance == null) _instance = Load();
                return _instance;
            }
        }

        //因为需要序列化，所以不能设置成private
        public Configuration() { }


        public List<Server> GetServers() { return ServerList; }

        public Server GetCurrentServer()
        {
            if (Index >= 0 && Index < ServerList.Count)
                return ServerList[Index];
            else
                return GetDefaultServer();
        }



        private static Configuration Load()
        {
            try
            {
                string configContent = File.ReadAllText(CONFIG_FILE);
                Configuration config = JsonConvert.DeserializeObject<Configuration>(configContent);
                config.IsDefault = false;
                if (config.LocalPort <= 0 || config.LocalPort>65535) config.LocalPort = 1080;
                if (config.Index == -1 && config.Strategy == null)
                    config.Index = 0;
                if (config.LogViewerConfig == null)
                    config.LogViewerConfig = new LogViewerConfiguration();
                if (config.ProxyConfig == null)
                    config.ProxyConfig = new ProxyConfiguration();
                if (config.HotkeyConfig == null)
                    config.HotkeyConfig = new HotkeyConfiguration();

                return config;
            }
            catch (FileNotFoundException)
            {
                return GetDefaultConfig();


            }
            catch (Exception e)
            {
                logger.Error(e.Message);
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
                logger.Error(e.Message);
            }
        }

        private static void CheckServer(Server server)
        {
            CheckPort(server.ServerPort);
            CheckPassword(server.Password);
            CheckServer(server.ServerName);
            CheckTimeout(server.Timeout, Server.MaxServerTimeoutSec);
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
            return new Configuration();
        }
    }
}
