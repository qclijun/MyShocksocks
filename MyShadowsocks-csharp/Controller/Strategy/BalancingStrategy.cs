using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Shadowsocks.Model;

namespace Shadowsocks.Controller.Strategy
{
    class BalancingStrategy : IStrategy
    {
        private ShadowsocksController _controller;
        private Random _random;

        public BalancingStrategy(ShadowsocksController controller)
        {
            _controller = controller;
            _random = new Random();
        }

        public string ID
        {
            get
            {
                return "com.shadowsocks.strategy.balancing";
            }
        }

        public string Name
        {
            get
            {
                return I18N.GetString("Load Balance");
            }
        }

        public Server GetAServer(IStrategyCallerType type, IPEndPoint localIPEndPoint)
        {
            var configs = _controller.Config.GetServers();
            int index = 0;
            if (type == IStrategyCallerType.TCP)
            {
                index = _random.Next();
            }
            else
            {
                index = localIPEndPoint.GetHashCode();
            }
            return configs[index % configs.Count];
        }

        public void ReloadServers()
        {
            //throw new NotImplementedException();
        }

        public void SetFailure(Server server)
        {
            //throw new NotImplementedException();
        }

        public void UpdateLastRead(Server server)
        {
            //throw new NotImplementedException();
        }

        public void UpdateLastWrite(Server server)
        {
            //throw new NotImplementedException();
        }

        public void UpdateLatency(Server server, TimeSpan latency)
        {
            //throw new NotImplementedException();
        }
    }
}
