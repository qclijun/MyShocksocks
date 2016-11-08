using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Shadowsocks.Model;


namespace Shadowsocks.Controller.Strategy
{
    using Statistics = Dictionary<string, List<StatisticsRecord>>;
    class StatisticsStrategy : IStrategy, IDisposable
    {
        private readonly ShadowsocksController _controller;
        private Server _currentServer;
        private readonly Timer _timer;
        private Statistics _filterStatistics;
        

        public StatisticsStrategy(ShadowsocksController controller)
        {


            //TODO::
            _controller = controller;

        }


        public string ID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Server GetAServer(IStrategyCallerType type, IPEndPoint localIPEndPoint)
        {
            throw new NotImplementedException();
        }

        public void ReloadServers()
        {
            throw new NotImplementedException();
        }

        public void SetFailure(Server server)
        {
            throw new NotImplementedException();
        }

        public void UpdateLastRead(Server server)
        {
            throw new NotImplementedException();
        }

        public void UpdateLastWrite(Server server)
        {
            throw new NotImplementedException();
        }

        public void UpdateLatency(Server server, TimeSpan latency)
        {
            throw new NotImplementedException();
        }
    }
}
