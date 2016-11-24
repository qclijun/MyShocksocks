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

using System.Security.Cryptography;
using System.Runtime.InteropServices;

using MyShadowsocks.Encryption;
using System.Diagnostics;
using MyShadowsocks.Controller;
using System.Net.Sockets;
using System.Threading;
using Junlee.Util.Sockets;

namespace Sample {
    enum ProxyType {
        Socks5 = 0,
        Http = 1,
    }
    class Product {
        public string Name { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Price { get; set; }

        public string[] Sizes { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProxyType Type { get; set; }

        public Product() { }
    }


    [Serializable]
    class MyObject {
        public int n1 = 0;
        private int n2 = 0;
        string str = "";

        internal MyObject(int xx) {
            n2 = xx;
        }

        public MyObject() { }

        public MyObject(int xx, int yy) {
            n2 = xx;
        }

        public int N2 {
            get;
            // set;
        }

        public void setStr(string txt) {
            str = txt;
        }


        public override string ToString() {
            return $"Object {base.ToString()} n1={n1},n2={n2}, N2={N2} str={str}";
        }
    }

    class Program {
        private static int GetFreePort() {
            int defaultPort = 8123;
            try {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

                HashSet<int> used = new HashSet<int>();
                foreach(var ep in tcpEndPoints) {
                    used.Add(ep.Port);
                }
                foreach(var i in used) {
                    Console.WriteLine(i);
                }

                for(int port = defaultPort;port <= 65535;++port) {
                    if(!used.Contains(port)) return port;
                }
            } catch(Exception ex) {
                Console.WriteLine(ex);
            }
            throw new Exception("No free port found.");
        }


        static async Task TaskA() {
            Console.WriteLine("TaskA start");
            await Task.Delay(4000);
            Console.WriteLine("TaskA completed.");
            
        }

        static async Task TaskB() {
            Console.WriteLine("TaskB start");
            await Task.Delay(2000);
            Console.WriteLine("TaskB completed");
        }

       static async Task TaskC() {
            Console.WriteLine("TaskC start");
            await Task.Delay(1000);
            Console.WriteLine("TaskC completed");
        }


        static void Main(string[] args) {

            //WinINet.SetSystemProxy(WinINet.SystemProxyOption.Proxy_PAC, "127.0.0.1:9090", "http://127.0.0.1:1081/pac?t=20161108152556298");


            MainAsync().Wait();



            string host = "A.ISS.TF";
            int port = 1024;
            string method = "aes-256-cfb";
            string password = "31554049";


            
           


            //Start(host, port, encryptor);

            Console.WriteLine("Here");
            Console.ReadKey();
        }

        static async Task MainAsync() {

            //'hot' task begin execute at creation
            Task a = TaskA();
            Task b = TaskB();
            Task c = TaskC();
            try {
                await a;
                await b;
                await c;
            } catch {

            }
        }




        static void DisplayByteArray(byte[] data, int offset, int count) {
            for(int i = 0;i < count;++i) {
                Console.Write("{0,3:X2}", data[i]);
            }
            Console.WriteLine();
        }

        static void DisplayByteArray(byte[] data) {
            DisplayByteArray(data, 0, data.Length);

        }


        static string requestText = @"GET http://t66y.com/ HTTP/1.1
Host: t66y.com
Proxy-Connection: keep-alive
Cache-Control: max-age=0
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Accept-Encoding: gzip, deflate, sdch
Accept-Language: zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4

";

        static async void Start(string host, int port, MbedTLSEncrytor encryptor) {
            Socket remoteSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            IPAddress[] addrList = Dns.GetHostAddresses(host);
            foreach(var addr in addrList) {
                Console.WriteLine(addr.AddressFamily + " " + addr);
            }
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try {
                await remoteSocket.ConnectTaskAsync(addrList, port, cts.Token);

                Console.WriteLine("connect successed " + remoteSocket.Connected);

                string dest = "t66y.com";
                //ushort destPort = 80;

                byte[] input = new byte[] { 3 };

                MemoryStream ms = new MemoryStream();
                ms.Write(input, 0, input.Length);
                ms.WriteByte((byte)dest.Length);
                ms.Write(Encoding.ASCII.GetBytes(dest), 0, dest.Length);
                ms.WriteByte(0);
                ms.WriteByte(80);

                input = ms.ToArray();

                DisplayByteArray(input);

                byte[] buffer = new byte[1024];
                int outLen;
                encryptor.EncryptFirstPackage(input, input.Length, buffer, out outLen);



                int bytes = await remoteSocket.SendTaskAsync(buffer, 0, outLen, SocketFlags.None);

                Console.WriteLine("send {0} bytes", bytes);

                input = Encoding.ASCII.GetBytes(requestText);
                encryptor.EncryptFirstPackage(input, input.Length, buffer, out outLen);


                bytes = await remoteSocket.SendTaskAsync(buffer, 0, outLen, SocketFlags.None);

                Console.WriteLine("send {0} bytes", bytes);


                bytes = await remoteSocket.ReceiveTaskAsync(buffer, 0, buffer.Length, SocketFlags.None);

                Console.WriteLine("receive {0} bytes", bytes);

                //解密
                Console.WriteLine("Decrypt  IV:");
                DisplayByteArray(buffer, 0, encryptor.IVSize);

                byte[] tempBuf = new byte[4096];
                encryptor.DecryptFirstPackage(buffer, bytes, tempBuf, out outLen);


                Console.WriteLine("Response:");
                Console.WriteLine(Encoding.ASCII.GetString(tempBuf, 0, outLen));


            } catch(OperationCanceledException ex) {
                Console.WriteLine(ex.Message + " " + ex);
            } catch(AggregateException ex) {
                Console.WriteLine(ex + " InnerEx: " + ex.InnerException);
            } catch(Exception ex) {
                Console.WriteLine(ex);
                throw;
            }
        }

    }
}
