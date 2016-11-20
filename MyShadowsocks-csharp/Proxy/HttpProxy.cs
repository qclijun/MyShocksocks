using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyShadowsocks.Util.Sockets;

namespace MyShadowsocks.Proxy
{
    class HttpProxy : IProxy
    {
        private class FakeAsyncResult : IAsyncResult
        {
            public  HttpState InnerState { get; }
            private readonly IAsyncResult r;

            public FakeAsyncResult(IAsyncResult orig, HttpState state)
            {
                r = orig;
                InnerState = state;
            }

            public bool IsCompleted => r.IsCompleted;
            public WaitHandle AsyncWaitHandle => r.AsyncWaitHandle;
            public object AsyncState => InnerState.AsyncState;
            public bool CompletedSynchronously => r.CompletedSynchronously;

        }

        private class HttpState
        {
            public AsyncCallback Callback { get; set; }
            public object AsyncState { get; set; }
            public int BytesToRead { get; set; }
            public Exception Ex { get; set; }
        }

        private readonly WrappedSocket _remote = new WrappedSocket();
        public EndPoint LocalEndPoint => _remote.LocalEndPoint;
        public EndPoint ProxyEndPoint { get; private set; }
        public EndPoint DestEndPoint { get; private set; }

        public void BeginConnectProxy(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            ProxyEndPoint = remoteEP;
            _remote.BeginConnect(remoteEP, callback, state);
        }

        public void EndConnectProxy(IAsyncResult asyncResult)
        {
            _remote.EndConnect(asyncResult);
            _remote.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
        }

        private const string HTTP_CRLF = "\r\n";
        private const string HTTP_CONNECT_TEMPLATE =
            "CONNECT {0} HTTP/1.1" + HTTP_CRLF +
            "Host: {0}" + HTTP_CRLF +
            "Proxy-Connection: keep-alive" + HTTP_CRLF +
            "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36" + HTTP_CRLF +
            "" + HTTP_CRLF;

        public void BeginConnectDest(EndPoint destEndPoint, AsyncCallback callback, object state)
        {
            DestEndPoint = destEndPoint;
            string request = string.Format(HTTP_CONNECT_TEMPLATE, destEndPoint);
            var b = Encoding.UTF8.GetBytes(request);
            var st = new HttpState();
            st.Callback = callback;
            st.AsyncState = state;

            _remote.BeginSend(b, 0, b.Length,SocketFlags.None, HttpRequestSendCallback, st);
        }


        private void HttpRequestSendCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as HttpState;
            try
            {
                _remote.EndSend(ar);
                //
                new LineReader(_remote, OnLineRead, OnException, OnFinish, Encoding.UTF8, HTTP_CRLF,
                    1024, new FakeAsyncResult(ar, state));
            }catch(Exception ex)
            {
                state.Ex = ex;
                state.Callback?.Invoke(new FakeAsyncResult(ar,state));
            }
        }


        public  void EndConnectDest(IAsyncResult asyncResult)
        {
            HttpState state = (asyncResult as FakeAsyncResult).InnerState;
            if (state.Ex != null)
            {
                throw state.Ex;
            }
        }

        public void BeginSend(byte[] buffer, int offset, int size, SocketFlags flags, AsyncCallback callback,
            object state)
        {
            _remote.BeginSend(buffer, offset, size, flags, callback, state);
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            return _remote.EndSend(asyncResult);
        }

        public void BeginReceive(byte[] buffer, int offset, int size, SocketFlags flags, AsyncCallback callback,
            object state)
        {
            _remote.BeginReceive(buffer, offset, size, flags, callback, state);
        }

        public int EndReceive(IAsyncResult asyncResult)
        {
            return _remote.EndReceive(asyncResult);
        }

        public void Shutdown(SocketShutdown how)
        {
            _remote.Shutdown(how);
        }

        public void Close()
        {
            _remote.Close();
        }



    }



}
