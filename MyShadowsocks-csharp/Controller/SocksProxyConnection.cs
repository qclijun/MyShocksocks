using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Junlee.Util.Sockets;
using MyShadowsocks.Encryption;
using MyShadowsocks.Model;
using MyShadowsocks.Util;
using NLog;

namespace MyShadowsocks.Controller {
    public class SocksProxyConnection : ProxyConnection {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static IServerProvider Sp = new SimpleServerProvider();

        private Server _currentServer;

        private byte[] ClientCipherBuffer = new byte[BufferSize]; //用于保存加密和解密的结果
        private byte[] ServerCipherBuffer = new byte[BufferSize];



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
                logger.Debug("first package: " + Utils.DumpByteArray(firstPackage));


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
                logger.Debug("first package: " + Utils.DumpByteArray(ClientBuffer, 0, bytesRead));

                await Task.WhenAll(dispatch, StartConnect()).ContinueWith(async (ans) => {
                    await StartPipe();
                });


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
            var s = Sp.GetAServer();
            _currentServer = s;
            try {

                IPAddress[] addrList = Dns.GetHostAddresses(s.ServerName);
                int port = s.ServerPort;
                var timeout = s.Timeout;
                ServerSocket = new Socket(addrList[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //var Cts = new CancellationTokenSource();
                //Cts.CancelAfter(TimeSpan.FromSeconds(timeout));
                //await ServerSocket.ConnectTaskAsync(addrList, port, Cts.Token);

                Task conn = ServerSocket.ConnectTaskAsync(addrList, port);
                if(conn == await Task.WhenAny(conn, Task.Delay(TimeSpan.FromSeconds(timeout)))) {

                    await conn;
                    logger.Info("Socket connected to ss serer: " + this);


                } else {
                    logger.Info(s.FriendlyName() + " time out");
                    OnException();
                }

            } catch(Exception ex) {
                OnException($"failed to connect to ss server: {s.FriendlyName()}. {ex.Message}");
            }
        }


        private void HandleUDPAssociate() {
            throw new NotImplementedException();
        }

        #region StartPipe        

        //使用ServerBuffer和ServerCipherBuffer
        //catched all exception
        protected override async Task StartPipeS2C() {
            //decryptor变成局部变量，而不需要成为类的字段，当异步任务结束时（连接中断或出现异常时）释放decryptor
            //另一个选择是成为类的字段，调用Close()的时候释放
            var decryptor = Program.Cache.GetDecryptor(_currentServer);

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
            } catch(Exception ex) {
                OnException(ex.ToString());
            } finally {
                //free decryptor
                Program.Cache.FreeEncryptor(decryptor);
            }
        }

        //使用ClientBuffer和ClientCipherBuffer
        //catched all exception
        protected override async Task StartPipeC2S() {
            var encryptor = Program.Cache.GetEncryptor(_currentServer);
            bool firstTime = true;
            int bytesToSend = 0;

            try {
                while(true) {
                    Task<int> t1 = ClientSocket.ReceiveTaskAsync(ClientBuffer, 0, RecvSize, SocketFlags.None);

                    int bytes = await t1;

                    logger.Debug("Receive: {0} bytes from Client: {1}", bytes, this.ToString());
                    if(bytes == 0) {
                        logger.Debug("Close Connection from Client: " + this.ToString());
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

                    logger.Debug("Send: {0} bytes to Server: {1}", bytes, this.ToString());
                    if(bytes <= 0) {
                        logger.Error("Send failed: " + this);
                        throw new SocketException((int)SocketError.OperationAborted);
                    }
                }
            } catch(ObjectDisposedException) {
                //ignore: socket already closed
            } catch(Exception ex) {
                OnException(ex.ToString());
            } finally {
                Program.Cache.FreeEncryptor(encryptor);
            }
        }




        #endregion







    }

}
