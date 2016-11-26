using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MyShadowsocks.Controller;
using MyShadowsocks.Encryption;
using MyShadowsocks.Model;
using NLog;

namespace MyShadowsocks_UI {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static IEnumerable<string> _supportedMethods;
        private static IList<Server> _boundServerList;

        private static MyShadowsocksController _controller;

        public static MyShadowsocksController Controller => _controller;

        public static Configuration Config => MyShadowsocksController.Config;

        //与UI绑定, 与Config.ServerList中的元素指向同一个Server对象
        //也就是说，对现有的元素进行修改会同时改变Config.ServerList
        //但是添加和删除元素不会同步
        public static IList<Server> BoundServerList => _boundServerList;

        public static IEnumerable<string> SupportedMethods => _supportedMethods;


        static App() {
            InitGlobal();
        }

        public App() {
            DispatcherUnhandledException += Application_DispatcherUnhandledException;
            Exit += App_Exit;

            //no-UI exception
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_Exit(object sender, ExitEventArgs e) {
            App.Controller.Stop();
            App.Config.SaveToFile();
        }

        private static void InitGlobal() {
            _controller = new MyShadowsocksController();                           
            _boundServerList = new ObservableCollection<Server>(Config.ServerList);//元素与ServerList的元素指向相同的Server对象
            _supportedMethods = MbedTLSEncrytor.SupportedMethods();
        }

       

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            string errMsg = $"Exception Type: {e.Exception.GetType().ToString()} {Environment.NewLine}Stack Trace:{Environment.NewLine}" +
                $"{ e.Exception.StackTrace}";
            logger.Fatal(errMsg);
            MessageBox.Show("Unexcepted error, app will exit.", "UI Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown();
        }


        private static int exited = 0;

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            if(Interlocked.Increment(ref exited) == 1) {
                logger.Fatal(e.ExceptionObject?.ToString());
                MessageBox.Show("Unexcepted error, app will exit.", "UI Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown();
            }
        }
    }
}
