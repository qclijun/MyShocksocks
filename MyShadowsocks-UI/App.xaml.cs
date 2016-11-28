using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

        // static member 
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        
        

        private static MyShadowsocksController _controller;

        public static MyShadowsocksController Controller => _controller;

        public static Configuration Config => MyShadowsocksController.Config;

        public static List<Server> ServerList => Config.ServerList;

        


        // instance member
        private NotifyIconWrapper _notifyIconComponent;
        private MainWindow _configWindow = null;

        internal bool IsFirstRun {
            get; private set;
        }


        static App() {
            _controller = new MyShadowsocksController();
        }

        public App() {
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");


            DispatcherUnhandledException += Application_DispatcherUnhandledException;
            this.Exit += App_Exit;
            this.Startup += App_Startup;

            //no-UI exception
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _notifyIconComponent = new NotifyIconWrapper(this);
            _configWindow = null;

            IsFirstRun = !File.Exists(Configuration.CONFIG_FILE);
        }

        private void App_Startup(object sender, StartupEventArgs e) {
            // start the controller
            if(Config.Enabled) StartController();

        }

        private void App_Exit(object sender, ExitEventArgs e) {
            App.Controller.Stop();
            App.Config.SaveToFile();
            
            _notifyIconComponent.Dispose();
        }




        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
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


        internal void ShowConfigWindow() {
            if(_configWindow != null) {
                _configWindow.WindowState = WindowState.Normal;
                _configWindow.Activate();
            } else {
                _configWindow = new MainWindow();
                _configWindow.Show();
                _configWindow.Activate();
                _configWindow.Closed += ConfigWindow_Closed;
            }            
        }

        private void ConfigWindow_Closed(object sender, EventArgs e) {
            _configWindow = null;
            
            if(IsFirstRun) {
                _notifyIconComponent.ShowFirstTimeBalloon();
                IsFirstRun = false;
            }
        }

        internal static async void StartController() {
            try {
                await App.Controller.Start();
            } catch(SocketException ex) {
                ReportError("StartController failed.", ex);

            }
        }

        internal static async void RestartController() {
            try {
                await App.Controller.Restart();
            } catch(SocketException ex) {
                ReportError("StartController failed.", ex);
            }
        }


        internal static void StopController() {
            App.Controller.Stop();
        }

        internal static void ReportError(string msg) {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        internal static void ReportError(string msg, Exception ex) {
            MessageBox.Show($"{msg} {ex.GetType()}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
