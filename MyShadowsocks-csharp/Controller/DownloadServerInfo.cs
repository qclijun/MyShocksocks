using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using MyShadowsocks.Model;

namespace MyShadowsocks.Controller {
    /// <summary>
    /// 从url中下载ISS server信息，构建Server对象
    /// </summary>
    class DownloadServerInfo {
        private const string url = "http://www.ishadowsocks.net";


        public static async Task<List<Server>> GetServerInfo() {
            const int timeout = 5;
            List<Server> serverList = new List<Server>();
            Task<byte[]> download = new WebClient().DownloadDataTaskAsync(url);
            
            Task t = Task.Delay(TimeSpan.FromSeconds(timeout));

            
            if(t == await Task.WhenAny(t, download)) {
                throw new TimeoutException("download from " + url + " time out.");
            }

            byte[] data = await download; //may throw exception


            HtmlDocument doc = new HtmlDocument();
            doc.Load(new MemoryStream(data), Encoding.UTF8);



            #region try parse
            try {
                foreach(HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class='col-sm-4 text-center']")) {
                    Server s = new Server();
                    foreach(var child in node.ChildNodes) {
                        if(child.NodeType != HtmlNodeType.Element) continue;
                        string text = child.InnerText;
                        if(text.IndexOf("端口") != -1) {
                            int index = text.IndexOf(':');
                            s.ServerPort = int.Parse(text.Substring(index + 1));

                        } else if(text.IndexOf("密码:") != -1) {
                            int index = text.IndexOf(':');
                            s.Password = text.Substring(index + 1);

                        } else if(text.IndexOf("加密方式") != -1) {
                            int index = text.IndexOf(':');
                            s.Method = text.Substring(index + 1);
                        } else if(text.IndexOf("服务器地址") != -1) {
                            int index = text.IndexOf(':');
                            s.HostName = text.Substring(index + 1);
                        }
                    }

                    if(s.HostName != "" && s.Password != "")
                        serverList.Add(s);


                }
            } catch(Exception) {
                //catch parse exception
                return serverList;
            }
            #endregion



            return serverList;
        }



        public static async Task<bool> UpdateServerList(List<Server> serverList2) {
            List<Server> newList = await GetServerInfo();


            if(newList.Count == 0) return false; //false 不需要更改ServerList

            //List<Server> serverList = Server.CloneList(serverList2);
            List<Server> serverList = serverList2;//直接修改 不克隆


            BitArray flags = new BitArray(newList.Count);

            bool modified = false;

            foreach(var s in serverList) {
                for(int i = 0;i < newList.Count;++i) {
                    if(flags[i]) continue;
                    if(string.Equals(s.HostName, newList[i].HostName, StringComparison.OrdinalIgnoreCase)) {
                        flags[i] = true;
                        if(s.Password != newList[i].Password) {
                            s.Password = newList[i].Password;
                            modified = true;
                        }
                        if(s.ServerPort != newList[i].ServerPort) {
                            s.ServerPort = newList[i].ServerPort;
                            modified = true;
                        }
                        if(!s.Method.Equals(newList[i].Method, StringComparison.OrdinalIgnoreCase)) {
                            s.Method = newList[i].Method;
                            modified = true;
                        }
                    }
                }
            }

            //newList中还有新的服务器
            for(int i = 0;i < newList.Count;++i) {
                if(flags[i] == false) {
                    modified = true;
                    serverList.Add(newList[i]);
                }
            }
            return modified;




        }
    }
}
