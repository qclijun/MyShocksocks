using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jun.Net.SystemProxy;

using MyShadowsocks.Model;
using MyShadowsocks.Properties;
using NLog;

namespace MyShadowsocks.Controller {
    public class MyShadowsocksController {

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        //only one configuration
        private static Configuration _config;

        public static Configuration Config => _config;

        public static List<Server> ServerList => _config.ServerList;


        //当_updateTimer更新Servers完成后引发该事件
        public static event EventHandler Timer_ServersUpdated;


        private PrivoxyRunner _privoxyRunner;
        private ProxyListener _listener;
        private Timer _updateTimer;

        public bool IsRunning { get; private set; }



        public static void LoadConfiguration() {
            _config = Configuration.Load();
            //UpdateServerList();
        }

        private async void UpdateServerList() {
            try {
                bool needUpdate = await DownloadServerInfo.UpdateServerList(Config.ServerList);
                
                if(needUpdate) {
                    //Config.ServerChanged = true;
                    //Config.SaveToFile();

                    Timer_ServersUpdated?.Invoke(this, null);
                    logger.Info("Update servers: Success.");
                } else {
                    logger.Info("Update servers: No need.");
                }
            } catch(Exception ex) {
                logger.Error("Update servers:  Error. " + ex.Message);
            }
        }

        static MyShadowsocksController() {
            LoadConfiguration();
        }



        public MyShadowsocksController() {
            _privoxyRunner = new PrivoxyRunner();
            _listener = new ProxyListener();
            //TimeSpan ts = TimeSpan.FromHours(1);
            TimeSpan ts = TimeSpan.FromSeconds(10);
            _updateTimer = new Timer(obj => UpdateServerList(), null, ts, ts);
            IsRunning = false;
        }

        public void Stop() {
            if(!IsRunning) return;
            IsRunning = false;
            _listener?.Stop();
            _privoxyRunner?.Stop();

            WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_None, null, null);
        }


 


        public async Task Start() {
            if(IsRunning) throw new Exception("Controller  already running. Please stop it first.");

            Task t = _listener.StartListen(Config.LocalPort);//t开始运行
            _privoxyRunner.Start(Config.LocalPort);
            IsRunning = true;



            //设置系统代理
            GeneratePacFile(PacFileName);
            WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_PAC, null, GetPacAddress());


            //t一直在运行，await t之后的代码只能在异常时运行
            //_listener出现异常（如端口被占用） 由谁来处理？？？需不需要用try-catch包含await t??， controller处理不了，还是交给UI
            await t;
        }

        public async Task Restart() {
            Stop();
            await Start();
        }


        private const string PacFileName = "pac.txt";

        private string GetPacAddress() {
            var path = Path.GetFullPath(PacFileName);
            return "file:///" + path;
        }

        private string GetProxyString() {
            return "Proxy " + _privoxyRunner.BindEp.ToString() + ";";
        }

        private void GeneratePacFile(string filename) {
            using(var zipInput = new System.IO.Compression.GZipStream(new MemoryStream(Resources.proxy_pac_txt),
                System.IO.Compression.CompressionMode.Decompress))
            using(var r = new StreamReader(zipInput))
            using(var w = new StreamWriter(File.Create(filename))) {
                string line;
                while((line = r.ReadLine()) != null) {
                    //if(line.IndexOf("__PROXY__", StringComparison.Ordinal) != -1) {
                    line = line.Replace("__PROXY__", GetProxyString());
                    w.WriteLine(line);
                    //}
                }
            }
        }



        public async Task OnProxyPortChanged() {
            if(!IsRunning) return;
            _privoxyRunner.Stop();
            _privoxyRunner.Start(_config.LocalPort);
            _listener.Stop();
            await _listener.StartListen(_config.LocalPort);
        }

        public void OnServerChanged() {


            //do nothing
            //当服务器信息改变后，只影响以后新建立的连接，已经建立的连接连不上了就会自动关闭
            //_listener和_privoxyRunner继续运行
        }

    }
}
