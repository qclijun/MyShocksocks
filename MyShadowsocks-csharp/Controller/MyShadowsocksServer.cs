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
using MyShadowsocks.Controller.Core;
using NLog;

namespace MyShadowsocks.Controller
{
    static class SocketAsyncEventArgsExt
    {
        public static void SetBuffer(this SocketAsyncEventArgs arg, Core.Buffer buf)
        {
            arg.SetBuffer(buf.Data, buf.Offset, buf.Count);
        }
    }

    public class MyShadowsocksServer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private ConcurrentStack<SocketAsyncEventArgs> _pool;

        private const int OptsToPreAlloc = 2;
        private readonly int _recvBufSize;
        private BufferManager _recvBufManager;

        private readonly int _maxConnections;
        private int _connections = 0;
        private Semaphore _sema;

        private Socket _listenSocket;


        public MyShadowsocksServer(int recvBufSize,int maxConnections)
        {
            this._recvBufSize = recvBufSize;
            this._maxConnections = maxConnections;
            _pool = new ConcurrentStack<SocketAsyncEventArgs>();
            _recvBufManager = new BufferManager(recvBufSize, OptsToPreAlloc * maxConnections * recvBufSize);
            _sema = new Semaphore(maxConnections, maxConnections);

            InitArgs();
            
        }

        private void InitArgs()
        {
            for(int i = 0; i < _maxConnections; ++i)
            {
                SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                arg.Completed += IO_Completed;
                AsyncUserToken token = new AsyncUserToken(_recvBufManager);
                arg.UserToken = token;
                arg.SetBuffer(_recvBufManager.GetBuffer());
                _pool.Push(arg);
            }
        }

