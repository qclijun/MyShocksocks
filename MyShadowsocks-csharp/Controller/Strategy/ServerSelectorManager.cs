using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Controller.Strategy {
    static class ServerSelectorManager {

        
        //默认为FailureRank
        public static IServerSelector GetSelector(string name) {
            
            IServerSelector result;
            if(_selectorDict.TryGetValue(name,out result)) {
                return new FailureRankServerSelector();
            }
            return result;
        }

        internal static IEnumerable<string> SupportedServerSelectors => _selectorDict.Keys;

        private static Dictionary<string, IServerSelector> _selectorDict = new Dictionary<string, IServerSelector>() {
            { "Fixed",new FixedServerSelector() },
            {"Random",new RandomServerSelector() },
            {"FailureRank", new FailureRankServerSelector() }
        };

    }
}
