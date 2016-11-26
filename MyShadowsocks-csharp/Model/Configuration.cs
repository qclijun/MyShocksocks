using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using MyShadowsocks.Controller;
using Newtonsoft.Json;
using NLog;

namespace MyShadowsocks.Model {
    [Serializable]
    public sealed class Configuration {

        private static readonly Logger logger = Program.DefaultLogger;

        public List<Server> ServerList {
            get; set;
        } = new List<Server>();

        [JsonIgnore]
        public bool ServerChanged { get; set; } = false;


        public string Strategy { get; set; } = "";
        public int Index { get; set; } = 0;
        public bool Global { get; set; } = false; //global or pac
        public bool Enabled { get; set; } = true;
        public bool ShareOverLan { get; set; } = false;


        public int LocalPort { get; set; } = 1080;
        public string PacUrl { get; set; } = "";
        public bool UseOnlinePac { get; set; } = false;
        public bool AvailabilityStatistics { get; set; } = false;
        public bool AuthCheckUpdate { get; set; } = true;
        public bool IsVerboseLogging { get; set; } = false;

        public LogViewerConfiguration LogViewerConfig { get; set; } = new LogViewerConfiguration();
        public ProxyConfiguration ProxyConfig { get; set; } = new ProxyConfiguration();
        public HotkeyConfiguration HotkeyConfig { get; set; } = new HotkeyConfiguration();

        public const string CONFIG_FILE = "gui-config.json";

        public event EventHandler LocalPort_Changed;

        


        //因为需要序列化，所以不能设置成private
        public Configuration() { }


        public List<Server> GetServers() { return ServerList; }

        public Server GetCurrentServer() {
            if(Index >= 0 && Index < ServerList.Count)
                return ServerList[Index];
            else
                return GetDefaultServer();
        }

        public static Configuration Load() {
            return Load(CONFIG_FILE);
        }


        public static Configuration Load(string config_file) {
            try {
                string configContent = File.ReadAllText(config_file);
                Configuration config = JsonConvert.DeserializeObject<Configuration>(configContent);

                if(config.LocalPort <= 0 || config.LocalPort > 65535) config.LocalPort = 1080;
                if(config.Index == -1 && config.Strategy == null)
                    config.Index = 0;
                if(config.LogViewerConfig == null)
                    config.LogViewerConfig = new LogViewerConfiguration();
                if(config.ProxyConfig == null)
                    config.ProxyConfig = new ProxyConfiguration();
                if(config.HotkeyConfig == null)
                    config.HotkeyConfig = new HotkeyConfiguration();

                return config;
            } catch(FileNotFoundException) {
                return new Configuration();


            } catch(Exception e) {
                logger.Error(e.Message);
                return new Configuration();
            }
        }

  

        public void SaveToFile() {
            if(this.Index >= this.ServerList.Count)
                this.Index = this.ServerList.Count - 1;
            if(this.Index < -1) this.Index = -1;
            if(this.Index == -1 && this.Strategy == null)
                this.Index = 0;

            try {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                using(StreamWriter sw = new StreamWriter(File.Open(CONFIG_FILE, FileMode.Create)))
                using(JsonWriter writer = new JsonTextWriter(sw)) {
                    serializer.Serialize(writer, this);
                }
            } catch(IOException e) {
                logger.Error(e.Message);
                throw;
            }
        }



        public void AddServer(Server s) {
            ServerList.Add(s);
        }


        private static void CheckServer(Server server) {
            CheckPort(server.ServerPort);
            CheckPassword(server.Password);
            CheckServer(server.HostName);
            CheckTimeout(server.Timeout, Server.MaxServerTimeoutSec);
        }

        private static void CheckPort(int port) {
            if(port <= 0 || port > 65535)
                throw new ArgumentException(I18N.GetString("Port out of range"));
        }

        private static void CheckLocalPort(int port) {
            CheckPort(port);
            if(port == 8123)
                throw new ArgumentException(I18N.GetString("Port can't be 8123"));
        }

        private static void CheckPassword(string password) {
            if(string.IsNullOrEmpty(password))
                throw new ArgumentException(I18N.GetString("Password can not be blank"));
        }

        private static void CheckServer(string server) {
            if(string.IsNullOrEmpty(server))
                throw new ArgumentException(I18N.GetString("Server IP can not be blank"));
        }

        private static void CheckTimeout(int timeout, int maxTimeout) {
            if(timeout <= 0 || timeout > maxTimeout)
                throw new ArgumentException(string.Format(
                    I18N.GetString("Timeout is invalid, it should not exceed {0}"), maxTimeout));

        }

        private static Server GetDefaultServer() {
            return new Server();
        }


    }
}
