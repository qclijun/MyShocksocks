using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Junlee.Util.Sockets {
    public static class SocketAsyncExt {

        public static Task<int> ReceiveTaskAsync(this Socket socket, byte[] buffer,
            int offset, int size, SocketFlags socketFlags) {
            var tcs = new TaskCompletionSource<int>();
            socket.BeginReceive(buffer, offset, size, socketFlags, ar => {
                try {
                    tcs.SetResult(socket.EndReceive(ar));
                } catch(Exception ex) {
                    tcs.SetException(ex);
                }
            }, null);
            return tcs.Task;
        }

        public static Task<int> ReceiveTaskAsync(this Socket socket, byte[] buffer,
      int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken) {
            var tcs = new TaskCompletionSource<int>();
            var reg = cancellationToken.Register(() => tcs.TrySetCanceled());
            socket.BeginReceive(buffer, offset, size, socketFlags, ar => {
                try {
                    tcs.SetResult(socket.EndReceive(ar));
                } catch(Exception ex) {
                    tcs.TrySetException(ex);
                }
            }, null);
            return tcs.Task;
        }

        public static Task<int> SendTaskAsync(this Socket socket, byte[] buffer,
            int offset, int size, SocketFlags socketFlags) {
            var tcs = new TaskCompletionSource<int>();
            socket.BeginSend(buffer, offset, size, socketFlags, ar => {
                try {
                    tcs.SetResult(socket.EndSend(ar));
                } catch(Exception ex) {
                    tcs.SetException(ex);
                }
            }, null);
            return tcs.Task;
        }

        public static Task<int> SendTaskAsync(this Socket socket, byte[] buffer,
            int offset, int size, SocketFlags socketFlags,CancellationToken cancellationToken) {
            var tcs = new TaskCompletionSource<int>();
            var reg = cancellationToken.Register(() => tcs.TrySetCanceled());
            socket.BeginSend(buffer, offset, size, socketFlags, ar => {
                try {
                    reg.Dispose();
                    tcs.SetResult(socket.EndSend(ar));
                } catch(Exception ex) {
                    tcs.TrySetException(ex);
                }
            }, null);
            return tcs.Task;
        }


        public static Task ConnectTaskAsync(this Socket socket, IPAddress[] addresses, int port) {
            return ConnectTaskAsync(socket, addresses, port, CancellationToken.None);
        }

        public static Task ConnectTaskAsync(this Socket socket, IPAddress[] addresses, int port,
            CancellationToken cancellationToken) {
            var tcs = new TaskCompletionSource<object>();
            var reg = cancellationToken.Register(() => tcs.TrySetCanceled());
            socket.BeginConnect(addresses, port, ar => {
                try {
                    reg.Dispose();
                    socket.EndConnect(ar);
                    tcs.TrySetResult(null);
                } catch(Exception ex) {
                    tcs.TrySetException(ex);
                }
            }, null);
            return tcs.Task;
        }

        public static Task<Socket> AcceptTaskAsync(this Socket socket, CancellationToken cancellationToken) {
            var tcs = new TaskCompletionSource<Socket>();
            var reg = cancellationToken.Register(() => tcs.TrySetCanceled());
            socket.BeginAccept(ar => {
                try {
                    reg.Dispose();
                    tcs.TrySetResult(socket.EndAccept(ar));
                } catch(Exception ex) {
                    tcs.TrySetException(ex);
                }
            }, null);            
            return tcs.Task;
        }


        public static Task<Socket> AcceptTaskAsync(this Socket socket) {
            return AcceptTaskAsync(socket, CancellationToken.None);
        }

        public static void FullClose(this Socket s) {
            try { s.Shutdown(SocketShutdown.Both); } catch { }
            s.Close();
            s.Dispose();
        }

    }
}
