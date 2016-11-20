using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Util.Sockets
{
    class LineReader
    {
        private WrappedSocket _socket;
        private Func<string, object, bool> _onLineRead;
        private Action<Exception, object> _onException;
        private Action<byte[], int, int, object> _onFinish;

        private Encoding _encoding;
        private byte[] _delimiterBytes;
        private int[] _delimiterSearchCharTable;
        private int[] _delimiterSearchOffsetTable;
        private object _state;

        private byte[] _lineBuffer;
        private int _bufferIndex;

        public LineReader(WrappedSocket socket,Func<string,object,bool> onLineRead,
            Action<Exception,object> onException,
            Action<byte[],int,int,object> onFinish,
            Encoding encoding, string delimiter, int maxLineBytes, object state)
        {
            Contract.Requires<ArgumentNullException>(socket != null, nameof(socket));
            Contract.Requires<ArgumentNullException>(onLineRead != null, nameof(onLineRead));
            Contract.Requires<ArgumentNullException>(encoding != null, nameof(encoding));
            Contract.Requires<ArgumentNullException>(delimiter != null, nameof(delimiter));

            _socket = socket;
            _onLineRead = onLineRead;
            _onException = onException;
            _onFinish = onFinish;
            _encoding = encoding;
            _state = state;

            _delimiterBytes = encoding.GetBytes(delimiter);
            if (_delimiterBytes.Length == 0) throw new ArgumentException("Too short!", nameof(delimiter));
            if (maxLineBytes < _delimiterBytes.Length)
                throw new ArgumentException("Too small!", nameof(maxLineBytes));

            _delimiterSearchCharTable = MakeCharTable(_delimiterBytes);
            _delimiterSearchOffsetTable = MakeOffsetTable(_delimiterBytes);
            _lineBuffer = new byte[maxLineBytes];

            socket.BeginReceive(_lineBuffer, 0, maxLineBytes, 0, ReceiveCallback, 0);

        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            int length = (int)ar.AsyncState;
            try
            {
                var bytesRead = _socket.EndReceive(ar);
                if (bytesRead == 0)
                {
                    OnFinish(length);
                    return;
                }
                length += bytesRead;

            }
        }


        private static  int[] MakeCharTable(byte[] needle)
        {
            const int ALPHABET_SIZE = 256;//byte 数量
            int[] table = new int[ALPHABET_SIZE];
            for(int i = 0; i < table.Length; ++i)
            {
                table[i] = needle.Length;
            }
            for(int i = 0; i < needle.Length - 1; ++i)
            {
                table[needle[i]] = needle.Length - 1 - i;
            }
            return table;
        }
    }
}
