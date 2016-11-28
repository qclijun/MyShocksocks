using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Jun.Net;

namespace MyShadowsocks.Controller {
    sealed class HttpProxyConnection : ProxyConnection {
        

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        private HttpRequestParser _parser;

        public HttpProxyConnection(Socket clientSocket)
            : base(clientSocket) { }



        public override async Task StartHandshake() {
            try {
                byte[] requestBytes = await ReadFirstRequest();
                await ProcessRequest(requestBytes, 0, requestBytes.Length);
                StartPipe();
            } catch {
                OnException();
            }
        }




        public override async Task ProcessRequest(byte[] buffer, int offset, int count) {
            try {
                string requestText = Encoding.ASCII.GetString(buffer, offset, count);
                _parser = new HttpRequestParser(requestText);

                try {
                    _parser.Parse();
                } catch {
                    throw new FormatException("Http Request Format Error.");
                }


                IPAddress[] addrList = Dns.GetHostAddresses(_parser.Host);
                ServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                string proxyConnValue;
                if(_parser.Headers.TryGetValue("Proxy-Connection", out proxyConnValue)) {
                    if(proxyConnValue.Equals("keep-alive", StringComparison.OrdinalIgnoreCase))
                        ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                }

                logger.Info("Connect: " + _parser.ToString());

                await ServerSocket.ConnectTaskAsync(addrList, _parser.Port);

                logger.Debug("Connect successed. " + this.ToString());
                string rq;
                if(_parser.Method == "CONNECT") {
                    rq = _parser.HttpVersion + " 200 Connection stablished\r\nProxy-Agent: Mentalis Proxy Server\r\n\r\n";
                    await ClientSocket.SendTaskAsync(Encoding.ASCII.GetBytes(rq), 0, rq.Length, SocketFlags.None);

                } else {
                    //rq = _parser.RequestText;
                    await ServerSocket.SendTaskAsync(buffer, offset, count, SocketFlags.None);

                }

            } catch {

                SendBadRequest();
                OnException();
            }
        }






        private const string brs = "HTTP/1.1 400 Bad Request\r\n"
            + "Connection: close\r\n"
            + "Content-Type: text/html\r\n"
            + "<html><head><title>400 Bad Request</title></head><body>"
            + "<div align=\"center\"><table border=\"0\" cellspacing=\"3\" cellpadding=\"3\" bgcolor=\"#C0C0C0\"><tr>"
            + "<td><table border=\"0\" width=\"500\" cellspacing=\"3\" cellpadding=\"3\"><tr>"
            + "<td bgcolor=\"#B2B2B2\"><p align=\"center\"><strong><font size=\"2\" face=\"Verdana\">400 Bad Request</font></strong></p>"
            + "</td></tr><tr><td bgcolor=\"#D1D1D1\"><font size=\"2\" face=\"Verdana\"> JOY The proxy server could not understand the HTTP request!"
            + "<br><br> Please contact your network administrator about this problem.</font></td></tr></table></center></td></tr></table></div></body></html>";

        private async void SendBadRequest() {
            try {
                byte[] bytes = Encoding.ASCII.GetBytes(brs);
                await ClientSocket.SendTaskAsync(bytes, 0, bytes.Length, SocketFlags.None);
            } catch {
                OnException();
            }
        }


        public override string ToString() {
            try {
                if(ConnectedToServer) {
                    return ClientSocket.RemoteEndPoint + " <----> "
                        + ServerSocket.RemoteEndPoint;
                } else {
                    return "Http connection " + ClientSocket.RemoteEndPoint;
                }

            } catch {
                return "Http Connection";
            }
        }

    }
}
