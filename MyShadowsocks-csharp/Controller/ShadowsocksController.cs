using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Shadowsocks.Model;
using Shadowsocks.Controller.Strategy;
using System.IO;

namespace Shadowsocks.Controller
{
    public class ShadowsocksController
    {
        private Thread _ramThread;
        private Thread _trafficThread;

        private Listener _listener;
        private PACServer _pacServer;
       
        public Configuration Config  => Configuration.Instance;

        private StrategyManager _strategyManager;

        private PrivoxyRunner _privoxyRunner;
        private GFWListUpdater _gfwListUpdater;
        //private AvailabilityStatistics availabilityStatistics = AvailabilityStatistics.Instance;


        public StatisticsStrategyConfiguration StatisticsConfiguration { get; private set; }

        private long _inboundCounter = 0;
        private long _outboundCounter = 0;
        public long InboundCounter => Interlocked.Read(ref _inboundCounter);
        public long OutboundCounter => Interlocked.Read(ref _outboundCounter);


        private Queue<TrafficPerSecond> _traffic;
        private bool _stopped = false;
        private bool _systemProxyIsDirty = false;

        //public Configuration GetCurrentConfiguration()
        //{
        //    return _config;
        //}


        public class PathEventArgs : EventArgs
        {
            public string Path;
        }

        private class TrafficPerSecond
        {
            public long InboundCounter;
            public long OutboundCouter;
            public long InboundIncreasement;
            public long OutboundIncreasement;
        }

        public event EventHandler ConfigChanged;
        public event EventHandler EnableStatusChanged;
        public event EventHandler EnableGlobalChanged;
        public event EventHandler ShareOverLANStatusChanged;
        public event EventHandler VerboseLoggingStatusChanged;
        public event EventHandler TrafficChanged;

        public event EventHandler<PathEventArgs> PACFileReadyToOpen;
        public event EventHandler<PathEventArgs> UserRuleFileReadyToOpen;

        public event EventHandler<GFWListUpdater.ResultEventArgs> UpdatePACFromGFWListCompleted;

        public event ErrorEventHandler UpdatePACFromGFWListError;
        public event ErrorEventHandler Errored;

        public ShadowsocksController()
        {
            
            StatisticsConfiguration = StatisticsStrategyConfiguration.Load();
            _strategyManager = new StrategyManager(this);
            StartReleaseMemory();
            StartTrafficStatistics(61);
        }

        public void Start()
        {
            Reload();
        }

        protected void Reload()
        {
            if (_privoxyRunner == null) _privoxyRunner = new PrivoxyRunner();
            if(_pacServer== null)
            {
                _pacServer = new PACServer();
                _pacServer.PACFileChanged += pacServer_PACFileChanged;
                _pacServer.UserRuleFileChanged += pacServer_UserRuleFileChanged;
            }
        }

        private void pacServer_UserRuleFileChanged(object sender, EventArgs e)
        {
            UpdateSystemProxy();
        }

        private void UpdateSystemProxy()
        {
            if (Config.Enabled)
            {
                //SystemProxy
            }
        }

        private void pacServer_PACFileChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #region Memory Management

        private void StartReleaseMemory()
        {
            _ramThread = new Thread(new ThreadStart(ReleaseMemory));
            _ramThread.IsBackground = true;
            _ramThread.Start();
        }

        private void ReleaseMemory()
        {
            while (true)
            {
                Util.Utils.ReleaseMemory(false);
                Thread.Sleep(30 * 1000);
            }
        }

        #endregion

        #region Traffic Statistics

        private void StartTrafficStatistics(int queueMaxSize)
        {
            _traffic = new Queue<TrafficPerSecond>();
            for(int i = 0; i < queueMaxSize; ++i)
            {
                _traffic.Enqueue(new TrafficPerSecond());
            }
            _trafficThread = new Thread(new ThreadStart(() => StartTrafficStatistics(queueMaxSize)));
            _trafficThread.IsBackground = true;
            _trafficThread.Start();
        }

        private void TrafficStatistics(int queueMaxSize)
        {
            while (true)
            {
                TrafficPerSecond previous = _traffic.Last();
                TrafficPerSecond current = new TrafficPerSecond();
                var inbound = current.InboundCounter = InboundCounter;
                var outbound = current.OutboundCouter = OutboundCounter;
                current.InboundIncreasement = inbound - previous.InboundCounter;
                current.OutboundIncreasement = outbound - previous.OutboundCouter;
                _traffic.Enqueue(current);
                if (_traffic.Count > queueMaxSize) _traffic.Dequeue();
                TrafficChanged?.Invoke(this, new EventArgs());
                Thread.Sleep(1000);
            }
        }

        #endregion

    }
}
