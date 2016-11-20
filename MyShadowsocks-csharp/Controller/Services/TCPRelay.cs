using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MyShadowsocks.Encryption;
using MyShadowsocks.Model;
using MyShadowsocks.Proxy;

namespace MyShadowsocks.Controller
{
    class TCPRelay : Service
    {
        private ShadowsocksController _controller;
        private DateTime _lastSweepTime;
        private Configuration _config;

        public ISet<TCPHandler> Handlers { get; private set; }

        public TCPRelay(ShadowsocksController controller, Configuration config)
        {
            _controller = controller;
            _config = config;
            Handlers = new HashSet<TCPHandler>();
            _lastSweepTime = DateTime.Now;
        }

        public override bool Handle(byte[] firstPacket, int length, Socket socket, object state)
        {
            if (socket.ProtocolType != ProtocolType.Tcp
                || (length < 2 || firstPacket[0] != 55))
                return false;
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            //TCPHandler handler = new

                return true;
        }
    }

    class TCPHandler
    {
        class AsyncSession
        {
            public IProxy Remote { get;}
            public AsyncSession(IProxy remote)
            {
                Remote = remote;
            }
        }

        class AsyncSession<T> : AsyncSession
        {
            public T State { get; set; }
            public AsyncSession(IProxy remote, T state) : base(remote)
            {
                State = state;
            }

            public AsyncSession(AsyncSession session, T state) : this(session.Remote,state)
            {
            }

            private readonly int _serverTimeout;
            private readonly int _proxyTimeout;

            public const int RecvSize = 8192;
            public const int RecvReserveSize = 1024; // TODO: modify this
            public const int BufferSize = RecvSize + RecvReserveSize + 32;

            public DateTime LastActivity { get; set; }

            private ShadowsocksController _controller;
            private Configuration _config;
            private TCPRelay _tcpRelay;
            private Socket _connectedSocket;

            private IEncryptor _encryptor;
            private Server _server;

            private AsyncSession _currentRemoteSession;

            private bool _proxyConnected;
            private bool _destConnected;

            private byte _command;
            private byte[] _firstPacket;
            private int _firstPacketLength;

            private int _totalRead = 0;
            private int _totalWrite = 0;

            private byte[] _remoteRecvBuffer = new byte[BufferSize];
            private byte[] _remoteSendBuffer = new byte[BufferSize];
            private byte[] _connectionRecvBuffer = new byte[BufferSize];
            private byte[] _connectionSendBuffer = new byte[BufferSize];

            private bool _connectionShutdown = false;
            private bool _remoteShutdown = false;
            private bool _closed = false;

            private readonly object _encryptionLock = new object();
            private readonly object _decryptionLock = new object();
            private readonly object _closeConnLock = new object();

            private DateTime _startConnectTime;
            private DateTime _startRecevingTime;
            private DateTime _startSendingTime;

        }

    }
}
