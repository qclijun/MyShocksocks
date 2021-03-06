﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyShadowsocks.Model;
using NLog;

namespace MyShadowsocks.Controller.Strategy
{


    class HighAvailabilityStrategy : IStrategy
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();
        class ServerStatus
        {
            public TimeSpan Latency;
            public DateTime LastTimeDetectLatency;
            public DateTime LastRead;
            public DateTime LastWrite;
            public DateTime LastFailure;
            public Server ThisServer;
            public double Score;
        }

        private ServerStatus _currentStatus;
        private Dictionary<Server, ServerStatus> _serverStatus;
        private ShadowsocksController _controller;
        private Random _random;

        public HighAvailabilityStrategy(ShadowsocksController controller)
        {
            _controller = controller;
            _random = new Random();
            _serverStatus = new Dictionary<Server, ServerStatus>();
        }

        public string ID
        {
            get
            {
                return "com.shadowsocks.strategy.ha";
            }
        }

        public string Name
        {
            get
            {
                return I18N.GetString("High Availability");
            }
        }

        public Server GetAServer(IStrategyCallerType type, IPEndPoint localIPEndPoint)
        {
            if (type == IStrategyCallerType.TCP)
            {
                ChooseNewServer();
            }
            if (_currentStatus == null) return null;
            return _currentStatus.ThisServer;
        }

        public void ReloadServers()
        {
            var newServerStatus = new Dictionary<Server, ServerStatus>(_serverStatus);
            foreach (var server in _controller.Config.GetServers())
            {
                ServerStatus status;
                if(newServerStatus.TryGetValue(server,out status))
                {
                    status.ThisServer = server;
                }
                else
                {
                    var status2 = new ServerStatus();
                    status2.ThisServer = server;
                    status2.LastFailure = DateTime.MinValue;
                    status2.LastRead = DateTime.Now;
                    status2.LastWrite = DateTime.Now;
                    status2.Latency = new TimeSpan(0, 0, 0, 0, 10);
                    status2.LastTimeDetectLatency = DateTime.Now;
                    newServerStatus.Add(server, status2);
                }
            }
            _serverStatus = newServerStatus;
            ChooseNewServer();
        }

        private void ChooseNewServer()
        {
            ServerStatus olsStatus = _currentStatus;
            List<ServerStatus> statusList = new List<ServerStatus>(_serverStatus.Values);
            DateTime now = DateTime.Now;
            foreach(var status in statusList)
            {
                status.Score =
                    100 * 1000 * Math.Min(5 * 60, (now - status.LastFailure).TotalSeconds)
                    - 2 * 5 * (Math.Min(2000, status.Latency.TotalMilliseconds) / (1 + (now - status.LastTimeDetectLatency).TotalSeconds / 30 / 10)
                    - 0.5 * 200 * Math.Min(5, (status.LastRead - status.LastWrite).TotalSeconds)
                    );
                logger.Debug($"server: {status.ThisServer.ToString()} latency: {status.Latency} score: {status.Score}");
            }
            ServerStatus max = null;
            foreach(var status in statusList)
            {
                if (max == null) max = status;
                else if (status.Score >= max.Score) max = status;
            }
            if (max != null)
            {
                if (_currentStatus == null || max.Score - _currentStatus.Score > 200)
                {
                    _currentStatus = max;
                    logger.Info($"HA switcing to server:  {_currentStatus.ThisServer.ToString()}");
                }
            }
        }

        public void SetFailure(Server server)
        {
            logger.Debug($"failure: {server.ToString()}");
            ServerStatus status;
            if (_serverStatus.TryGetValue(server, out status))
            {
                status.LastFailure = DateTime.Now;
            }
        }

        public void UpdateLastRead(Server server)
        {
            logger.Debug($"last read: {server.ToString()}");
            ServerStatus status;
            if(_serverStatus.TryGetValue(server,out status))
            {
                status.LastRead = DateTime.Now;
            }
        }

        public void UpdateLastWrite(Server server)
        {
            logger.Debug($"last write: {server.ToString()}");
            ServerStatus status;
            if (_serverStatus.TryGetValue(server, out status))
            {
                status.LastWrite = DateTime.Now;
            }
        }

        public void UpdateLatency(Server server, TimeSpan latency)
        {
            logger.Debug($"latency: {server.ToString()} {latency}");
            ServerStatus status;
            if(_serverStatus.TryGetValue(server,out status))
            {
                status.Latency = latency;
                status.LastTimeDetectLatency = DateTime.Now;
            }
        }
    }
}
