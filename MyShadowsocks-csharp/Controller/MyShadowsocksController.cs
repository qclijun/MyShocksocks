using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jun.Net.SystemProxy;
using MyShadowsocks.Controller.Strategy;
using MyShadowsocks.Encryption;
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




        //ServersChanged有可能是由Timer引起的，也有可能时由UI引起的
        public static event EventHandler ServersChanged;

        private static Timer _updateTimer;

        private PrivoxyRunner _privoxyRunner;
        private ProxyListener _listener;

        private PACFileUpdater _pacFileUpdater;


        public bool IsRunning { get; private set; }

        private static async void UpdateServerList() {
            try {
                bool needUpdate = await DownloadServerInfo.UpdateServerList(Config.ServerList);

                if(needUpdate) {
                    //Config.ServerChanged = true;
                    //Config.SaveToFile();

                    //ServersChanged?.Invoke(typeof(Timer), null);
                    RaiseServersChangedEvent(typeof(Timer));
                    logger.Info("Update servers: Success.");
                } else {
                    logger.Info("Update servers: No need.");
                }
            } catch(Exception ex) {
                logger.Error("Update servers:  Error. " + ex.Message);
            }
        }

        public static void RaiseServersChangedEvent(object sender) {
            ServersChanged?.Invoke(sender, null);
        }


        static MyShadowsocksController() {
            _config = Configuration.Load();
            UpdateServerList();
            TimeSpan ts = TimeSpan.FromHours(1);
            //TimeSpan ts = TimeSpan.FromSeconds(10);
            _updateTimer = new Timer(obj => UpdateServerList(), null, ts, ts);
        }



        public MyShadowsocksController() {
            _privoxyRunner = new PrivoxyRunner();
            _listener = new ProxyListener();
            _pacFileUpdater = new PACFileUpdater();
            _pacFileUpdater.PACFileChanged += PacFileUpdater_PACFileChanged;
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
            

            SetMode(Config.Global);

            //t一直在运行，await t之后的代码只能在异常时运行
            //_listener出现异常（如端口被占用） 由谁来处理？？？需不需要用try-catch包含await t??， controller处理不了，还是交给UI
            await t;
        }

        public async Task Restart() {
            Stop();
            await Start();
        }

        private const string PacFileName = PACFileUpdater.PAC_FILE;

        private string tempPacFilePath = "";

        private string GetTempPacFile() {
            if(tempPacFilePath == "") NewTempPacFile();
            return "file:///" + tempPacFilePath;
        }

        private void NewTempPacFile() {
            string pac;
            if(!File.Exists(PacFileName)) {
                pac = Jun.Utils.UnZipText(Resources.proxy_pac_txt);
                File.WriteAllText(PacFileName, pac);
            } else {
                using(StreamReader r = new StreamReader(File.Open(PacFileName,FileMode.Open,FileAccess.Read,
                    FileShare.ReadWrite))) {
                    pac = r.ReadToEnd();
                }
            }
            tempPacFilePath = Path.GetTempFileName();
            File.WriteAllText(tempPacFilePath, pac.Replace("__PROXY__", GetProxyString()));

        }




        private string GetProxyString() {
            return "Proxy " + _privoxyRunner.BindEp.ToString() + ";";
        }




        public void SetMode(bool globalMode) {
            Contract.Requires<Exception>(IsRunning);

            Config.Global = globalMode;

            if(globalMode) {
                logger.Info("Proxy Mode: Global");
                WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_Direct, _privoxyRunner.BindEp.ToString(), null);
            } else {
                //pacMode
                logger.Info("Proxy Mode: PAC");
                //设置系统代理
                WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_PAC, null, GetTempPacFile());
            }
        }




        public async Task OnProxyPortChanged() {
            if(!IsRunning) return;
            _privoxyRunner.Stop();
            _privoxyRunner.Start(_config.LocalPort);
            _listener.Stop();
            await _listener.StartListen(_config.LocalPort);
        }



        public static void SetStrategy(string newStrategy) {
           
            logger.Info("Change strategy to: " + newStrategy);

            Config.Strategy = newStrategy;
            SocksProxyConnection.SetStrategy(newStrategy);
            
        }



        private void PacFileUpdater_PACFileChanged(object sender, FileSystemEventArgs e) {
            if(Config.Global) return;

            logger.Info("PAC File Changed.");
            NewTempPacFile();
            WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_PAC, null, GetTempPacFile());
        }


        public static IEnumerable<string> SupportedServerSelectors => ServerSelectorManager.SupportedServerSelectors;

        public static IEnumerable<string> SuppportedEncryptMethods => MbedEncryptor.SupportedMethods;


    }
}
