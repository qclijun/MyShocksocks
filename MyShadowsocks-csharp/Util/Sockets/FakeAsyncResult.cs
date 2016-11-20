using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyShadowsocks.Util.Sockets
{
    class FakeAsyncResult : IAsyncResult
    {
        public bool IsCompleted { get; } = true;
        public WaitHandle AsyncWaitHandle { get; } = null;
        public object AsyncState { get; set; } = null;
        public bool CompletedSynchronously { get; } = true;
        public Exception InternalException { get; set; } = null;
    }
}
