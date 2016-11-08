using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Junlee.Util.SystemProxy;
//using Shadowsocks.Util.SystemProxy;

namespace Sample
{
     enum ProxyType
    {
        Socks5=0,
        Http=1,
    }
    class Product
    {
        public string Name { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Price { get; set; }

        public string[] Sizes { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProxyType Type { get; set; }

        public Product() { }
    }
   

    [Serializable]
    class MyObject
    {
        public int n1 = 0;
        private int n2 = 0;
        string str = "";

        internal MyObject(int xx)
        {
            n2 = xx;
        }

        public MyObject() { }

        public MyObject(int xx,int yy)
        {
            n2 = xx;
        }

        public int N2 { get;
           // set;
        }
        
        public void setStr(string txt)
        {
            str = txt;
        }
       

        public override string ToString()
        {
            return $"Object {base.ToString()} n1={n1},n2={n2}, N2={N2} str={str}";
        }
    }

    class Program
    {
        private static int GetFreePort()
        {
            int defaultPort = 8123;
            try
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

                HashSet<int> used = new HashSet<int>();
                foreach(var ep in tcpEndPoints)
                {
                    used.Add(ep.Port);
                }
                foreach(var i in used)
                {
                    Console.WriteLine(i);
                }

                for(int port = defaultPort; port <= 65535; ++port)
                {
                    if (!used.Contains(port)) return port;
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            throw new Exception("No free port found.");
        }



        static void Main(string[] args)
        {
            WinINet.SetIEProxy(WinINet.IEProxyOption.Proxy_PAC, "127.0.0.1:9090", "http://127.0.0.1:1081/pac?t=20161108152556298");

            Console.WriteLine("Here");
            Console.ReadLine();
        }
    }
}
