using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Model
{
    [Serializable]
    public class ProxyConfiguration
    {
        public const int PROXY_SOCKS5 = 0;
        public const int PROXY_HTTP = 1;
        public const int MaxProxyTimeoutSec = 10;

        private const int DefaultProxyTimeoutset = 3;

        private bool useProxy;
        private int proxyType;
        private string proxyServer;
        private int proxyPort;
        private int proxyTimeout;

        public int ProxyType { get; set; }

        public ProxyConfiguration()
        {
            useProxy = false;
            proxyType = PROXY_SOCKS5;
            proxyServer = "";
            proxyPort = 0;
            proxyTimeout = DefaultProxyTimeoutset;
        }
    }
}
