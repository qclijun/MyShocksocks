using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MyShadowsocks.Controller.Core
{
    struct Buffer
    {
        public readonly byte[] Data;
        public int Offset;
        public int Count;

        public Buffer(byte[] data, int offset, int count)
        {
            Data = data;
            Offset = offset;
            Count = count;
        }

        public bool IsNull()  { return Data == null; } 

        public static readonly Buffer NoneBuffer = new Buffer(null, 0, 0);
    }



    /// <summary>
    /// 一块连续的内存，它被分为固定大小的小块，以避免重复的分配与释放
    /// 它是线程安全的。
    /// </summary>
    class BufferManager
    {
        private readonly int _bufferSize;
        private readonly int _totalBytes;
        private readonly byte[] _data;
        private int _index = 0;
        //private Stack<int> freeBuffers;
        private ConcurrentBag<int> _freeBuffers;

        public int BufferSize { get { return _bufferSize; } }
        public int TotalBytes { get { return _totalBytes; } }

        public byte[] Data { get { return _data; } }

        public BufferManager(int bufferSize, int totalBytes)
        {
            this._bufferSize = bufferSize;
            this._totalBytes = totalBytes;
            _data = new byte[totalBytes];
            //freeBuffers = new Stack<int>();
            _freeBuffers = new ConcurrentBag<int>();
        }


        public Buffer GetBuffer()
        {
            int offset;
            bool hasElement = _freeBuffers.TryTake(out offset);
            if (hasElement)
            {
                return new Buffer(Data, offset, BufferSize);
            }
            else
            {
                if (_index + BufferSize > TotalBytes) return Buffer.NoneBuffer;
                Buffer buf = new Buffer(_data, _index, BufferSize);
                _index += BufferSize;
                return buf;
            }

        }

        public void FreeBuffer(Buffer buf)
        {
            _freeBuffers.Add(buf.Offset);
        }




    }
}
