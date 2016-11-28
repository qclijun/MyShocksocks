using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyShadowsocks.Model;

namespace MyShadowsocks.Controller.Strategy {
    class FixedServerSelector : IServerSelector {
        private int _index;

        public string Name {
            get {
                if(CultureInfo.CurrentCulture.Name.StartsWith("zh"))
                    return "固定策略";
                else return "Fixed";
            }
        }

        public FixedServerSelector(int index = 0) {
            _index = index;
        }

        public void SetIndex(int index) {
            _index = index;
        }


        public int GetAServerIndex() {
            return _index;
        }

        public void SetFailure(int index) {
            // do nothing
        }
    }

    class RandomServerSelector : IServerSelector {
        private Random rand = new Random();

        public int GetAServerIndex() {
            return rand.Next(MyShadowsocksController.ServerList.Count);
        }

        public void SetFailure(int index) {
            //do nothing
        }

        public string Name {
            get {
                if(CultureInfo.CurrentCulture.Name.StartsWith("zh"))
                    return "随机策略";
                else return "Random";
            }
        }
    }

    class FailureRankServerSelector : IServerSelector {

        int[] _failureCount;
        private int _bestServerIndex;

        public FailureRankServerSelector() {
            _failureCount = new int[MyShadowsocksController.ServerList.Count];
            _bestServerIndex = 0;
            MyShadowsocksController.ServersChanged += (sender, e) => Reset();
        }

        private void Reset() {
            _failureCount = new int[MyShadowsocksController.ServerList.Count];
            _bestServerIndex = 0;
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

        public string Name {
            get {
                if(CultureInfo.CurrentCulture.Name.StartsWith("zh"))
                    return "失连评级策略";
                else return "FailureRank";
            }
        }


    }



}
