using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Controller {
    class HttpRequestParser {

        public string RequestText {
            get; private set;
        }

        public string Method { get; private set; }

        public string HttpVersion { get; private set; }


        public string UrlPath { get; private set; }

        public int Port { get; private set; }

        public string Host {
            get; private set;
        }
        public string FirstLine { get; private set; }

        private string _postString = "";

        public string PostString {
            get {
                if(_postString == "") {
                    int index = RequestText.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                    _postString = RequestText.Substring(index + 4);
                }
                return _postString;
            }
        }

        private Dictionary<string, string> _headers
            = new Dictionary<string, string>();

        public IDictionary<string, string> Headers => _headers;

        public HttpRequestParser(string request) {
            RequestText = request;

        }





        public void Parse() {
            StringReader _source = new StringReader(RequestText);


            string line = _source.ReadLine();
            FirstLine = line;
            string[] firstLine = line.Split(' ');
            Method = firstLine[0].ToUpper();
            UrlPath = firstLine[1];
            HttpVersion = firstLine[2];
            while((line = _source.ReadLine()) != null) {
                int index = line.IndexOf(':');
                if(index != -1) {
                    _headers.Add(line.Substring(0, index), line.Substring(index + 1).Trim());
                }
            }

            GetHostAndPort();

        }

        private void GetHostAndPort() {
            Host = _headers["Host"];
            int defaultPort = Method == "CONNECT" ? 443 : 80;

            int index = Host.IndexOf(':');
            if(index != -1) {
                Port = int.Parse(Host.Substring(index + 1));
                Host = Host.Substring(0, index);
            } else {
                Port = defaultPort;
            }
        }



        public override string ToString() {
            return FirstLine;
        }
    }
}
