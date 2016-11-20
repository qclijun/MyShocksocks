using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyShadowsocks.Util.Sockets;

namespace MyShadowsocks.Controller
{
    enum HttpMethod
    {
        Get = 0,
        Post = 1,
        Connect =  2,
        Put,
        Delete,
        Trace,
        Patch,
        Options,
        Head,

    }
    class HttpRequestParser
    {
        private ArraySegment<byte> _data;

        public HttpMethod Method { get; private set; }

        public string Version { get; private set; }

        public Uri FullPath { get; private set; }

        private Dictionary<string, string> _headers;
        public HttpRequestParser(ArraySegment<byte> data)
        {
            this._data = data;
            _headers = new Dictionary<string, string>();
        }

        public HttpRequestParser(byte[] data,int offset, int count)
        {
            _data = new ArraySegment<byte>(data,offset,count);
            _headers = new Dictionary<string, string>();
        }

        public HttpRequestParser(byte[] data) : this(data, 0, data.Length) { }


        private static Dictionary<string, HttpMethod> methodMap;

        static HttpRequestParser()
        {
            methodMap = new Dictionary<string, HttpMethod>();
            methodMap.Add("get", HttpMethod.Get);
            methodMap.Add("post", HttpMethod.Post);
            methodMap.Add("connect", HttpMethod.Connect);
            methodMap.Add("put", HttpMethod.Put);
            methodMap.Add("delete", HttpMethod.Delete);
            methodMap.Add("head", HttpMethod.Head);
            methodMap.Add("options", HttpMethod.Options);
            methodMap.Add("trace", HttpMethod.Trace);
            methodMap.Add("patch", HttpMethod.Patch);
        }


        private HttpMethod GetMethodFromString(string methodStr)
        {
            return methodMap[methodStr.ToLower()];
        }

        public void Parse()
        {
            
            using(StreamReader sr = new StreamReader(new MemoryStream(_data.Array, _data.Offset, _data.Count)))
            {
                string line = sr.ReadLine();
                string[] firstlines = line.Split(' ');

                Method = GetMethodFromString(firstlines[0].ToLower());
                string path = firstlines[1];
                Version = firstlines[2];

                while ((line = sr.ReadLine()) != null)
                {
                    int index = -1;
                    if ((index = line.IndexOf(':')) != -1)
                    {
                        _headers.Add(line.Substring(0, index).ToLower(), line.Substring(index + 1).TrimStart());
                    }

                }
                var host = _headers["host"];

                switch (Method)
                {
                    case HttpMethod.Connect:
                        FullPath = new Uri("http://" + path);
                        break;
                    default:
                        FullPath = new Uri(path);
                        break;
                }
                

            }
        }

        public string GetHeader(string headerName)
        {
            string value = "";
            if(!_headers.TryGetValue(headerName.ToLower(),out value))
            {
                value = "";
            }
            return value;
        }



        public string Host
        {
            get
            {
                return FullPath.Host;
            }
        }

        public int Port
        {
            get
            {
                if (FullPath.IsDefaultPort) return 80;
                return FullPath.Port;
            }
        }



        public IPEndPoint GetEndPoint()
        {
            
            return SocketUtils.GetEndPoint(Host, Port);
        }

        public override string ToString()
        {
            return Method.ToString() + " " + FullPath.ToString();
        }

    }
}
