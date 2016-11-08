using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shadowsocks.Model;

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
                    //WinINet
                }
            }
        }
        
    }
}
