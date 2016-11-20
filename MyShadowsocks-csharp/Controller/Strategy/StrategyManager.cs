using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MyShadowsocks.Controller.Strategy;

namespace MyShadowsocks.Controller.Strategy
{
    class StrategyManager
    {
        private List<IStrategy> _strategies;

        public StrategyManager(ShadowsocksController controller)
        {
            _strategies = new List<IStrategy>();
            _strategies.Add(new BalancingStrategy(controller));
            _strategies.Add(new HighAvailabilityStrategy(controller));
            _strategies.Add(new StatisticsStrategy(controller));

            //TODO: load DLL plugins;
        }

        public IList<IStrategy> GetStrategies()
        {
            return _strategies;
        }
    }

}
