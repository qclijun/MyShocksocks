using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Controller
{
    public class I18N
    {
        protected static Dictionary<string, string> Strings;

        static void Init(string res)
        {
            using (var sr = new StringReader(res))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Trim().Length == 0) continue;
                    if (line[0] == '#') continue;
                    var pos = line.IndexOf('=');
                    if (pos < 1) continue;
                    Strings[line.Substring(0, pos)] = line.Substring(pos + 1);
                }
            }
        }

        static I18N()
        {
            Strings = new Dictionary<string, string>();
            string name = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (name.StartsWith("zh"))
            {
                if (name == "zh" || name == "zh-CN")
                {
                    Init(Shadowsocks.Properties.Resources.cn);
                }
                else
                {
                    Init(Shadowsocks.Properties.Resources.zh_tw);
                }
            }
        }

        public static string GetString(string key)
        {
            if (Strings.ContainsKey(key))
            {
                return Strings[key];
            }
            else
            {
                return key;
            }
        }

    }
}
