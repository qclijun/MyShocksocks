using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    class ResultEventArgs : EventArgs
    {
        public bool Success;
        public ResultEventArgs(bool success)
        {
            this.Success = success;
        }
    }
    class GFWListUpdater
    {
        public const string GFWLIST_URL = "https://raw.githubusercontent.com/gfwlist/gfwlist/master/gfwlist.txt";
        public const string GFWLIST_FILE = "gfwlist.txt";
        public const string USER_RULE_FILE = "user-rule.txt";
        public const string USER_ABP_FILE = "abp.txt";
        public const string PAC_FILE = "pac.txt";
        private static readonly IEnumerable<char> IgnoreLineBegins = new char[] { '!', '[' };

        private event EventHandler<ResultEventArgs> UpdateCompelted;
        private event ErrorEventHandler Error;
        private string currPath;

        public GFWListUpdater()
        {
            currPath = Directory.GetCurrentDirectory();
            UpdateCompelted = (object sender, ResultEventArgs args) =>
            {
                if (args.Success) Console.WriteLine("Update completed success.");
                else Console.WriteLine("Update failed" );
            };
        }

        private void HttpDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                File.WriteAllText(Path.Combine(currPath,GFWLIST_FILE), e.Result, Encoding.UTF8);
                List<string> lines = ParseResult(e.Result);
                if (File.Exists(USER_RULE_FILE))
                {
                    string local = File.ReadAllText(USER_RULE_FILE, Encoding.UTF8);
                    using (var sr = new StringReader(local))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Trim().Length==0) continue;
                            if (IgnoreLineBegins.Contains(line[0])) continue;
                            lines.Add(line);
                        }
                    }
                }
                string abpContent="";
                if (File.Exists(USER_ABP_FILE))
                {
                    abpContent = File.ReadAllText(USER_ABP_FILE, Encoding.UTF8);
                }
                else
                {
                    //abpContent = Utils.UnGzip(Resources.abp_js);
                    abpContent = Unzip("abp.js.gz");
                }
                abpContent = abpContent.Replace("__RULES__", JsonConvert.SerializeObject(lines, Formatting.Indented));
                if (File.Exists(PAC_FILE))
                {
                    string original = File.ReadAllText(PAC_FILE, Encoding.UTF8);
                    if (original == abpContent)
                    {
                        UpdateCompelted(this, new ResultEventArgs(false));
                        return;
                    }
                }
                File.WriteAllText(PAC_FILE, abpContent, Encoding.UTF8);
                if (UpdateCompelted != null)
                {
                    UpdateCompelted(this, new ResultEventArgs(true));
                }
            }
            catch (Exception ex)
            {
                //if (Error != null) Error(this, new ErrorEventArgs(ex));
                Console.WriteLine(ex);
            }
        }

        public void UpdatePACFromGFWList(/*Configuration config*/)
        {
            WebClient http = new WebClient();
            http.Proxy = new WebProxy(IPAddress.Loopback.ToString(), 1088);
            http.DownloadStringCompleted += HttpDownloadStringCompleted;
            http.DownloadStringAsync(new Uri(GFWLIST_URL));
        }

        private static List<string> ParseResult(string result)
        {
            byte[] bytes = Convert.FromBase64String(result);
            string content = Encoding.ASCII.GetString(bytes);
            List<string> valid_line = new List<string>();
            using (var sr = new StringReader(content))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Trim().Length == 0) continue;
                    if (IgnoreLineBegins.Contains(line[0])) continue;
                  
                    valid_line.Add(line);
                }
            }
            return valid_line;
        }

        public static string Unzip(string zipfileName)
        {
            MemoryStream dest = new MemoryStream();
            using(var input = new GZipStream(File.Open(zipfileName, FileMode.Open, FileAccess.Read),
                CompressionMode.Decompress,false))
            {
                input.CopyTo(dest);
            }
            return Encoding.UTF8.GetString(dest.ToArray());
        }
    }
}
