using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Junlee.Util.Sockets;
using MyShadowsocks.Controller.Core;
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

        public ProxyListener() { }

        public async Task StartListen(int port) {
            Stop();
            ListenEp = new IPEndPoint(IPAddress.Any, port);

            try {
                _listenSocket = new Socket(ListenEp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _listenSocket.Bind(ListenEp);
                _listenSocket.Listen(100);
                logger.Info("Start Proxy On  " + ListenEp);
            }catch(SocketException ex) {
                logger.Error("Listen failed. " + ex.Message);
            }

            try {
                while(true) {
                    Socket requestSocket =
                        await _listenSocket.AcceptTaskAsync().ConfigureAwait(false);
                    ProxyConnection newConn = new SocksProxyConnection(requestSocket);

                    await newConn.StartHandshake();
                }
            }catch(ObjectDisposedException) {
                //// Stop() cause this exception, and this task finished.
            }catch(Exception ex) {
                logger.Error("Accept failed." + ex.Message);
            }

        }

       

        public void Stop() {
            if(_listenSocket != null) {
                logger.Info("Stop listening on " + ListenEp);
                _listenSocket.Close();               
            }           
        }

        

        //private void RemoveConnection(ProxyConnection conn) {
        //    if(!conn.ConnectedToServer) return;
        //    lock (_connections) {
        //        _connections.Remove(conn);
        //    }
        //}

        //private void AddConnection(ProxyConnection conn) {
        //    lock (_connections) {
        //        _connections.Add(conn);
        //    }
        //}

        private bool _disposed = false;
        public void Dispose() {
            if(_disposed) return;
            _disposed = true;

            _listenSocket?.Close();
            _listenSocket = null;            
            
        }

        public void Close() {
            Dispose();
        }

     

    }
}


