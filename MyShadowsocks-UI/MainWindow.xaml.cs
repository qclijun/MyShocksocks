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
using System.Timers;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MyShadowsocks_UI {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        //与UI绑定, 与Config.ServerList中的元素指向同一个Server对象
        //也就是说，对现有的元素进行修改会同时改变Config.ServerList
        //但是添加和删除元素不会同步
        public IList<Server> BoundServerList {
            get; private set;
        }

        public MainWindow() {
            InitializeComponent();
            LoadConfiguration();
            MyShadowsocksController.ServersChanged += Controller_Timer_ServersUpdated;
        }

        private void LoadConfiguration() {

            BoundServerList = new ObservableCollection<Server>(App.ServerList);

            gridConfiguration.DataContext = App.Config; //data binding            
            cBoxMethod.ItemsSource = MyShadowsocksController.SuppportedEncryptMethods;
            lstServerList.ItemsSource = BoundServerList;
            lstServerList.SelectedIndex = App.Config.Index;
            lstServerList.Focus();
            ReflushMoveBtn();
        }

        //MainWindow即是ServesChanged事件的订阅者，也是发送者
        private void Controller_Timer_ServersUpdated(object sender, EventArgs e) {
            //如果不是Timer引起的，则返回
            if(!object.ReferenceEquals(sender, typeof(System.Threading.Timer))) {
                Debug.Assert(object.ReferenceEquals(sender, this.GetType())); //由它自己引起的，则直接返回
                return;
            }

            this.Dispatcher.BeginInvoke((ThreadStart)delegate () {
                BoundServerList.Clear();
                foreach(var s in App.Config.ServerList) {
                    BoundServerList.Add(s);
                }
                lstServerList.SelectedIndex = App.Config.Index;

            });
        }

        private void ReflushMoveBtn() {
            if(lstServerList.SelectedIndex == 0) btnMoveUp.IsEnabled = false;
            else btnMoveUp.IsEnabled = true;
            if(lstServerList.SelectedIndex == BoundServerList.Count - 1) btnMoveDown.IsEnabled = false;
            else btnMoveDown.IsEnabled = true;
        }



        private void btnAdd_Click(object sender, RoutedEventArgs e) {

            if(CheckInput()) {
                Server s = new MyShadowsocks.Model.Server();
                BoundServerList.Add(s);

                lstServerList.SelectedIndex = BoundServerList.Count - 1;

            } else {
                //error
            }
            lstServerList.Focus();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {

            int index = lstServerList.SelectedIndex;
            BoundServerList.RemoveAt(index);
            if(index >= BoundServerList.Count) --index;
            lstServerList.SelectedIndex = index;
            lstServerList.Focus();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e) {
            if(CheckInput()) {
                int index = lstServerList.SelectedIndex;
                var newServer = BoundServerList[index].Clone();
                BoundServerList.Insert(index + 1, newServer);
                //lstServerList.SelectedIndex = index + 1;
            }
            lstServerList.Focus();
        }

        private void SwapServerAt(int index1, int index2) {
            var s1 = BoundServerList[index1];
            BoundServerList[index1] = BoundServerList[index2];
            BoundServerList[index2] = s1;
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
                if(!BoundServerList.Contains(s)) return; //BoundServerList.remove  导致的SelectionChanged事件
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
                    App.ReportError("请输入一个合适整数");
                } else {
                    //MessageBox.Show(e.Error.ErrorContent.ToString(), "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.ReportError(e.Error.ErrorContent.ToString(), e.Error.Exception);
                }

            }
        }











        private void ckEnableProxy_Click(object sender, RoutedEventArgs e) {
            CheckBox source = e.Source as CheckBox;
            if(source.IsChecked == true) {
                App.StartController();
            } else {
                App.StopController();
            }
        }






        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if(CheckInput()) {
                UpdateLocalPort();
                UpdateServerList();
                this.Close();
            } else {
                //check failed
                lstServerList.Focus();
            }

        }


        private void UpdateServerList() {
            App.Config.ServerList = new List<Server>(BoundServerList);

            //点击OK按钮时引发ServersChanged事件
            MyShadowsocksController.RaiseServersChangedEvent(this.GetType());
        }

        //目标改变时显式更新源（只在点击OK时更新）
        private void UpdateLocalPort() {
            BindingExpression binding = txtLocalPort.GetBindingExpression(TextBox.TextProperty);
            if(binding.IsDirty) {
                binding.UpdateSource();
                if(App.Controller.IsRunning) {
                    App.RestartController();
                }

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

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
