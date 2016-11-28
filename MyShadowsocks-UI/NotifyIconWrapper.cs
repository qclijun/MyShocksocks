using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using MyShadowsocks.Controller;
using MyShadowsocks.Properties;

namespace MyShadowsocks_UI {
    public partial class NotifyIconWrapper : Component {
        private Bitmap icon_baseBitmap;
        private Icon icon_base, icon_in, icon_out, icon_both, targetIcon;
        private App app;




        public NotifyIconWrapper(IContainer container) : this() {
            container.Add(this);

            InitializeComponent();
        }

        public NotifyIconWrapper(App app) : this() {
            this.app = app;

            if(app.IsFirstRun) {
                app.ShowConfigWindow();
            }





        }


        public NotifyIconWrapper() {
            InitializeComponent();
            LoadConfig();


            UpdateTrayIcon();
            AttachEvent();



        }

        private void LoadConfig() {
            var Config = App.Config;

            enableItem.Checked = Config.Enabled;
            modeItem.Enabled = Config.Enabled;
            globalModeItem.Checked = Config.Global;
            pacModeItem.Checked = !Config.Global;
            allowClientsItem.Checked = Config.ShareOverLan;
            verboseLogItem.Checked = Config.IsVerboseLogging;

            onlinePacItem.Checked = onlinePacItem.Enabled && Config.UseOnlinePac;
            localPacItem.Checked = !onlinePacItem.Checked;
            UpdatePACItemEnabledStatus();
            UpdateServerMenuItem();


            startOnBootItem.Checked = AutoStartup.Check();
            checkUpdatesAtStartItem.Checked = Config.AuthCheckUpdate;
        }

        private void UpdatePACItemEnabledStatus() {
            if(localPacItem.Checked) {
                editLocalPACItem.Enabled = true;
                updateLocalPACItem.Enabled = true;
                editUserRuleItem.Enabled = true;
                editOnlinePACItem.Enabled = false;
            } else {
                editLocalPACItem.Enabled = false;
                updateLocalPACItem.Enabled = false;
                editUserRuleItem.Enabled = false;
                editOnlinePACItem.Enabled = true;
            }
        }
        private void UpdateServerMenuItem() {
            var items = this.serversItem.DropDownItems;

            //删除toolStripSeparator_special前面的ToolStripItem
            while(items[0] != this.toolStripSeparator_special) {
                items.RemoveAt(0);
            }
            int i = 0;

            //add strategy
            foreach(var strategy in MyShadowsocksController.SupportedServerSelectors) {
                var item = new ToolStripMenuItem(strategy);
                item.Tag = "strategy";
                item.Click += AStrategyItem_Click;
                if(strategy == App.Config.Strategy) item.Checked = true;
                items.Insert(i++, item);
            }
            items.Insert(i++, new ToolStripSeparator());

            //add server

            for(int index = 0;index < App.ServerList.Count;++index) {
                var s = App.ServerList[index];
                var item = new ToolStripMenuItem(s.ToString());
                item.Tag = "server_" + index.ToString();
                item.Enabled = (App.Config.Strategy == "Fixed");
                item.Checked = (index == App.Config.Index);
                item.Click += AServer_Click;
                items.Insert(i++, item);

            }


        }

        private void AServer_Click(object sender, EventArgs e) {


            var item = sender as ToolStripMenuItem;
            if(item.Checked) return;

            var tagString = item.Tag as string;
            int i = tagString.IndexOf('_');
            int index = int.Parse(tagString.Substring(i + 1));
            App.Config.Index = index;
            MyShadowsocksController.SetStrategy("Fixed");

            foreach(ToolStripItem it in serversItem.DropDownItems) {
                if(it.Tag == null) continue;
                var tagStr = it.Tag as string;
                if(tagStr.StartsWith("server")) {
                    (it as ToolStripMenuItem).Checked = tagString == tagStr;
                }

            }

        }

