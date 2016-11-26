using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyShadowsocks.Model;

namespace MyShadowsocks.Controller {
    class SimpleServerProvider : IServerProvider {
        
       

        public int GetAServerIndex() {
            return 0;
        }

        public void SetFailure(int index) {
            // do nothing
        }
    }

    class RandomServerProvider : IServerProvider {
        private Random rand = new Random();

        public int GetAServerIndex() {
            return rand.Next(MyShadowsocksController.ServerList.Count);
        }

        public void SetFailure(int index) {
            //do nothing
        }
    }

    class FailureRankServerProvider : IServerProvider {
        
        int[] _failureCount;
        private int _bestServerIndex;

        public FailureRankServerProvider() {            
            _failureCount = new int[MyShadowsocksController.ServerList.Count];
            _bestServerIndex = 0;
            MyShadowsocksController.Timer_ServersUpdated += (sender, e) => Reset();
        }

        private void Reset() {
            _failureCount = new int[MyShadowsocksController.ServerList.Count];
            _bestServerIndex = 0;
        }

        public Server GetAServer() {
            
            return MyShadowsocksController.ServerList[_bestServerIndex];
        }

        public int GetAServerIndex() {
            
            return _bestServerIndex;
        }


        //因为GetAServer()比SetFailure()要更常用，
        //所以将更新_failureCount和_bestSererIndex的工作交给SetFailure
        public void SetFailure(int index) {
            //写_failureCount需锁定
            Interlocked.Increment(ref _failureCount[index]);
            if(_bestServerIndex != index) return;
            int min = _failureCount[0];
            int minIndex = _bestServerIndex;
            for(int i = 1;i < MyShadowsocksController.ServerList.Count;++i) {
                if(_failureCount[i] < min) {
                    min = _failureCount[i];
                    minIndex = i;
                }
            }
            //写_bestServerIndex需锁定
            Interlocked.Exchange(ref _bestServerIndex, minIndex);
        }
    }

}
