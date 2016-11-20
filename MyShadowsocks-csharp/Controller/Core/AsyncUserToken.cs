using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace MyShadowsocks.Controller.Core
{
    class AsyncUserToken
    {

        public Socket WorkSocket { get; set; }

        public Socket RemoteSocket { get; set; }

        public bool IsConnected { get; set; } = false; // 与remote连接

        public bool ReceiveCompleted { get; set; }

        private BufferManager _recvBufManager;
        private List<Core.Buffer> _recvBufList;
        

        public int BytesReceived { get; set; }
        public bool IsFromRemote { get; set; } = false; //recvBuf 

        public bool IsSendRemote { get; set; } = false;

        public int BufferCount { get { return _recvBufList.Count; } }


        public Core.Buffer ConnectBytes { get; set; }


        public AsyncUserToken(BufferManager recvBufManager)
        {
            _recvBufList = new List<Core.Buffer>();
            this._recvBufManager = recvBufManager;

        }


        public void AddReceiveBuffer(SocketAsyncEventArgs e)
        {
            Debug.Assert(e.Buffer == _recvBufManager.Data);

            _recvBufList.Add(new Core.Buffer(e.Buffer, e.Offset, e.BytesTransferred));
            BytesReceived += e.BytesTransferred;
        }




        internal Core.Buffer GetReceiveBuf()
        {
            Debug.Assert(ReceiveCompleted);
            Debug.Assert(_recvBufList.Count >= 1);

            //if (_recvBufList.Count == 1)
            //{
            //    return _recvBufList[0];
            //}
            byte[] newData = new byte[BytesReceived];
            int index = 0;
            foreach (var buf in _recvBufList)
            {
                //Array.Copy(buf.Data, buf.Offset, newData, index, buf.Count);
                System.Buffer.BlockCopy(buf.Data, buf.Offset, newData, index, buf.Count);
                index += buf.Count;
                _recvBufManager.FreeBuffer(buf);
            }

            return new Core.Buffer(newData, 0, newData.Length);
        }

        public void ClearReceiveBuf()
        {
            BytesReceived = 0;
            //foreach (var buf in _recvBufList)
            //{
            //    _recvBufManager.FreeBuffer(buf);
            //}
            _recvBufList.Clear();
            ReceiveCompleted = false;
        }

        public void Clear()
        {
            
            ClearReceiveBuf();
            if (WorkSocket != null) WorkSocket = null;
            if (RemoteSocket != null) RemoteSocket = null;
            IsConnected = false;
            IsFromRemote = false;
            IsSendRemote = false;
        }
    }
}
