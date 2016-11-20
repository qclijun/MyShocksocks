using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MyShadowsocks.Util.Sockets;

namespace MyShadowsocks.Proxy
{
    class DirectConnect : IProxy
    {
        private class FakeEndPoint : EndPoint
        {
            public override AddressFamily AddressFamily => AddressFamily.Unspecified;
            public override string ToString()
            {
                return "null proxy";
            }

        }

        private WrappedSocket _remote = new WrappedSocket();



        public EndPoint DestEndPoint { get; private set; }


        public EndPoint LocalEndPoint
            => _remote.LocalEndPoint;


        public EndPoint ProxyEndPoint
        {
            get { return new FakeEndPoint(); }
        }


        public void BeginConnectDest(EndPoint destEndPoint, AsyncCallback callback, object state)
        {
            DestEndPoint = destEndPoint;
            _remote.BeginConnect(destEndPoint, callback, state);
        }

        public void BeginConnectProxy(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            var r = new FakeAsyncResult
            {
                AsyncState = state,
            };
            callback?.Invoke(r);
        }

        public void BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            //do nothing
        }

        public void BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            _remote.BeginSend(buffer, offset, size, socketFlags, callback, state);
        }

        public void Close()
        {
            _remote.Close();
        }

        public void EndConnectDest(IAsyncResult asyncResult)
        {
            _remote.EndConnect(asyncResult);
            _remote.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
        }

        public void EndConnectProxy(IAsyncResult asyncResult)
        {
            //do nothing
        }

        public int EndReceive(IAsyncResult asyncResult)
        {
            return _remote.EndReceive(asyncResult);
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            return _remote.EndSend(asyncResult);
        }

        public void Shutdown(SocketShutdown how)
        {
            _remote.Shutdown(how);
        }
    }
}
