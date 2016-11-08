using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shadowsocks.Controller;

namespace Shadowsocks.Model
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

        public string server;
        public int server_port;
        public string password;
        public string method;
        public string remarks;
        public bool auth;
        public int timeout;

        public override int GetHashCode()
        {
            return server.GetHashCode() ^ server_port;
        }

        public override bool Equals(object obj)
        {
            Server o2 = (Server)obj;
            return server == o2.server && server_port == o2.server_port;
        }

        public string FriendlyName()
        {
            if (string.IsNullOrEmpty(server))
            {
                return I18N.GetString("New server");
       
            }
            string serverStr;

            var hostType = Uri.CheckHostName(server);
            if (hostType == UriHostNameType.Unknown)
                throw new FormatException("Invalid Server Address.");
            switch (hostType)
            {
                case UriHostNameType.IPv6:
                    serverStr = $"[{server}]:{server_port}";
                    break;
                default:
                    serverStr = $"{server}:{server_port}";
                    break;
            }
            return string.IsNullOrEmpty(remarks) ? serverStr :
                $"{remarks}({serverStr})";
        }

        public Server()
        {
            server = "";
            server_port = 8388;
            method = "aes-256-cfb";
            password = "";
            remarks = "";
            auth = false;
            timeout = DefaultServerTimeoutSec;
    
        }
        public Server(string ssURL) : this()
        {
            var match = UrlFinder.Match(ssURL);
            if (!match.Success) throw new FormatException();
            var base64 = match.Groups[1].Value;
            match = DetailsParser.Match(Encoding.UTF8.GetString(Convert.FromBase64String(
                base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='))));
            method = match.Groups["method"].Value;
            auth = match.Groups["auth"].Success;
            password = match.Groups["password"].Value;
            server = match.Groups["hostname"].Value;
            server_port = int.Parse(match.Groups["port"].Value);
        }

        public string Identifier()
        {
            return $"{server}:{server_port}";
        }
    }
}
