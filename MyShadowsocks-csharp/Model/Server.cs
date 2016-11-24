using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyShadowsocks.Controller;

namespace MyShadowsocks.Model
{
    [Serializable]
    public class Server
    {
        public static readonly Regex
            UrlFinder = new Regex("^ss://((?:[A-Za-z0-9_/]+)|((?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?))$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase),
            DetailsParser = new Regex("^((?<method>.+?)(?<auth>-auth)??:(?<password>.*)@(?<hostname>.+?)" +
            ":(?<port>\\d+?))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const int DefaultServerTimeoutSec = 5;
        public const int MaxServerTimeoutSec = 20;

        public string ServerName { get; set; } = "";
        public int ServerPort { get; set; } = 8388;
        public string Password { get; set; } = "";
        public string Method { get; set; } = "aes-256-cfb";
        public string Remarks { get; set; } = "";
        public bool Auth { get; set; } = false;
        public int Timeout { get; set; } = DefaultServerTimeoutSec;

 

        public string FriendlyName()
        {
            if (string.IsNullOrEmpty(ServerName))
            {
                return I18N.GetString("New server");
       
            }
            return string.IsNullOrEmpty(Remarks) ? ToString() : $"{Remarks}({ToString()})";
        }

        


        public Server()
        {
          
        }

        public Server(string ssURL) : this()
        {
            var match = UrlFinder.Match(ssURL);
            if (!match.Success) throw new FormatException($"Invalid shadowsocks URL: {ssURL}");
            var base64 = match.Groups[1].Value;
            match = DetailsParser.Match(Encoding.UTF8.GetString(Convert.FromBase64String(
                base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='))));
            Method = match.Groups["method"].Value;
            Auth = match.Groups["auth"].Success;
            Password = match.Groups["password"].Value;
            ServerName = match.Groups["hostname"].Value;
            ServerPort = int.Parse(match.Groups["port"].Value);
        }

        public string Identifier()
        {
            return ToString();
        }

        public override string ToString() {
            return ServerName + ":" + ServerPort;
        }


        public Server Clone() {
            return (Server)this.MemberwiseClone();
        }


        public static List<Server> CloneList(List<Server> list) {
            List<Server> ret = new List<Server>(list.Count);
            foreach(var s in list) {
                ret.Add((Server)s.MemberwiseClone());
            }
            
            return ret;
        }


    }
}
