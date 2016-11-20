using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using MyShadowsocks.Model;

namespace MyShadowsocks.Controller.Strategy
{
    public enum IStrategyCallerType
    {
        TCP,UDP
    }

    public interface IStrategy
    {
        string Name { get; }
        string ID { get; }
        void ReloadServers();
        Server GetAServer(IStrategyCallerType type, IPEndPoint localIPEndPoint);

        void UpdateLatency(Server server, TimeSpan latency);

        void UpdateLastRead(Server server);

        void UpdateLastWrite(Server server);

        void SetFailure(Server server);
    }
}
