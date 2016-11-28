using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jun;
using Jun.Net;
using MyShadowsocks.Controller.Strategy;
using MyShadowsocks.Model;
using MyShadowsocks.Util;
using NLog;

namespace MyShadowsocks.Controller {
    sealed class SocksProxyConnection : ProxyConnection {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static IServerSelector Sp = null;
        private static EncryptorPool Pool = new EncryptorPool();

        private Server _currentServer;

        private byte[] ClientCipherBuffer = new byte[BufferSize]; //用于保存加密和解密的结果
        private byte[] ServerCipherBuffer = new byte[BufferSize];


        static SocksProxyConnection() {
            SetStrategy(MyShadowsocksController.Config.Strategy);
        }

        internal static void SetStrategy(string name) {
            Sp = ServerSelectorManager.GetSelector(name);
            if(Sp is FixedServerSelector) {
                (Sp as FixedServerSelector).SetIndex(MyShadowsocksController.Config.Index);
            }
        }


        public SocksProxyConnection(Socket requestSocket) : base(requestSocket) {


        }


        public override Task ProcessRequest(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override async Task StartHandshake() {
            try {
                byte[] firstPackage = await ReadFirstRequest();
                if(firstPackage.Length < 2)
                    throw new SocketException((int)SocketError.ProtocolType);
                logger.Debug("receive {0} bytes from {1}", firstPackage.Length, ClientSocket.RemoteEndPoint);
                logger.Debug("first package: " + Jun.Utils.DumpByteArray(firstPackage));


                byte[] respose = { 5, 0 };
                if(firstPackage[0] != 5) {
                    //reject socks 4
                    respose = new byte[] { 0, 91 };
                    logger.Error("socks 5 protocol error");

                }
                await ClientSocket.SendTaskAsync(respose, 0, respose.Length, SocketFlags.None);
                int bytesRead = await ClientSocket.ReceiveTaskAsync(ClientBuffer, 0, 3,
                    SocketFlags.None);
                if(bytesRead < 3) {
                    OnException("failed to recv three bytes from client");
                    return;
                }

                Task dispatch = DispatchCmd(ClientBuffer, bytesRead);

                logger.Debug("receive {0} bytes from {1}", bytesRead, ClientSocket.RemoteEndPoint);
                logger.Debug("first package: " + Jun.Utils.DumpByteArray(ClientBuffer, 0, bytesRead));


                await Task.WhenAll(dispatch, StartConnect()).ContinueWith((ans) => {
                    StartPipe();
                }, TaskContinuationOptions.OnlyOnRanToCompletion);


            } catch(Exception ex) {
                OnException(ex);
            }
        }

        private async Task DispatchCmd(byte[] data, int length) {
            Contract.Assert(length == 3);

            if(data[1] == 1) {
                byte[] response = { 5, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
                int bytes = await ClientSocket.SendTaskAsync(response, 0, response.Length, SocketFlags.None);
                logger.Debug("send {0} bytes to {1}", bytes, ClientSocket.RemoteEndPoint);

            } else if(data[1] == 3) {
                HandleUDPAssociate();
            } else {
                throw new SocketException((int)SocketError.ProtocolType);
            }
        }

        public override string ToString() {
            try {
                if(ConnectedToServer) {
                    return ClientSocket.RemoteEndPoint + " <----> "
                        + _currentServer;
                } else {
                    return "Connection from " + ClientSocket.RemoteEndPoint;
                }
            } catch {
                return "Connection";
            }
        }



        private async Task StartConnect() {
            int index = Sp.GetAServerIndex();
            var s = MyShadowsocksController.ServerList[index];
            _currentServer = s;
            try {

                IPAddress[] addrList = Dns.GetHostAddresses(s.HostName);
                int port = s.ServerPort;
                var timeout = s.Timeout;
                ServerSocket = new Socket(addrList[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //var Cts = new CancellationTokenSource();
                //Cts.CancelAfter(TimeSpan.FromSeconds(timeout));
                //await ServerSocket.ConnectTaskAsync(addrList, port, Cts.Token);

                Task conn = ServerSocket.ConnectTaskAsync(addrList, port);
                if(conn == await Task.WhenAny(conn, Task.Delay(TimeSpan.FromSeconds(timeout)))) {

                    await conn;
                    AddConnection(this);
                    logger.Info("New connection  {0}, Current connection count: {1}", this, ConnectionCount);


                } else {
                    logger.Info("failed to connect ss server " + s.ToString() + ": time out");
                    Sp.SetFailure(index);
                    OnException();
                }

            } catch(SocketException ex) {
                Sp.SetFailure(index);

                OnException($"Failed to connect  ss server {s.ToString()}. SocketException: {ex.SocketErrorCode}");
            } catch(Exception ex) {
                Sp.SetFailure(index);

                OnException($"Failed to connect  ss server {s.ToString()}. {ex.TypeAndMessage()}");
            }
        }


        private void HandleUDPAssociate() {
            throw new NotImplementedException();
        }

        #region StartPipe        

        //使用ServerBuffer和ServerCipherBuffer

        protected override async Task StartPipeS2C() {
            //decryptor变成局部变量，而不需要成为类的字段，当异步任务结束时（连接中断或出现异常时）释放decryptor
            //另一个选择是成为类的字段，调用Close()的时候释放
            var decryptor = Pool.GetDecryptor(_currentServer);

            bool firstTime = true;
            int bytesToSend = 0;
            try {

                while(true) {

                    Task<int> t1 = ServerSocket.ReceiveTaskAsync(ServerBuffer, 0, RecvSize, SocketFlags.None);
                    int bytes = await t1;

                    logger.Debug("Receive: {0} bytes from Server: {1}", bytes, this.ToString());
                    if(bytes == 0) {
                        logger.Debug("Close Connection from Server: " + this);
                        Close();
                        break;
                    }

                    if(firstTime) {
                        decryptor.DecryptFirstPackage(ServerBuffer, bytes, ServerCipherBuffer, out bytesToSend);
                        firstTime = false;
                    } else {
                        decryptor.Decrypt(ServerBuffer, bytes, ServerCipherBuffer, out bytesToSend);
                    }


                    bytes = await ClientSocket.SendTaskAsync(ServerCipherBuffer, 0, bytesToSend, SocketFlags.None);
                    logger.Debug("Send: {0} bytes to Client: {1}", bytes, this.ToString());
                    if(bytes <= 0) {
                        logger.Error("Send failed: " + this);
                        throw new SocketException((int)SocketError.OperationAborted);
                    }



                }//End while
            } catch(ObjectDisposedException) {
                //ignore: socket already closed
            } finally {
                //free decryptor
                Pool.FreeEncryptor(decryptor);
            }
        }

        //使用ClientBuffer和ClientCipherBuffer

        protected override async Task StartPipeC2S() {
            var encryptor = Pool.GetEncryptor(_currentServer);
            bool firstTime = true;
            int bytesToSend = 0;

            try {
                while(true) {
                    Task<int> t1 = ClientSocket.ReceiveTaskAsync(ClientBuffer, 0, RecvSize, SocketFlags.None);

                    int bytes = await t1;

                    logger.Debug("Receive: {0} bytes from Client: {1}", bytes, this);
                    if(bytes == 0) {
                        logger.Debug("Close Connection from Client: " + this);
                        Close();
                        break;
                    }


                    if(firstTime) {
                        encryptor.EncryptFirstPackage(ClientBuffer, bytes, ClientCipherBuffer, out bytesToSend);
                        firstTime = false;
                    } else {
                        encryptor.Encrypt(ClientBuffer, bytes, ClientCipherBuffer, out bytesToSend);
                    }


                    bytes = await ServerSocket.SendTaskAsync(ClientCipherBuffer, 0, bytesToSend, SocketFlags.None);

                    logger.Debug("Send: {0} bytes to Server: {1}", bytes, this);
                    if(bytes <= 0) {
                        logger.Error("Send failed: " + this);
                        throw new SocketException((int)SocketError.OperationAborted);
                    }
                }
            } catch(ObjectDisposedException) {
                //ignore: socket already closed
            } finally {
                Pool.FreeEncryptor(encryptor);
            }
        }




        #endregion







    }

}
