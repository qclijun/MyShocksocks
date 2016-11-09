using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shadowsocks.Model;
using Junlee.Util.SystemProxy;

namespace Shadowsocks.Controller
{
    public static class SystemProxy
    {
        private static string GetTimestamp(DateTime value)
            => value.ToString("yyyyMMddHHmmssfff");

        public static void Update(Configuration config, bool forceDisable)
        {
            
            bool global = config.Global;
            bool enabled = config.Enabled;
            if (forceDisable) enabled = false;
            if (enabled)
            {
                if (global)
                {
                    WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_Direct, $"127.0.0.1:{config.LocalPort}", null);
                }
                else
                {
                    string pacUrl;
                    if (config.UseOnlinePac && !string.IsNullOrEmpty(config.PacUrl))
                        pacUrl = config.PacUrl;
                    else
                        pacUrl = $"http://127.0.0.1:{config.LocalPort}/pac?t={GetTimestamp(DateTime.Now)}";
                    WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_PAC, null, pacUrl);
                }
            }
            else //no proxy
            {
                WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_None, null, null);
            }
        }
        
    }
}