        private void AStrategyItem_Click(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            if(item.Checked) return;

            string strategy = item.Text;
            
            MyShadowsocksController.SetStrategy(strategy);

            foreach(ToolStripItem it in serversItem.DropDownItems) {
                if(it.Tag == null) continue;
                var tagString = it.Tag as string;
                if(tagString.StartsWith("server")) {
                    it.Enabled = strategy == "Fixed";
                } else if(tagString == "strategy") {
                    ToolStripMenuItem it2 = it as ToolStripMenuItem;
                    it2.Checked = (it2.Text == App.Config.Strategy);
                }
            }


        }

        private void AttachEvent() {

            enableItem.Click += EnableItem_Click;
            pacModeItem.Click += ProxyModeItem_Click;
            globalModeItem.Click += ProxyModeItem_Click;

            exitItem.Click += ExitItem_Click;
            aboutItem.Click += AboutItem_Click;
        }



        private void ProxyModeItem_Click(object sender, EventArgs e) {
            Contract.Requires(sender == pacModeItem || sender == globalModeItem);

            bool isGlobal = sender == globalModeItem;
            if(App.Config.Global == isGlobal) return;
            pacModeItem.Checked = !pacModeItem.Checked;
            globalModeItem.Checked = !globalModeItem.Checked;

            App.Controller.SetMode(isGlobal);
            
        }

        private void EnableItem_Click(object sender, EventArgs e) {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if(item.Checked) {
                item.Checked = false;
                modeItem.Enabled = false;
                App.Config.Enabled = false;
                App.StopController();
                
            } else {
                item.Checked = true;
                modeItem.Enabled = true;
                App.Config.Enabled = true;
                App.StartController();
                
            }
        }

        private void AboutItem_Click(object sender, EventArgs e) {
            Process.Start("https://github.com/shadowsocks/shadowsocks-windows");
        }




        private void ExitItem_Click(object sender, EventArgs e) {

            app.Shutdown();

        }





        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e) {

        }

        private void notifyIcon1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) {
            if(e.Button == System.Windows.Forms.MouseButtons.Left) {
                app.ShowConfigWindow();
            }
        }

        private void UpdateTrayIcon() {
            int dpi;
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
            dpi = (int)graphics.DpiX;
            graphics.Dispose();

            Uri iconUri;

            icon_baseBitmap = null;
            if(dpi <= 96) {
                iconUri = new Uri("Data/ss16.png", UriKind.Relative);

            } else if(dpi <= 120) {
                iconUri = new Uri("Data/ss20.png", UriKind.Relative);

            } else {
                iconUri = new Uri("Data/ss24.png", UriKind.Relative);

            }
            icon_baseBitmap = new Bitmap(System.Windows.Application.GetResourceStream(iconUri).Stream);

            iconUri = new Uri("Data/ssIn24.png", UriKind.Relative);
            Bitmap ssIn24Bitmap = new Bitmap(System.Windows.Application.GetResourceStream(iconUri).Stream);
            iconUri = new Uri("Data/ssOut24.png", UriKind.Relative);
            Bitmap ssOut24Bitmap = new Bitmap(System.Windows.Application.GetResourceStream(iconUri).Stream);





            icon_base = Icon.FromHandle(icon_baseBitmap.GetHicon());
            targetIcon = icon_base;
            icon_in = Icon.FromHandle(AddBitmapOverlay(icon_baseBitmap, ssIn24Bitmap).GetHicon());
            icon_out = Icon.FromHandle(AddBitmapOverlay(icon_baseBitmap, ssOut24Bitmap).GetHicon());
            icon_both = Icon.FromHandle(AddBitmapOverlay(icon_baseBitmap, ssIn24Bitmap, ssOut24Bitmap).GetHicon());
            notifyIcon1.Icon = targetIcon;

            notifyIcon1.Text = "Hello MyShadowsocks";
        }


        private Bitmap AddBitmapOverlay(Bitmap original, params Bitmap[] overlays) {
            Bitmap bitmap = new Bitmap(original.Width, original.Height, PixelFormat.Format64bppArgb);
            Graphics canvas = Graphics.FromImage(bitmap);
            canvas.DrawImage(original, new System.Drawing.Point(0, 0));
            foreach(Bitmap overlay in overlays) {
                canvas.DrawImage(new Bitmap(overlay, original.Size), new System.Drawing.Point(0, 0));
            }
            canvas.Save();
            return bitmap;
        }






    }
}
