using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyShadowsocks.Controller;
using MyShadowsocks.Model;
using Newtonsoft.Json;

namespace MyShadowsocks {
    static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new MyShadowsocks.View.ConfigForm());
        //}




        private static async Task Init() {
            //init global objects:
            Config = Configuration.Load();
            Cache = new EncryptorPool();
            try {
                bool needUpdate = await DownloadServerInfo.UpdateServerList(Config.ServerList);
                DefaultLogger.Debug("Need Update Server List: " + needUpdate);
                if(needUpdate) Configuration.Save(Config);
            }catch(Exception ex) {
                DefaultLogger.Error("Failed to update server list: "+ex.Message);
            }


            
        }


        static void Main() {
            DefaultLogger.Info("Test logger");

            

            MainAsync().Wait();
           






            Console.WriteLine("Ready to exit.");
            Console.ReadKey();
        }

        static async Task MainAsync() {
            try {
                await Init();
                ProxyListener listener = new ProxyListener();
                await listener.StartListen(9050);
            }catch(Exception ex) {
                DefaultLogger.Error(ex);
            }
        }


        public static NLog.Logger DefaultLogger = NLog.LogManager.GetLogger("default");

        public static Configuration Config;
        public static List<Server> ServerList => Config.ServerList;


        public static EncryptorPool Cache;





    }
}
