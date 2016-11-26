using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jun;
using Jun.Net;
using NLog;

namespace MyShadowsocks.Controller {


    public class ProxyListener {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        public IPEndPoint ListenEp { get; private set; }

        public int ListenPort => ListenEp.Port;
        public IPAddress ListenAddress => ListenEp.Address;

        private Socket _listenSocket;

        //private HashSet<ProxyConnection> _connections = new HashSet<ProxyConnection>();

        //public int ConnectionCount => _connections.Count();

        public ProxyListener(IPEndPoint listenEp) {
            ListenEp = listenEp;
        }

        public ProxyListener(IPAddress address, int port)
            : this(new IPEndPoint(address, port)) { }

        public ProxyListener(int port) : this(IPAddress.Any, port) { }

        public ProxyListener():this(0) {
            
        }

        public async Task StartListen(int port) {
            if(_listenSocket != null)
                throw new Exception("Already started. Please stop it first.");

            if(NetUtils.IsPortInUse(port)) throw new SocketException((int)SocketError.AddressAlreadyInUse);

            ListenEp.Port = port;

            try {
                _listenSocket = new Socket(ListenEp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _listenSocket.Bind(ListenEp);
                _listenSocket.Listen(100);
                logger.Info("Start Proxy On  " + ListenEp);
            } catch(SocketException ex) {
                logger.Error("Listen failed. " + ex.SocketErrorCode);
                Stop();
                throw;
            }

            try {
                while(true) {
                    Socket requestSocket =
                        await _listenSocket.AcceptTaskAsync().ConfigureAwait(false);
                    ProxyConnection newConn = new SocksProxyConnection(requestSocket);

                    //StartHandshake处理了所有的异常，不会有异常抛出。
                    //所以无限循环只能在AcceptTaskAsync失败（如Stop())时结束
                    await newConn.StartHandshake();
                }
            } catch(ObjectDisposedException) {
                //// Stop() cause this exception, and this task finished.
                //ignore 
            } catch(SocketException ex) {

                logger.Error("Accept failed. " + ex.SocketErrorCode);
                Stop();
                throw;
            } catch(Exception ex) {
                logger.Error("Accept failed." + ex.TypeAndMessage());
                Stop();
                throw;
            }

        }

        public void Stop() {
            if(_listenSocket != null) {
                logger.Info("Stop listening on " + ListenEp);
                _listenSocket.Close();
                _listenSocket = null;
            }
        }


        //private bool _disposed = false;
        //public void Dispose() {
        //    if(_disposed) return;
        //    _disposed = true;

        //    _listenSocket?.Close();
        //    _listenSocket = null;            

        //}

        public void Close() {
            Stop();

        }



    }
}


