using System;

namespace MyShadowsocks_UI {
    partial class NotifyIconWrapper {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing) {

            if(disposing && (components != null)) {
                components.Dispose();
            }
            
            base.Dispose(disposing);
            
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.enableItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pacModeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.globalModeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.serversItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_special = new System.Windows.Forms.ToolStripSeparator();
            this.configItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statConfigItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showQRCodeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ScanQRCodeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pacItem = new System.Windows.Forms.ToolStripMenuItem();
            this.localPacItem = new System.Windows.Forms.ToolStripMenuItem();
            this.onlinePacItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.editLocalPACItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateLocalPACItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editUserRuleItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editOnlinePACItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.startOnBootItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowClientsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.showLogsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verboseLogItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updatesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkUpdatesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.checkUpdatesAtStartItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.exitItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableItem,
            this.modeItem,
            this.toolStripSeparator1,
            this.serversItem,
            this.pacItem,
            this.toolStripSeparator2,
            this.startOnBootItem,
            this.allowClientsItem,
            this.toolStripSeparator5,
            this.showLogsItem,
            this.verboseLogItem,
            this.updatesItem,
            this.aboutItem,
            this.toolStripSeparator7,
            this.exitItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(235, 314);
            this.contextMenuStrip1.Text = "MyShadowsocks";
            // 
            // enableItem
            // 
            this.enableItem.Name = "enableItem";
            this.enableItem.Size = new System.Drawing.Size(234, 26);
            this.enableItem.Text = "启用代理";
            // 
            // modeItem
            // 
            this.modeItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pacModeItem,
            this.globalModeItem});
            this.modeItem.Name = "modeItem";
            this.modeItem.Size = new System.Drawing.Size(234, 26);
            this.modeItem.Text = "代理模式";
            // 
            // pacModeItem
            // 
            this.pacModeItem.Name = "pacModeItem";
            this.pacModeItem.Size = new System.Drawing.Size(144, 26);
            this.pacModeItem.Text = "PAC模式";
            // 
            // globalModeItem
            // 
            this.globalModeItem.Name = "globalModeItem";
            this.globalModeItem.Size = new System.Drawing.Size(144, 26);
            this.globalModeItem.Text = "全局模式";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(231, 6);
            // 
            // serversItem
            // 
            this.serversItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator_special,
            this.configItem,
            this.statConfigItem,
            this.showQRCodeItem,
            this.ScanQRCodeItem});
            this.serversItem.Name = "serversItem";
            this.serversItem.Size = new System.Drawing.Size(234, 26);
            this.serversItem.Text = "服务器";
            // 
            // toolStripSeparator_special
            // 
            this.toolStripSeparator_special.Name = "toolStripSeparator_special";
            this.toolStripSeparator_special.Size = new System.Drawing.Size(228, 6);
            // 
            // configItem
            // 
            this.configItem.Name = "configItem";
            this.configItem.Size = new System.Drawing.Size(231, 26);
            this.configItem.Text = "编辑服务器...";
            // 
            // statConfigItem
            // 
            this.statConfigItem.Name = "statConfigItem";
            this.statConfigItem.Size = new System.Drawing.Size(231, 26);
            this.statConfigItem.Text = "统计配置...";
            // 
            // showQRCodeItem
            // 
            this.showQRCodeItem.Name = "showQRCodeItem";
            this.showQRCodeItem.Size = new System.Drawing.Size(231, 26);
            this.showQRCodeItem.Text = "显示二维码...";
            // 
            // ScanQRCodeItem
            // 
            this.ScanQRCodeItem.Name = "ScanQRCodeItem";
            this.ScanQRCodeItem.Size = new System.Drawing.Size(231, 26);
            this.ScanQRCodeItem.Text = "扫描屏幕上的二维码...";
            // 
            // pacItem
            // 
            this.pacItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.localPacItem,
            this.onlinePacItem,
            this.toolStripSeparator4,
            this.editLocalPACItem,
            this.updateLocalPACItem,
            this.editUserRuleItem,
            this.editOnlinePACItem});
            this.pacItem.Name = "pacItem";
            this.pacItem.Size = new System.Drawing.Size(234, 26);
            this.pacItem.Text = "PAC";
            // 
            // localPacItem
            // 
            this.localPacItem.Name = "localPacItem";
            this.localPacItem.Size = new System.Drawing.Size(248, 26);
            this.localPacItem.Text = "使用本地PAC";
            // 
            // onlinePacItem
            // 
            this.onlinePacItem.Name = "onlinePacItem";
            this.onlinePacItem.Size = new System.Drawing.Size(248, 26);
            this.onlinePacItem.Text = "使用在线PAC";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(245, 6);
            // 
            // editLocalPACItem
            // 
            this.editLocalPACItem.Name = "editLocalPACItem";
            this.editLocalPACItem.Size = new System.Drawing.Size(248, 26);
            this.editLocalPACItem.Text = "编辑本地PAC文件...";
            // 
            // updateLocalPACItem
            // 
            this.updateLocalPACItem.Name = "updateLocalPACItem";
            this.updateLocalPACItem.Size = new System.Drawing.Size(248, 26);
            this.updateLocalPACItem.Text = "从GFWList更新本地PAC";
            // 
            // editUserRuleItem
            // 
            this.editUserRuleItem.Name = "editUserRuleItem";
            this.editUserRuleItem.Size = new System.Drawing.Size(248, 26);
            this.editUserRuleItem.Text = "编辑GFWList用户规则...";
            // 
            // editOnlinePACItem
            // 
            this.editOnlinePACItem.Name = "editOnlinePACItem";
            this.editOnlinePACItem.Size = new System.Drawing.Size(248, 26);
            this.editOnlinePACItem.Text = "编辑在线PAC网址...";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(231, 6);
            // 
            // startOnBootItem
            // 
            this.startOnBootItem.Name = "startOnBootItem";
            this.startOnBootItem.Size = new System.Drawing.Size(234, 26);
            this.startOnBootItem.Text = "开机启动";
            // 
            // allowClientsItem
            // 
            this.allowClientsItem.Name = "allowClientsItem";
            this.allowClientsItem.Size = new System.Drawing.Size(234, 26);
            this.allowClientsItem.Text = "允许来自局域网的连接";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(231, 6);
            // 
            // showLogsItem
            // 
            this.showLogsItem.Name = "showLogsItem";
            this.showLogsItem.Size = new System.Drawing.Size(234, 26);
            this.showLogsItem.Text = "显示日志...";
            // 
            // verboseLogItem
            // 
            this.verboseLogItem.Name = "verboseLogItem";
            this.verboseLogItem.Size = new System.Drawing.Size(234, 26);
            this.verboseLogItem.Text = "详细记录日志";
            // 
            // updatesItem
            // 
            this.updatesItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkUpdatesItem,
            this.toolStripSeparator6,
            this.checkUpdatesAtStartItem});
            this.updatesItem.Name = "updatesItem";
            this.updatesItem.Size = new System.Drawing.Size(234, 26);
            this.updatesItem.Text = "更新...";
            // 
            // checkUpdatesItem
            // 
            this.checkUpdatesItem.Name = "checkUpdatesItem";
            this.checkUpdatesItem.Size = new System.Drawing.Size(189, 26);
            this.checkUpdatesItem.Text = "检查更新...";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(186, 6);
            // 
            // checkUpdatesAtStartItem
            // 
            this.checkUpdatesAtStartItem.Name = "checkUpdatesAtStartItem";
            this.checkUpdatesAtStartItem.Size = new System.Drawing.Size(189, 26);
            this.checkUpdatesAtStartItem.Text = "启动时检查更新";
            // 
            // aboutItem
            // 
            this.aboutItem.Name = "aboutItem";
            this.aboutItem.Size = new System.Drawing.Size(234, 26);
            this.aboutItem.Text = "关于...";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(231, 6);
            // 
            // exitItem
            // 
            this.exitItem.Name = "exitItem";
            this.exitItem.Size = new System.Drawing.Size(234, 26);
            this.exitItem.Text = "退出";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Text = "MyShadowsocks";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.BalloonTipClicked += new System.EventHandler(this.notifyIcon1_BalloonTipClicked);
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            this.contextMenuStrip1.ResumeLayout(false);

        }

        internal void ShowFirstTimeBalloon() {
            this.notifyIcon1.BalloonTipTitle = "Shadowsocks is here";
            this.notifyIcon1.BalloonTipText = "You can turn on/off Shadowsocks in the context menu";
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.ShowBalloonTip(0);
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem enableItem;
        private System.Windows.Forms.ToolStripMenuItem modeItem;
        private System.Windows.Forms.ToolStripMenuItem pacModeItem;
        private System.Windows.Forms.ToolStripMenuItem globalModeItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem serversItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator_special;
        private System.Windows.Forms.ToolStripMenuItem configItem;
        private System.Windows.Forms.ToolStripMenuItem statConfigItem;
        private System.Windows.Forms.ToolStripMenuItem showQRCodeItem;
        private System.Windows.Forms.ToolStripMenuItem ScanQRCodeItem;
        private System.Windows.Forms.ToolStripMenuItem pacItem;
        private System.Windows.Forms.ToolStripMenuItem localPacItem;
        private System.Windows.Forms.ToolStripMenuItem onlinePacItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem updateLocalPACItem;
        private System.Windows.Forms.ToolStripMenuItem editUserRuleItem;
        private System.Windows.Forms.ToolStripMenuItem editOnlinePACItem;
        private System.Windows.Forms.ToolStripMenuItem startOnBootItem;
        private System.Windows.Forms.ToolStripMenuItem allowClientsItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem showLogsItem;
        private System.Windows.Forms.ToolStripMenuItem verboseLogItem;
        private System.Windows.Forms.ToolStripMenuItem updatesItem;
        private System.Windows.Forms.ToolStripMenuItem checkUpdatesItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem checkUpdatesAtStartItem;
        private System.Windows.Forms.ToolStripMenuItem aboutItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem editLocalPACItem;
    }
}
