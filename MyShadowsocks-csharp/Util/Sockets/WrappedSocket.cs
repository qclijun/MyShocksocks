using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyShadowsocks.Util.Sockets
{
    class WrappedSocket:IDisposable
    {
        public EndPoint LocalEndPoint => _activeSocket?.LocalEndPoint;

        //only used during connection and close, so it won't cost too much.
        private SpinLock _socketSyncLock = new SpinLock();

        private volatile bool _disposed;

        private bool Connected => _activeSocket != null;

        private Socket _activeSocket;

        private class TcpUserToken
        {
            public AsyncCallback Callback { get; }
            public object AsyncState { get; }

            public TcpUserToken(AsyncCallback callback, object state)
            {
                Callback = callback;
                AsyncState = state;
            }
        }

        public WrappedSocket()
        {

        }

        public void BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);

            }
            if (Connected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }
            var arg = new SocketAsyncEventArgs();
            arg.RemoteEndPoint = remoteEP;
            arg.UserToken = new TcpUserToken(callback, state);
            arg.Completed += OnTcpConnectCompleted;
            if (!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, arg))
            {
                ProcessTcpConnect(arg);
            }
        }

        private void OnTcpConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessTcpConnect(e);
        }

        private void ProcessTcpConnect(SocketAsyncEventArgs e)
        {
            using (e)
            {
                e.Completed -= OnTcpConnectCompleted;
                var token = e.UserToken as TcpUserToken;
                if (e.SocketError != SocketError.Success)
                {
                    var ex = e.ConnectByNameError ?? new SocketException((int)e.SocketError);
                    var r = new FakeAsyncResult
                    {
                        AsyncState = token.AsyncState,
                        InternalException = ex,
                    };
                    token.Callback(r);
                }
                else
                {
                    var lockTaken = false;
                    if (!_socketSyncLock.IsHeldByCurrentThread)
                    {
                        _socketSyncLock.TryEnter(ref lockTaken);
                    }
                    try
                    {
                        if (Connected)
                        {
                            e.ConnectSocket.FullClose();
                        }
                        else
                        {
                            _activeSocket = e.ConnectSocket;
                            if (_disposed)
                            {
                                _activeSocket.FullClose();
                                _activeSocket = null;
                            }
                            var r = new FakeAsyncResult
                            {
                                AsyncState = token.AsyncState,
                            };
                            token.Callback(r);
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            _socketSyncLock.Exit();
                        }
                    }
                }
            }
        }


        public void EndConnect(IAsyncResult asyncResult)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            var r = asyncResult as FakeAsyncResult;
            if (r == null)
            {
                throw new ArgumentException("Invalid asyncResult.", nameof(asyncResult));
            }
            if (r.InternalException != null)
            {
                throw r.InternalException;
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            var lockTaken = false;
            if (!_socketSyncLock.IsHeldByCurrentThread)
                _socketSyncLock.TryEnter(ref lockTaken);
            try
            {
                if (disposing)
                {
                    //free managed resources
                    _activeSocket.FullClose();
                }
                //free unmanaged resources

            }
            finally
            {
                if (lockTaken) _socketSyncLock.Exit();
                _disposed = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }

        ~WrappedSocket()
        {
            Dispose(false);
        }

        public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags flags,
            AsyncCallback callback, object state)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)

                throw new SocketException((int)SocketError.NotConnected);
            return _activeSocket.BeginSend(buffer, offset, size, flags, callback, state);
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
                throw new SocketException((int)SocketError.NotConnected);
            return _activeSocket.EndSend(asyncResult);
        }

        public IAsyncResult BeginReceive(byte[] buffer,int offset, int size, SocketFlags flags,
            AsyncCallback callback,object state)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
                throw new SocketException((int)SocketError.NotConnected);
            return _activeSocket.BeginSend(buffer, offset, size, flags, callback, state);
        }

        public int EndReceive(IAsyncResult asyncResult)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
                throw new SocketException((int)SocketError.NotConnected);
            return _activeSocket.EndReceive(asyncResult);
        }

        public void Shutdown(SocketShutdown how)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
                return;
            _activeSocket.Shutdown(how);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            SetSocketOption(optionLevel, optionName, optionValue ? 1 : 0);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            if (!Connected)
                throw new SocketException((int)SocketError.NotConnected);
            _activeSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }

    }
}
