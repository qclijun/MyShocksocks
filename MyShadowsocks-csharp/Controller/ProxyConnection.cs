using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Jun.Net;
using NLog;

namespace MyShadowsocks.Controller {

    /// <summary>
    /// ProxyConnection由两个Socket组成，发送请求的ClientSocket和响应请求的ServerSocket组成。
    /// 它负责管理这两个Socket的连接、关闭，接收和发送数据。
    /// ClientSocket的连接由ProxyListener建立，连接建立好之后传递给ProxyConnection的构造函数。
    /// </summary>
    public abstract class ProxyConnection : IDisposable {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static HashSet<ProxyConnection> _connections = new HashSet<ProxyConnection>();

        public static int ConnectionCount => _connections.Count;

        public EventHandler CloseHandler = (sender, e) => RemoveConnection((ProxyConnection)sender);


        public Socket ClientSocket { get; private set; }

        public Socket ServerSocket { get; protected set; }

        public bool ConnectedToServer => ServerSocket != null && ServerSocket.Connected;

        public const int RecvSize = 8192;

        public const int BufferSize = RecvSize + 32;



        public byte[] ClientBuffer { get; } = new byte[BufferSize];
        public byte[] ServerBuffer { get; } = new byte[BufferSize];



        private bool _clientShutdown = false;
        private bool _serverShutdown = false;


        //DateTime field
        private DateTime _lastActivity;
        private DateTime _startConnectTime;
        private DateTime _sstartReceiveTime;
        private DateTime _startSendingTime;

        private readonly TimeSpan _serverTimeout;


        


        protected ProxyConnection(Socket clientSocket) {
            ClientSocket = clientSocket;
            _lastActivity = DateTime.Now;
        }

        private ProxyConnection() : this(null) { }



        #region IDisposable

        ~ProxyConnection() {
            Dispose(false);
        }

        private int _disposed = 0;

        public virtual void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            int oldValue = Interlocked.Exchange(ref _disposed, 1);
            if(oldValue == 1) return;


            logger.Debug("Close Connection " + this);
            if(disposing) {
                //free managed resources
                
                try {
                    ClientSocket?.FullClose();
                    ServerSocket?.FullClose();

                    CloseHandler?.Invoke(this, null);
                } catch { }
            }

            // free unmanaged resources


        }



        public void Close() {
            Dispose();
        }

        public void CheckClose() {
            if(_clientShutdown && _serverShutdown)
                Close();
        }


        #endregion


        protected void OnException(Exception ex) {
            logger.Error(ex.Message);
            OnException();
        }

        protected void OnException(string msg) {
            logger.Error(msg);
            OnException();
        }

        protected void OnException() {
            OnClosed();
        }

        protected static void AddConnection(ProxyConnection conn) {
            if(conn == null) return;
            lock (_connections) {
                _connections.Add(conn);
            }
        }

        protected static void RemoveConnection(ProxyConnection conn) {
            lock (_connections) {
                _connections.Remove(conn);
            }
        }

        private void OnClosed() {
            CloseHandler(this, null);
            Close();           
        }


        public override string ToString() {
            try {
                if(ConnectedToServer) {
                    return ClientSocket.RemoteEndPoint + " <----> "
                        + ServerSocket.RemoteEndPoint;
                } else {
                    return "Connection from " + ClientSocket.RemoteEndPoint;
                }
            } catch {
                return "Connection";
            }
        }


        public abstract Task StartHandshake();

        public abstract Task ProcessRequest(byte[] buffer, int offset, int count);


        #region ReadFirstRequest


        protected async Task<byte[]> ReadFirstRequest() {
            MemoryStream ms = new MemoryStream(2048);
            bool completed = false;
            while(!completed) {
                var task = ClientSocket.ReceiveTaskAsync(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None);
                int bytesRead = await task.ConfigureAwait(false);
                if(bytesRead <= 0) {
                    throw new SocketException((int)SocketError.OperationAborted);
                }
                ms.Write(ClientBuffer, 0, bytesRead);
                completed = bytesRead < ClientBuffer.Length;
            }
            return ms.ToArray();
        }


        #endregion



        #region StartPipe        

        protected virtual async Task StartPipeS2C() {
            while(true) {
                Task<int> t1 = ServerSocket.ReceiveTaskAsync(ServerBuffer, 0, ServerBuffer.Length, SocketFlags.None);
                int bytes = await t1;
                if(bytes <= 0) {
                    throw new SocketException((int)SocketError.OperationAborted);
                }
                bytes = await ClientSocket.SendTaskAsync(ServerBuffer, 0, bytes, SocketFlags.None);
                if(bytes <= 0) {
                    throw new SocketException((int)SocketError.OperationAborted);
                }
            }
        }

        protected virtual async Task StartPipeC2S() {
            while(true) {
                Task<int> t1 = ClientSocket.ReceiveTaskAsync(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None);

                int bytes = await t1;
                if(bytes <= 0) {
                    throw new SocketException((int)SocketError.OperationAborted);
                }

                bytes = await ServerSocket.SendTaskAsync(ClientBuffer, 0, await t1, SocketFlags.None);
                if(bytes <= 0) {
                    throw new SocketException((int)SocketError.OperationAborted);
                }
            }
        }

        /// <summary>
        /// ClientSocket和ServerSocket都已经连接，开始传输数据
        /// 这个异步方法已经处理了全部的异常，不会有异常抛出
        /// </summary>
        /// <returns>void</returns>
        public async void StartPipe() {
            logger.Debug("StartPipe...");
            try {
                await await Task.WhenAny(StartPipeC2S(), StartPipeS2C());
            } catch(Exception ex) {
                OnException(ex);
            }
        }

        #endregion

    }






}
