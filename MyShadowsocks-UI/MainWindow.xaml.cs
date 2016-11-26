using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyShadowsocks.Controller;
using MyShadowsocks.Model;

namespace MyShadowsocks_UI {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        //private bool _raiseSelectionEvent = true;

        public MainWindow() {
            InitializeComponent();

            LoadConfiguration();

            if(App.Config.Enabled) StartController();



        }

        private void LoadConfiguration() {
            MyShadowsocksController.Timer_ServersUpdated += Controller_Timer_ServersUpdated;

            gridConfiguration.DataContext = App.Config; //data binding
            cBoxMethod.ItemsSource = App.SupportedMethods;
            lstServerList.ItemsSource = App.BoundServerList;

            
            lstServerList.SelectedIndex = App.Config.Index;
            

            lstServerList.Focus();
            ReflushMoveBtn();


        }

        private void Controller_Timer_ServersUpdated(object sender, EventArgs e) {
            this.Dispatcher.BeginInvoke((ThreadStart)delegate () {
                App.BoundServerList.Clear();
                foreach(var s in App.Config.ServerList) {
                    App.BoundServerList.Add(s);
                }
                lstServerList.SelectedIndex = App.Config.Index;
                
            });
        }

        private void ReflushMoveBtn() {
            if(lstServerList.SelectedIndex == 0) btnMoveUp.IsEnabled = false;
            else btnMoveUp.IsEnabled = true;
            if(lstServerList.SelectedIndex == App.BoundServerList.Count - 1) btnMoveDown.IsEnabled = false;
            else btnMoveDown.IsEnabled = true;
        }

        private string GetServerListString() {
            StringBuilder sb = new StringBuilder();
            foreach(var s in App.Config.ServerList) {
                sb.AppendLine(s.ToString());
            }
            return sb.ToString();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {

            if(CheckInput()) {
                Server s = new MyShadowsocks.Model.Server();
                App.BoundServerList.Add(s);

                lstServerList.SelectedIndex = App.BoundServerList.Count - 1;

            } else {
                //error
            }
            lstServerList.Focus();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {

            int index = lstServerList.SelectedIndex;
            App.BoundServerList.RemoveAt(index);
            if(index >= App.BoundServerList.Count) --index;
            lstServerList.SelectedIndex = index;
            lstServerList.Focus();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e) {
            if(CheckInput()) {
                int index = lstServerList.SelectedIndex;
                var newServer = App.BoundServerList[index].Clone();
                App.BoundServerList.Insert(index + 1, newServer);
                //lstServerList.SelectedIndex = index + 1;
            }
            lstServerList.Focus();
        }

        private void SwapServerAt(int index1, int index2) {
            var s1 = App.BoundServerList[index1];
            App.BoundServerList[index1] = App.BoundServerList[index2];
            App.BoundServerList[index2] = s1;
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e) {
            int index = lstServerList.SelectedIndex;
            SwapServerAt(index, index + 1);
            lstServerList.SelectedIndex = index + 1;
            ReflushMoveBtn();
            lstServerList.Focus();
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e) {
            int index = lstServerList.SelectedIndex;
            SwapServerAt(index, index - 1);
            lstServerList.SelectedIndex = index - 1;
            ReflushMoveBtn();
            lstServerList.Focus();
        }



        private void lstServerList_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if(e.RemovedItems.Count > 0) {
                Server s = e.RemovedItems[0] as Server;
                if(!App.BoundServerList.Contains(s)) return; //BoundServerList.remove  导致的SelectionChanged事件
                if(!CheckServer(s)) {
                    lstServerList.SelectedValue = s;

                }

            }

        }

        //处理验证错误
        private void Grid_Error(object sender, ValidationErrorEventArgs e) {


            if(e.Action == ValidationErrorEventAction.Added) {

                if(e.Source == txtServerPort || e.Source == txtTimeout) {
                    //MessageBox.Show("请输入一个合适整数", "Inpput Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ReportError("请输入一个合适整数");
                } else {
                    //MessageBox.Show(e.Error.ErrorContent.ToString(), "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ReportError(e.Error.ErrorContent.ToString(),e.Error.Exception);
                }

            }
        }


        private async void StartController() {
            try {
                await App.Controller.Start();
            } catch(SocketException ex) {               
                    ReportError("StartController failed." , ex);
                
            }
        }

        private async void RestartController() {
            try {
                await App.Controller.Restart();
            } catch(SocketException ex) {
                ReportError("StartController failed.", ex);
            }
        }


        private void StopController() {
            App.Controller.Stop();
        }


        private void ReportError(string msg, Exception ex) {
            MessageBox.Show($"{msg} {ex.GetType()}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ReportError(string msg) {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }



        private void ckEnableProxy_Click(object sender, RoutedEventArgs e) {
            CheckBox source = e.Source as CheckBox;
            if(source.IsChecked == true) {
                StartController();
            } else {
                StopController();
            }
        }


 
        


        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if(CheckInput()) {
                UpdateLocalPort();
                UpdateServerList();


            } else {
                //check failed
                lstServerList.Focus();
            }

        }


        private void UpdateServerList() {
            
            App.Config.ServerList = new List<Server>(App.BoundServerList);
        }

        //目标改变时显式更新源
        private void UpdateLocalPort() {
            BindingExpression binding = txtLocalPort.GetBindingExpression(TextBox.TextProperty);
            if(binding.IsDirty) {
                binding.UpdateSource();
                RestartController();
            }
        }


        private bool CheckInput() {
            if(string.IsNullOrEmpty(txtPassword.Password)) {
                MessageBox.Show("Password cannot be empty.");
                return false;
            }
            if(Uri.CheckHostName(txtHostName.Text) == UriHostNameType.Unknown) {
                MessageBox.Show("Invalid hostname.");
                return false;
            }


            return true;
        }

        private bool CheckServer(Server s) {
            if(string.IsNullOrEmpty(s.Password)) {
                MessageBox.Show("Password cannot be empty.");
                return false;
            }
            if(Uri.CheckHostName(s.HostName) == UriHostNameType.Unknown) {
                MessageBox.Show("Invalid hostname.");
                return false;
            }


            return true;
        }



    }
}
