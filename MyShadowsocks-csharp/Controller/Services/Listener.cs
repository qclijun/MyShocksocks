using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

using MyShadowsocks.Model;
using System.Diagnostics;
using NLog;

namespace MyShadowsocks.Controller
{
    public class Listener
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private class UDPState
        {
            public Socket socket = null;
            public byte[] buffer = new byte[4096];
            public EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        //private Configuration _config;
        private bool _shareOverLAN = false;

        private Socket _tcpSocket;
        private Socket _udpSocket;
        private List<Service> _services ;

        private int _localPort = 1080;
        private IPAddress _localAddress = IPAddress.Loopback;


        public int LocalPort { get { return _localPort; } }

        public Listener()
        {
            _services = new List<Service>();
            
        } 


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

        public void Start()
        {
            
            if (CheckIfPortInUse(LocalPort))
                throw new WebException(I18N.GetString("Port already in use"));
            try
            {
                // Create a TCP/IP socket
                _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IPEndPoint localEndPoint = new IPEndPoint(_localAddress, LocalPort);

                // Bind the socket to the local endpoint and listen for incoming connections.
                _tcpSocket.Bind(localEndPoint);
                _udpSocket.Bind(localEndPoint);
                _tcpSocket.Listen(1024);

                // Start an aysnchronous socket to listen for connections.
                //Logging.Info("Shadowsocks  started.");
                logger.Info("Shadowsocks started.Listening localport {0} ...",LocalPort);

                _tcpSocket.BeginAccept(new AsyncCallback(AcceptCallback), _tcpSocket);
                UDPState udpState = new UDPState();
                udpState.socket = _udpSocket;
                _udpSocket.BeginReceiveFrom(udpState.buffer, 0, udpState.buffer.Length, 0,
                    ref udpState.remoteEndPoint, new AsyncCallback(RecvFromCallback), udpState);

            }
            catch (SocketException e)
            {
                //Trace.TraceError(e.Message);
                logger.Error( e.Message);
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
                logger.Error(e.Message);
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
                    logger.Error(e.Message);
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
                logger.Error(e.Message);
                conn.Close();
            }
        }


        //udp request handle
        private void RecvFromCallback(IAsyncResult ar)
        {
            UDPState state = (UDPState)ar.AsyncState;
            var socket = state.socket;
            try
            {
                int byteRead = socket.EndReceiveFrom(ar, ref state.remoteEndPoint);
                foreach (Service service in _services)
                {
                    if (service.Handle(state.buffer, byteRead, socket, state))
                        break;
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
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

    }
}
