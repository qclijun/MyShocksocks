using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shadowsocks.Model;
using Shadowsocks.Controller;

namespace Shadowsocks.View
{
    public partial class ConfigForm : Form
    {
        private ShadowsocksController controller;

       private Configuration _modifiedConfiguration;
        private int _lastSelectedIndex = -1;
        public ConfigForm()
        {
            InitializeComponent();
        }

        public ConfigForm(ShadowsocksController controller)
        {
            this.Font = SystemFonts.MessageBoxFont;
            InitializeComponent();
            this.serversListBox.Dock = DockStyle.Fill;
            this.PerformLayout();

            this.controller = controller;
            
        }

        private void UpdateText()
        {

        }

        private void ShowWindow()
        {
            this.Opacity = 1;
            this.Show();
            IPTextBox.Focus();
        }

        private bool SaveOldSelectedServer()
        {
            //try
            //{
            //    if(_lastSelectedIndex==-1||_lastSelectedIndex>=_)
            //}
            return true;
        }
    }
}
