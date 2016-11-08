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

    public class GFWListUpdater
    {
        public class ResultEventArgs : EventArgs
        {
            public bool Success;
            public ResultEventArgs(bool success)
            {
                this.Success = success;
            }
        }


        private const string GFWLIST_URL = "https://raw.githubusercontent.com/gfwlist/gfwlist/master/gfwlist.txt";
        private const string GFWLIST_FILE = "gfwlist.txt";
        private static readonly IEnumerable<char> IgnoreLineBegins = new char[] { '!', '[' };

        private event EventHandler<ResultEventArgs> UpdateComplted;
        private event ErrorEventHandler Error;

        private void HttpDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                File.WriteAllText(Utils.GetTempPath(GFWLIST_FILE), e.Result, Encoding.UTF8);
                List<string> lines = ParseResult(e.Result);
                if (File.Exists(PACServer.USER_RULE_FILE))
                {
                    string[] strs = File.ReadAllLines(PACServer.USER_RULE_FILE, Encoding.UTF8);
                    foreach(var line in strs)
                    {
                        if (line.IsWhiteSpace()) continue;
                        if (line.BeginWithAny(IgnoreLineBegins)) continue;
                        lines.Add(line);
                    }                  
                }
                string abpContent;
                if (File.Exists(PACServer.USER_ABP_FILE))
                {
                    abpContent = File.ReadAllText(PACServer.USER_ABP_FILE, Encoding.UTF8);
                }
                else
                {
                    abpContent = Utils.UnGzip(Resources.abp_js);
                }
                abpContent = abpContent.Replace("__RULES__", JsonConvert.SerializeObject(lines, Formatting.Indented));
                if (File.Exists(PACServer.PAC_FILE))
                {
                    string original = File.ReadAllText(PACServer.PAC_FILE, Encoding.UTF8);
                    if (original == abpContent)
                    {
                        UpdateComplted(this, new ResultEventArgs(false));
                        return;
                    }
                }
                File.WriteAllText(PACServer.PAC_FILE, abpContent, Encoding.UTF8);
                if (UpdateComplted != null)
                {
                    UpdateComplted(this, new ResultEventArgs(true));
                }
            }catch(Exception ex)
            {
                if (Error != null) Error(this, new ErrorEventArgs(ex));
            }
        }

        public  void UpdatePACFromGFWList(int localPort)
        {
            WebClient http = new WebClient();
            http.Proxy = new WebProxy(IPAddress.Loopback.ToString(),localPort);
            http.DownloadStringCompleted += HttpDownloadStringCompleted;
            http.DownloadStringAsync(new Uri(GFWLIST_URL));
        }

        private static List<string> ParseResult(string result)
        {
            byte[] bytes = Convert.FromBase64String(result);
            string content = Encoding.ASCII.GetString(bytes);
            List<string> valid_line = new List<string>();
            using(var sr = new StringReader(content))
            {
                string line;
                while((line=sr.ReadLine())!=null)
                {
                    if (line.IsWhiteSpace()) continue;
                    if (line.BeginWithAny(IgnoreLineBegins)) continue;
                    valid_line.Add(line);
                }
            }
            return valid_line;
        }
    }
}
