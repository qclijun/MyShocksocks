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




    


        static void Main() {
            DefaultLogger.Info("Test logger");

            

            MainAsync().Wait();
           






            Console.WriteLine("Ready to exit.");
            Console.ReadKey();
        }

        static async Task MainAsync() {
            try {
                
                MyShadowsocksController controller = new MyShadowsocksController();

                await controller.Start();

            }catch(Exception ex) {
                DefaultLogger.Error(ex);
            }
        }


        public static NLog.Logger DefaultLogger = NLog.LogManager.GetLogger("default");

        


        





    }
}
