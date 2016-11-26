using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Model
{
    [Serializable]
    public class ProxyConfiguration
    {
        public enum ProxyType
        {
            SOCKS5 = 0,
            HTTP = 1,
        }

        //public const int PROXY_SOCKS5 = 0;
        //public const int PROXY_HTTP = 1;

        public const int MaxProxyTimeoutSec = 10;

        private const int DefaultProxyTimeoutsec = 3;

        public bool UseProxy { get; set; } = false;
        public ProxyType Type { get; set; } = ProxyType.SOCKS5;
        public string ProxyServer { get; set; } = "";
        public int ProxyPort { get; set; } = 0;
        public int ProxyTimeout { get; set; } = DefaultProxyTimeoutsec;



        public ProxyConfiguration()
        {
           
        }

        public ProxyConfiguration Clone() {
            return MemberwiseClone() as ProxyConfiguration;
        }

    }
}
