using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MyShadowsocks.Util.Sockets;
using NLog;

namespace MyShadowsocks.Controller
{
    public class AsyncClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Socket connectSocket;
        private MemoryStream w;
        public AsyncClient()
        {
            w = new MemoryStream();
        }

        public void Connect()
        {
            string host = "www.baidu.com";
            var remoteEP = SocketUtils.GetEndPoint(host, 80);
            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
            arg.Completed += IO_Completed;
            arg.RemoteEndPoint = remoteEP;
            byte[] data = Encoding.ASCII.GetBytes(request);
            arg.SetBuffer(data, 0, data.Length);
            if(!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, arg))
            {
                ProcessConnect(arg);
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("never reach here.");

            }
        }

        string request = @"GET / HTTP/1.1
Host: www.baidu.com
Connection: keep-alive
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Accept-Encoding: gzip, deflate, sdch
Accept-Language: zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4

";

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket remote = e.ConnectSocket;
                // byte[] data = Encoding.UTF8.GetBytes(request);
                //e.SetBuffer(data, 0, data.Length);
                byte[] buffer = new byte[1024];
                e.SetBuffer(buffer, 0, buffer.Length);
                if (!remote.ReceiveAsync(e))
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                logger.Error("ProcessConnect() failed. " + e.ConnectByNameError);
                Close(e);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                byte[] buffer = new byte[1024];
                e.SetBuffer(buffer, 0, buffer.Length);
                if (!e.ConnectSocket.ReceiveAsync(e))
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                logger.Error("ProcessSend() failed.");
                Close(e);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            Console.WriteLine("receive {0} bytes from {1}",e.BytesTransferred,e.RemoteEndPoint);
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred < e.Count)
                {
                    w.Write(e.Buffer, e.Offset, e.Count);
                    Console.WriteLine(Encoding.ASCII.GetString(w.ToArray()));
                    Close(e);
                }
                else
                {
                    w.Write(e.Buffer, e.Offset, e.Count);
                    e.SetBuffer(e.Offset, e.Count);
                    if (!e.ConnectSocket.ReceiveAsync(e))
                    {
                        ProcessReceive(e);
                    }
                }
                
            }
            else
            {
                logger.Error("ProcessReceive() failed.");
                Close(e);
            }
            
            
        }

        private void Close(SocketAsyncEventArgs e)
        {
            if (e.ConnectSocket != null)
            {
                e.ConnectSocket.Close();
            }
            e.Dispose();
        }
    }
}