        public void Start(IPEndPoint localEP)
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(localEP);
            _listenSocket.Listen(_maxConnections / 10);
            Console.WriteLine("listening "+localEP.ToString());
            logger.Info("listening " + localEP.ToString());
            StartAccept(null);
        }

        private void StartAccept(SocketAsyncEventArgs acceptArg)
        {
            if (acceptArg == null)
            {
                acceptArg = new SocketAsyncEventArgs();
                acceptArg.Completed += Accept_Completed;

            }
            else
            {
                acceptArg.AcceptSocket = null;
            }
            _sema.WaitOne();
            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptArg);
            }

        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }


        private void ProcessAccept(SocketAsyncEventArgs acceptArg)
        {
            Interlocked.Increment(ref _connections);
            Socket s = acceptArg.AcceptSocket;

            logger.Info("Client({0}) connection accepted.", s.RemoteEndPoint);

            //从池中申请一个EventArgs用于receive, 并填充上下文属性
            SocketAsyncEventArgs recvArg;
            if (!TryGetEventArg(out recvArg))
            {
                logger.Info("Pool is empty. Stop accepting new connection.");
                CloseSocket(s);
                return;
            }
            AsyncUserToken token = recvArg.UserToken as AsyncUserToken;
            token.WorkSocket = s;
            token.IsFromRemote = false;
            bool willRaiseEvent = s.ReceiveAsync(recvArg);
            if (!willRaiseEvent)
            {
                ProcessReceive(recvArg);
            }

            //accept another
            StartAccept(acceptArg);

        }

        private bool TryGetEventArg(out SocketAsyncEventArgs e)
        {
            if(!_pool.TryPop(out e))
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                //try again
                if(!_pool.TryPop(out e))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    if(!_pool.TryPop(out e))
                    {
                        return false;
                    }
                }
            }
            return true;
        }



        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                default:
                    throw new SystemException("Never reach here. The last operation completed on the socket was not a receive , send or connect: "+
                        e.LastOperation.ToString());
            }
        }



        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            logger.Info("ProcessReceive()");
            Debug.Assert(e.Buffer == _recvBufManager.Data,$"{e.Buffer==null}"); //EventArgs 的Buffer必定是在recvBufManager中分配的


            AsyncUserToken token = e.UserToken as AsyncUserToken;
            bool fromRemote = token.IsFromRemote;
            var ep = fromRemote ? token.RemoteSocket.RemoteEndPoint : token.WorkSocket.RemoteEndPoint;
            logger.Info("received {0} bytes from  {1}", e.BytesTransferred, ep);
            if (e.SocketError == SocketError.Success)
            {

                ////0字节是关闭socket的正常途径 
                if (e.BytesTransferred == 0)
                {
                    CloseAndRelease(e);
                    return;
                }

                if (!HandlePacket(e)) //处理数据失败
                {
                    logger.Error("handle failed.");
                    CloseAndRelease(e);
                }
                else
                {

                }
            }
            else
            {
                logger.Error("receive failed " + e.SocketError.ToString());
                CloseAndRelease(e);
            }
        }

        private void AllocBuffer(SocketAsyncEventArgs e)
        {
            var newBuf = _recvBufManager.GetBuffer();
            if (!newBuf.IsNull())
            {
                e.SetBuffer(newBuf);
            }
            else
            {
                logger.Error("No more buffer in BufferManager.");
                // e 不能重用了
                e.Dispose();
            }
        }

        bool HandlePacket(SocketAsyncEventArgs e)
        {
            
            AsyncUserToken token = e.UserToken as AsyncUserToken;
            //Socket socket = token.WorkSocket;
            //Debug.Assert(socket.ProtocolType == ProtocolType.Tcp);

            if (token.IsFromRemote) //response  from server, 直接转发给client
            {
                Debug.Assert(token.IsConnected);
                e.SetBuffer(e.Offset, e.BytesTransferred);
                token.IsSendRemote = false;
                if (!token.WorkSocket.SendAsync(e))
                {
                    ProcessSend(e);
                }
                return true;
            }
            if (token.IsConnected) 
            {
                // request from client && has connected to server,直接转发给server
                e.SetBuffer(e.Offset, e.BytesTransferred);
                token.IsSendRemote = true;
                if (!token.RemoteSocket.SendAsync(e))
                {
                    ProcessSend(e);
                }
                return true;
            }



            token.ReceiveCompleted = e.BytesTransferred < e.Count;
            if (!token.ReceiveCompleted)
            {
                token.AddReceiveBuffer(e);

                AllocBuffer(e);
                token.IsFromRemote = false;
                bool willRaiseEvent = token.WorkSocket.ReceiveAsync(e); //重用e,用于下一个receive
                if (!willRaiseEvent) ProcessReceive(e);

                return true;
            }
            else 
            {
                //if (token.BytesReceived == 0)
                //{
                //    CloseAndRelease(e);
                //    return true;
                //}
                token.AddReceiveBuffer(e);
                Core.Buffer receiveBuf = token.GetReceiveBuf();
                token.ClearReceiveBuf();
                return HandleRequest(receiveBuf.Data, receiveBuf.Offset, receiveBuf.Count, e);
            }

        }



        private bool HandleRequest(byte[] data, int offset, int count, SocketAsyncEventArgs e)
        {
            //try
            //{
 
                AsyncUserToken token = e.UserToken as AsyncUserToken;
                //              
                //从浏览器收到的数据

                Debug.Assert(token.IsConnected == false);

                HttpRequestParser request = new HttpRequestParser(data,offset,count);
                request.Parse();
                logger.Info("Handle request: " + request.ToString());
                IPEndPoint remoteEP = request.GetEndPoint();



                //连接server
                logger.Info("Connect to server " + remoteEP + " .......");

            switch (request.Method)
            {
                case HttpMethod.Connect:

                    break;
                case HttpMethod.Get:
                case HttpMethod.Post:

                    break;
                default:
                    throw new NotImplementedException(request.Method.ToString());
            }



            token.ConnectBytes = new Core.Buffer(data, offset, count);
                e.RemoteEndPoint = remoteEP;
                e.SetBuffer(null, 0, 0);
                if (!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, e))
                {
                    ProcessConnect(e);
                }





            return true;
             
            //}catch(Exception ex)
            //{
            //    logger.Error("handle exception: " + ex.Message);
            //    throw;
            //}
        }

        private void StartConnect(IPEndPoint remoteEP,SocketAsyncEventArgs e)
        {
            e.RemoteEndPoint = remoteEP;
            e.SetBuffer(null, 0, 0);
            if (!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, e))
            {
                ProcessConnect(e);
            }
        }


        private void ConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnect(e);
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;
            if (e.SocketError != SocketError.Success)
            {
                var ex = e.ConnectByNameError ?? new SocketException((int)e.SocketError);
                logger.Error("connect to {0} faile: {1} ",e.RemoteEndPoint, ex.Message);
                //throw ex;
                CloseAndRelease(e);
            }
            else
            {
                logger.Info("connect to {0} sucess.", e.RemoteEndPoint);
                token.RemoteSocket = e.ConnectSocket;
                token.IsConnected = true;
                Debug.Assert(token.RemoteSocket.Connected);

                //连接成功后，重用e,
                e.SetBuffer(token.ConnectBytes);
                token.IsSendRemote = true;
                bool willRaiseEvent = token.RemoteSocket.SendAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }




                //连接成功后，新建eventArgs, 准备接受client数据
                //SocketAsyncEventArgs recvArg;
                //if (!TryGetEventArg(out recvArg))
                //{
                //    logger.Error("Pool is empty. Cannot receive from server.");
                //    CloseSocket(token.RemoteSocket);
                //    return;
                //}
                //AllocBuffer(recvArg);
                //((AsyncUserToken)recvArg.UserToken).IsFromRemote = true;
                //bool willRaiseEvent2 = token.RemoteSocket.ReceiveAsync(recvArg);
                //if (!willRaiseEvent2)
                //{
                //    ProcessReceive(recvArg);
                //}
            }

        }

        private bool InBufferManager(SocketAsyncEventArgs e)
        {
            return e.Buffer == _recvBufManager.Data;
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {



            AsyncUserToken token = e.UserToken as AsyncUserToken;
            logger.Info("Send: {0} bytes to {1} ", e.BytesTransferred,
                token.IsSendRemote ? token.RemoteSocket.RemoteEndPoint : token.WorkSocket.RemoteEndPoint);
            if(e.SocketError == SocketError.Success)
            {
                

                if (!InBufferManager(e))
                {
                    //不在bufferManager中，需要重新在bufferManager中申请
                    AllocBuffer(e);
                }
                else
                {
                    e.SetBuffer(e.Offset, e.Count);
                }
                
               
                if (token.IsSendRemote)
                {
                    token.IsFromRemote = true;
                    if (!token.RemoteSocket.ReceiveAsync(e))
                    {
                        ProcessReceive(e);
                    }
                }
                else
                {
                    token.IsFromRemote = false;
                    if (!token.WorkSocket.ReceiveAsync(e))
                    {
                        ProcessReceive(e);
                    }
                }

              


            }
            else
            {
                logger.Error("Send failed. ");
                CloseAndRelease(e);
            }
        }


        private void CloseSocket(Socket socket)
        {
            if (socket == null) return;
            try
            {
                socket.Shutdown(SocketShutdown.Receive);
                
                logger.Info("Close socket  "+socket.RemoteEndPoint);

                socket.Close();
                
            }catch(SocketException ex)
            {
                logger.Error("Cannot close the socket." + ex.Message);
                throw;
            }
        }

        private void CloseAndRelease(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken  as  AsyncUserToken;
            if (token.IsFromRemote)
            {
                logger.Debug("Connection closed from the  server " + token.RemoteSocket.RemoteEndPoint);
                CloseSocket(token.RemoteSocket);
                Interlocked.Decrement(ref _connections);
                _sema.Release();
                CloseSocket(token.WorkSocket);

            }
            else
            {
                logger.Debug("Connection closed from the client " + token.WorkSocket.RemoteEndPoint);
                Interlocked.Decrement(ref _connections);
                _sema.Release();
                CloseSocket(token.WorkSocket);
                if (token.IsConnected) CloseSocket(token.RemoteSocket);               
            }

            token.Clear();
            _pool.Push(e);
        }

    }
}
