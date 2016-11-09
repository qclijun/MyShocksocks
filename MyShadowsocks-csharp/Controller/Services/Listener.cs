using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

using Shadowsocks.Model;

namespace Shadowsocks.Controller
{
    public class Listener
    {
        

        class UDPState
        {
            public Socket socket = null;
            public byte[] buffer = new byte[4096];
            public EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        private Configuration _config;
        private bool _shareOverLAN;
        private Socket _tcpSocket;
        private Socket _udpSocket;
        private List<Service> _services;

        public Listener(List<Service> services)
        {
            this._services = services;
        }
        private static bool CheckIfPortInUse(int port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach(IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port) return true;
            }
            return false;
        }

        public void Start(Configuration config)
        {
            this._config = config;
            this._shareOverLAN = config.ShareOverLan;
            if (CheckIfPortInUse(config.LocalPort))
                throw new Exception(I18N.GetString("Port already in use"));
            try
            {
                // Create a TCP/IP socket
                _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IPEndPoint localEndPoint = _shareOverLAN ? new IPEndPoint(IPAddress.Any, _config.LocalPort)
                    : new IPEndPoint(IPAddress.Loopback, _config.LocalPort);

                // Bind the socket to the local endpoint and listen for incoming connections.
                _tcpSocket.Bind(localEndPoint);
                _udpSocket.Bind(localEndPoint);
                _tcpSocket.Listen(1024);

                // Start an aysnchronous socket to listen for connections.
                Logging.Info("Shadowsocks  started.");
                _tcpSocket.BeginAccept(new AsyncCallback(AcceptCallback), _tcpSocket);
                UDPState udpState = new UDPState();
                udpState.socket = _udpSocket;
                _udpSocket.BeginReceiveFrom(udpState.buffer, 0, udpState.buffer.Length, 0,
                    ref udpState.remoteEndPoint, new AsyncCallback(RecvFromCallback), udpState);


            }
            catch (SocketException)
            {
                _tcpSocket.Close();
                throw;
            }
        }

        public void Stop()
        {
            if(_tcpSocket!=null)
            {
                _tcpSocket.Close();
                _tcpSocket = null;
            }
            if (_udpSocket != null)
            {
                _udpSocket.Close();
                _udpSocket = null;
            }
            _services.ForEach(s => s.Stop());
        }

        private void RecvFromCallback(IAsyncResult ar)
        {
            UDPState state = (UDPState)ar.AsyncState;
            var socket = state.socket;
            try
            {
                int byteRead = socket.EndReceiveFrom(ar, ref state.remoteEndPoint);
                foreach(Service service in _services)
                {
                    if (service.Handle(state.buffer, byteRead, socket, state))
                        break;
                }
            }
            catch (ObjectDisposedException)
            {

            }catch(Exception ex)
            {
                Logging.Debug(ex);
            }
            finally
            {
                try
                {
                    socket.BeginReceiveFrom(state.buffer, 0, state.buffer.Length, 0,
                        ref state.remoteEndPoint, new AsyncCallback(RecvFromCallback),
                        state);
                }
                catch (ObjectDisposedException) { }
                catch (Exception) { }
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            try
            {
                Socket conn = listener.EndAccept(ar);
                byte[] buf = new byte[4096];
                object[] state = new object[] { conn, buf };
                conn.BeginReceive(buf, 0, buf.Length, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (ObjectDisposedException) { }
            catch(Exception e)
            {
                Logging.LogUsefulException(e);
            }
            finally
            {
                try
                {
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                }
                catch (ObjectDisposedException) { }
                catch(Exception e)
                {
                    Logging.LogUsefulException(e);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            object[] state = (object[])ar.AsyncState;
            Socket conn = (Socket)state[0];
            byte[] buf = (byte[])state[1];
            try
            {
                int byteRead = conn.EndReceive(ar);
                foreach(Service service in _services)
                {
                    if (service.Handle(buf, byteRead, conn, null))
                        return;
                }
                // no service found for this
                if (conn.ProtocolType == ProtocolType.Tcp)
                    conn.Close();
            }catch(Exception e)
            {
                Logging.LogUsefulException(e);
                conn.Close();
            }
        }



    }
}
