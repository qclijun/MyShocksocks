using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Shadowsocks.Controller
{
    public interface IService
    {
        bool Handle(byte[] firstPacket, int length, Socket socket, object state);
        void Stop();
    }

    public abstract class Service : IService
    {
        public abstract bool Handle(byte[] firstPacket, int length, Socket socket, object state);
        public virtual void Stop() { }
    }
}
