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

            logger.Info("Client({0} connection accepted.", s.RemoteEndPoint);

            //从池中申请一个EventArgs用于receive, 并填充上下文属性
            SocketAsyncEventArgs recvArg;
            if (!TryGetEventArg(out recvArg))
            {
                logger.Info("Pool is empty. Stop accepting new connection.");
                CloseSocket(s);
                return;
            }
            ((AsyncUserToken)recvArg.UserToken).WorkSocket = s;

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
                default:
                    throw new SystemException("Never reach here. The last operation completed on the socket was not a receive or send: "+
                        e.LastOperation.ToString());
            }
        }



        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            logger.Info("ProcessReceive()");


            AsyncUserToken token = e.UserToken as AsyncUserToken;
            //处理接受到的数据前，先清除receivebuf，释放BufferManager
            token.ClearReceiveBuf();
            if (e.SocketError == SocketError.Success)
            {
                logger.Info("received {0} bytes from client {1}", e.BytesTransferred, token.WorkSocket.RemoteEndPoint);

                if (e.BytesTransferred == 0)
                {
                    logger.Info("connection has been disconnceted from client " + token.WorkSocket.RemoteEndPoint);
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

        private void SetBuffer(SocketAsyncEventArgs e)
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
            Socket socket = token.WorkSocket;
            Debug.Assert(socket.ProtocolType == ProtocolType.Tcp);

            token.ReceiveCompleted = e.BytesTransferred < e.Count;
            if (!token.ReceiveCompleted)
            {
                token.AddReceiveBuffer(e);

                SetBuffer(e);
                bool willRaiseEvent = socket.ReceiveAsync(e); //重用e,用于下一个receive
                if (!willRaiseEvent) ProcessReceive(e);

                return true;
            }
            else //token.ReceiveCompleted
            {
                token.AddReceiveBuffer(e);
                Core.Buffer receiveBuf = token.GetReceiveBuf();
                return HandleRequest(receiveBuf.Data, receiveBuf.Offset, receiveBuf.Count, e);
            }

        }

        private bool HandleRequest(byte[] data, int offset, int count, SocketAsyncEventArgs e)
        {
            try
            {
 
                AsyncUserToken token = e.UserToken as AsyncUserToken;
                Socket socket = token.WorkSocket;
                
                //
                logger.Info("Handle request: {0} bytes {1} buffer ", count, token.BufferCount);

                using (StreamReader sr = new StreamReader(new MemoryStream(data, offset, count)))
                {
                    string line = sr.ReadLine();
                    string[] firstLines = line.Split(' ');
                    string method = firstLines[0];
                    string path = firstLines[1];
                    string host = "";
                    while((line=sr.ReadLine())!= null)
                    {
                        int index = line.IndexOf(':');
                        if (index != -1)
                        {
                            if (line.Substring(0, index).Equals("host", StringComparison.OrdinalIgnoreCase))
                            {
                                host = line.Substring(index + 1);
                                break;
                            }
                        }
                    }
                    logger.Info(method + " " + host + path);
                }

                //转发数据
                e.SetBuffer(data, offset, count);
                bool willRaiseEvent = token.WorkSocket.SendAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }

                return true;
             
            }catch(Exception ex)
            {
                logger.Error("handle exception: " + ex.Message);
                throw;
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;
            Socket s = token.WorkSocket;
            if(e.SocketError == SocketError.Success)
            {
                logger.Info("transferred {0} bytes to client {1}", e.BytesTransferred, s.RemoteEndPoint);

                //重用e,用于下一个接受              
                SetBuffer(e);
                bool willRaiseEvent = token.WorkSocket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }
            }
            else
            {
                logger.Error("Send failed.");
                CloseAndRelease(e);
            }
        }


        private void CloseSocket(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Receive);
                Interlocked.Decrement(ref _connections);
                logger.Info("Client({0}) has been disconnected from the server.", socket.RemoteEndPoint);

                socket.Close();
                _sema.Release();
            }catch(SocketException ex)
            {
                logger.Error("Cannot close the socket." + ex.Message);
                throw;
            }
        }

        private void CloseAndRelease(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken  as  AsyncUserToken;
            CloseSocket(token.WorkSocket);
            _pool.Push(e);
        }

    }
}
