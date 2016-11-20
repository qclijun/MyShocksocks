using System;
using System.Net;
using System.Net.Sockets;

namespace MyShadowsocks.Util.Sockets
{
    public static class SocketUtils
    {

        public static IPEndPoint GetEndPoint(string host, int port)
        {
            IPAddress[] addrList = Dns.GetHostAddresses(host);
            return new IPEndPoint(addrList[0], port);
        }

        public static void FullClose(this Socket s)
        {

            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }
            try
            {
                s.Disconnect(false);
            }
            catch (Exception)
            {
            }
            try
            {
                s.Close();
            }
            catch (Exception)
            {
            }
            try
            {
                s.Dispose();
            }
            catch (Exception)
            {
            }

        }
    }
}
