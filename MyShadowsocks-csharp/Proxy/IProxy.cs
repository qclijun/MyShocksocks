using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Proxy
{
    public interface IProxy
    {
        EndPoint LocalEndPoint { get; }
        EndPoint ProxyEndPoint { get; }
        EndPoint DestEndPoint { get; }
        void BeginConnectProxy(EndPoint remoteEP, AsyncCallback callback, object state);
        void EndConnectProxy(IAsyncResult asyncResult);
        void BeginConnectDest(EndPoint destEndPoint, AsyncCallback callback, object state);
        void EndConnectDest(IAsyncResult asyncResult);
        void BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback,
            object state);
        int EndSend(IAsyncResult asyncResult);

        void BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback,
            object state);
        int EndReceive(IAsyncResult asyncResult);

        void Shutdown(SocketShutdown how);
        void Close();

    }
}
