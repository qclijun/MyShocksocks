using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Shadowsocks.Properties;
using Shadowsocks.Util;
using Newtonsoft.Json;
using Shadowsocks.Model;

namespace Shadowsocks.Controller
{

    public class GfwUpdater
    {

        public const string DEFAULT_GFWLIST_URI = "https://raw.githubusercontent.com/gfwlist/gfwlist/master/gfwlistjjj.txt";
        public const string GFWLIST_FILE = "gfwlist.txt";
        public static readonly string GfwListFilePath = Path.Combine(Path.GetTempPath(), GFWLIST_FILE);

        public event EventHandler<EventArgs> GfwFileChanged;


        public GfwUpdater()
        {

        }



        private void HttpDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null) throw new WebException("Cannot download gfwlist", e.Error);
            string gfwlist = e.Result;      
            if (File.Exists(GfwListFilePath))
            {
                string localContent = File.ReadAllText(GfwListFilePath);
                if (localContent == gfwlist) return;
            }
            UpdateLocalGfwFile(gfwlist);
        }

        private void UpdateLocalGfwFile(string newContent)
        {

            File.WriteAllText(GfwListFilePath, newContent);
            if (GfwFileChanged != null)
            {
                GfwFileChanged(this, EventArgs.Empty);
            }
        }

        //Download gfwlist to GfwListFilePath
        //不是异步，可能会阻塞线程
        public static void DownloadGfwList(string gfwListUri)
        {
            WebClient http = new WebClient();
            http.DownloadFile(new Uri(gfwListUri), GfwListFilePath);
        }

        public static void DownloadGfwList()
        {
            DownloadGfwList(DEFAULT_GFWLIST_URI);
        }

        public void UpdateGfwListFromUri(string gfwListUri)
        {
            WebClient http = new WebClient();
            //http.Proxy = new WebProxy(IPAddress.Loopback.ToString(), localPort);
            http.DownloadStringCompleted += HttpDownloadStringCompleted;
            http.DownloadStringAsync(new Uri(gfwListUri));
        }

        public void UpdateGfwListFromUri()
        {
            UpdateGfwListFromUri(DEFAULT_GFWLIST_URI);
        }
    }
}
