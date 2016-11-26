using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MyShadowsocks.Model;

namespace MyShadowsocks.Controller
{
    using Newtonsoft.Json;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Threading;
    using Statistics = Dictionary<string, List<StatisticsRecord>>;
    using NLog;

    public sealed class AvailabilityStatistics : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string DateTimePattern = "yyyy-MM-dd HH:mm:ss";
        private const string StatisticsFilesName = "shadowsocks.availability.json";

        public static string AvailabilityStatisticsFile;

        static AvailabilityStatistics()
        {
            AvailabilityStatisticsFile = Util.Utils.GetTempPath(StatisticsFilesName);
        }

        private ShadowsocksController _controller;
        private StatisticsStrategyConfiguration Config => _controller.StatisticsConfiguration;

        private int Repeat => Config.RepeatTimesNum;
        public const int TimeoutMilliseconds = 500;

        private readonly ConcurrentDictionary<string, List<int>> _latencyRecords
            = new ConcurrentDictionary<string, List<int>>();
        private readonly ConcurrentDictionary<string, List<int>> _inboundSpeedRecords
            = new ConcurrentDictionary<string, List<int>>();
        private readonly ConcurrentDictionary<string, List<int>> _outboundSpeedRecords
            = new ConcurrentDictionary<string, List<int>>();
        private readonly ConcurrentDictionary<string, InOutBoundRecord> _inOutBoundRecords
            = new ConcurrentDictionary<string, InOutBoundRecord>();

        private class InOutBoundRecord
        {
            private long _inbound;
            private long _lastInbound;
            private long _outbound;
            private long _lastOutbound;

            public void UpdateInbound(long delta)
            {
                Interlocked.Add(ref _inbound, delta);
            }

            public void UpdateOutbound(long delta)
            {
                Interlocked.Add(ref _outbound, delta);
            }

            public void GetDelta(out long inboundDelta, out long outboundDelta)
            {
                var i = Interlocked.Read(ref _inbound);
                var il = Interlocked.Exchange(ref _lastInbound, i);
                inboundDelta = i - il;

                var o = Interlocked.Read(ref _outbound);
                var ol = Interlocked.Exchange(ref _lastOutbound, o);
                outboundDelta = o - ol;
            }
        }


        //tasks
        private readonly TimeSpan _delayBeforeStart = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(2);
        private Timer _recorder;

        private TimeSpan RecordingInterval => TimeSpan.FromMinutes(Config.DataCollectionMinute);
        private Timer _speedMonior;
        private readonly TimeSpan _monitorInterval = TimeSpan.FromSeconds(1);



        // Static Sington Initialization
        public static AvailabilityStatistics Instance { get; } = new AvailabilityStatistics();
        public Statistics RawStatistics { get; private set; }
        public Statistics FilteredStatistics { get; private set; }

        private AvailabilityStatistics()
        {
            RawStatistics = new Statistics();
        }

        void UpdateConfiguration(ShadowsocksController controller)
        {
            _controller = controller;
            Reset();
            try
            {
                if (Config.StatisticsEnabled)
                {
                    StartTimerWithoutState(ref _recorder, Run, RecordingInterval);
                    LoadRawStatistics();
                    StartTimerWithoutState(ref _speedMonior, UpdateSpeed, _monitorInterval);
                }
                else
                {
                    _recorder?.Dispose();
                    _speedMonior?.Dispose();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        private void StartTimerWithoutState(ref Timer timer, TimerCallback callback, TimeSpan interval)
        {
            if (timer?.Change(_delayBeforeStart, interval) == null)
            {
                timer = new Timer(callback, null, _delayBeforeStart, interval);
            }
        }

        private void UpdateSpeed(object _)
        {
            foreach (var kv in _inOutBoundRecords)
            {
                var id = kv.Key;
                var record = kv.Value;
                long inboundDelta, outboundDelta;
                record.GetDelta(out inboundDelta, out outboundDelta);
                var inboundSpeed = GetSpeedInKiBPerSecond(inboundDelta, _monitorInterval.TotalSeconds);
                var outboundSpeed = GetSpeedInKiBPerSecond(outboundDelta, _monitorInterval.TotalSeconds);

                var inR = _inboundSpeedRecords.GetOrAdd(id, (k) => new List<int>());
                var outR = _outboundSpeedRecords.GetOrAdd(id, (k) => new List<int>());
                inR.Add(inboundSpeed);
                outR.Add(outboundSpeed);

                logger.Debug($"{id}:  current/max inbound {inboundSpeed}/{inR.Max()} KiB/s, " +
                    $"current/max outbound {outboundSpeed}/{outR.Max()} KiB/s");
            }
        }

        private void Reset()
        {
            _inboundSpeedRecords.Clear();
            _outboundSpeedRecords.Clear();
            _latencyRecords.Clear();
        }

        private void Run(object _)
        {
            UpdateRecords();
            Reset();

        }

        class UpdateRecordsState
        {
            public int counter;
        }

        private void UpdateRecords()
        {
            var records = new Dictionary<string, StatisticsRecord>();
            UpdateRecordsState state = new UpdateRecordsState();
            state.counter = _controller.Config.GetServers().Count;
            foreach (var server in _controller.Config.GetServers())
            {
                var id = server.Identifier();
                List<int> inboundSpeedRecords = null;
                List<int> outboundSpeedRecords = null;
                List<int> latencyRecords = null;
                _inboundSpeedRecords.TryGetValue(id, out inboundSpeedRecords);
                _outboundSpeedRecords.TryGetValue(id, out outboundSpeedRecords);
                _latencyRecords.TryGetValue(id, out latencyRecords);
                StatisticsRecord record = new StatisticsRecord(id, inboundSpeedRecords, outboundSpeedRecords, latencyRecords);
                records[id] = record;
                if (Config.Ping)
                {
                    MyPing ping = new MyPing(server, Repeat);
                    ping.Completed += ping_Completed;
                    ping.Start(new PingState { state = state, record = record });
                }else if (!record.IsEmptyData())
                {
                    AppendRecord(id, record);
                }

            }
            if (!Config.Ping)
            {
                Save();
                FilterRawStatistics();
            }

        }


        private void ping_Completed(object sender, MyPing.CompletedEventArgs args)
        {
            PingState pingState = (PingState)args.UserState;
            UpdateRecordsState state = pingState.state;
            Server server = args.server;
            StatisticsRecord record = pingState.record;
            record.SetResponse(args.RoundtripTime);
            if (!record.IsEmptyData())
            {
                AppendRecord(server.Identifier(), record);

            }
            logger.Debug(string.Format("Ping {0} {1} times, {2}% packages loss, min {3} ms, max {4} ms, avg {5} ms",
                server.ToString(), 100 - record.PackageLoss * 100, record.MinResponse, record.MaxResponse, record.AverageResponse));  
           if(Interlocked.Decrement(ref state.counter) == 0)
            {
                Save();
                FilterRawStatistics();
            }     
                
        }

        private void AppendRecord(string serverIdentifier, StatisticsRecord record)
        {
            try
            {
                List<StatisticsRecord> records;
                lock (RawStatistics)
                {
                    if(!RawStatistics.TryGetValue(serverIdentifier,out records))
                    {
                        records = new List<StatisticsRecord>();
                        RawStatistics.Add(serverIdentifier, records);
                    }
                    
                }
                records.Add(record);
            }catch(Exception e)
            {
                logger.Error(e.Message);
            }
        }

        private void Save()
        {
            logger.Debug($"save statistics to {AvailabilityStatisticsFile}");
            if (RawStatistics.Count == 0) return;
            try
            {
                string content;
#if DEBUG
                content = JsonConvert.SerializeObject(RawStatistics, Formatting.Indented);
#else
                content = JsonConvert.SerializeObject(RawStatistics, Formatting.None);
#endif
                File.WriteAllText(AvailabilityStatisticsFile, content);
            }catch(IOException e)
            {
                logger.Error(e.Message);
            }
        }


        private bool IsValidRecord(StatisticsRecord record)
        {
            if (Config.ByHourOfDay)
            {
                if (!record.timestamp.Hour.Equals(DateTime.Now.Hour)) return false;
            }
            return true;
        }

        private void FilterRawStatistics()
        {
            try
            {
                logger.Debug("filter raw statistics");
                if (RawStatistics == null) return;
                if (FilteredStatistics == null)
                    FilteredStatistics = new Statistics();
                foreach(var serverAndRecords in RawStatistics)
                {
                    var server = serverAndRecords.Key;
                    var filteredRecords = serverAndRecords.Value.FindAll(IsValidRecord);
                    FilteredStatistics[server] = filteredRecords;
                }
            }catch(Exception e)
            {
                logger.Error(e.Message);
            }
        }

        private void LoadRawStatistics()
        {
            try
            {
                var path = AvailabilityStatisticsFile;
                logger.Debug($"loading statistics from {path}");
                if (!File.Exists(path))
                {
                    using (File.Create(path))
                    {
                        //do nothing
                    }
                }
                else
                {
                    var content = File.ReadAllText(path);
                    RawStatistics = JsonConvert.DeserializeObject<Statistics>(content) ?? RawStatistics;

                }
            }catch(Exception e)
            {
                logger.Error(e.Message);
                Console.WriteLine($"failed to load statistics; try to reload {_retryInterval.TotalMilliseconds} minutes later");
                _recorder.Change(_retryInterval, RecordingInterval);
            }
        }

        private static  int GetSpeedInKiBPerSecond(long bytes, double seconds)
        {
            var result = (int)(bytes / seconds) / 1024;
            return result;
        }

        public void UpdateLatency(Server server, int latency)
        {
            _latencyRecords.GetOrAdd(server.Identifier(), k =>
            {
                List<int> records = new List<int>();
                records.Add(latency);
                return records;
            });
        }

        public void UpdateInboundCounter(Server server, long n)
        {
            _inOutBoundRecords.AddOrUpdate(server.Identifier(), k =>
            {
                var r = new InOutBoundRecord();
                r.UpdateInbound(n);
                return r;
            }, (k, v) =>
            {
                v.UpdateInbound(n);
                return v;
            });
        }

        public void UpdateOutboundCounter(Server server, long n)
        {
            _inOutBoundRecords.AddOrUpdate(server.Identifier(), k =>
            {
                var r = new InOutBoundRecord();
                r.UpdateOutbound(n);
                return r;
            }, (k, v) =>
            {
                v.UpdateOutbound(n);
                return v;
            });
        }

        class PingState
        {
            public UpdateRecordsState state;
            public StatisticsRecord record;
        }


        class MyPing
        {
            public const int TimeoutMilliseconds = 500;
            public EventHandler<CompletedEventArgs> Completed;
            private Server server;
            private int repeat;
            private IPAddress ip;
            private Ping ping;
            private List<int?> RoundtripTime;

            public MyPing(Server server, int repeat)
            {
                this.server = server;
                this.repeat = repeat;
                RoundtripTime = new List<int?>(repeat);
                ping = new Ping();
                ping.PingCompleted += Ping_PingCompleted;
            }

            public void Start(object userState)
            {
                if (server.HostName == "")
                {
                    FireCompleted(new Exception("Invalid Server"), userState);
                    return;
                }
                new Task(() => ICMPTest(0, userState)).Start();
            }

            private void ICMPTest(int delay, object userState)
            {
                try
                {
                    logger.Debug($"Ping {server.ToString()}");
                    if (ip == null)
                    {
                        ip = Dns.GetHostAddresses(server.HostName)
                            .First(ip =>
                            ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ||
                            ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
                    }
                    repeat--;
                    if (delay > 0)
                        Thread.Sleep(delay);
                    ping.SendAsync(ip, TimeoutMilliseconds, userState);
                }catch(Exception e)
                {
                    logger.Error($"An Exception occured while evaluating {server.ToString()}");
                    logger.Error(e.Message);
                    FireCompleted(e, userState);
                }
            }

            private void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
            {
                try
                {
                    if (e.Reply.Status == IPStatus.Success)
                    {
                        logger.Debug($"Ping {server.ToString()} {e.Reply.RoundtripTime} ms");
                        RoundtripTime.Add((int)e.Reply.RoundtripTime);
                    }
                    else
                    {
                        logger.Debug($"Ping {server.ToString()} timeout");
                        RoundtripTime.Add(null);
                    }
                    TestNext(e.UserState);
                }catch(Exception ex)
                {
                    logger.Error($"An exception occured while evaluating {server.ToString()}");
                    logger.Error(ex.Message);
                    FireCompleted(ex, e.UserState);
                }
            }

            private void TestNext(object userState)
            {
                if (repeat > 0)
                {
                    int delay = TimeoutMilliseconds + new Random().Next() % TimeoutMilliseconds;
                    new Task(() => ICMPTest(delay, userState)).Start();
                }
                else
                {
                    FireCompleted(null, userState);
                }
            }

            private void FireCompleted(Exception error, object userState)
            {
                Completed?.Invoke(this, new CompletedEventArgs
                {
                    Error = error,
                    server = server,
                    RoundtripTime = RoundtripTime,
                    UserState = userState
                });
            }

            public class CompletedEventArgs : EventArgs
            {
                public Exception Error;
                public Server server;
                public List<int?> RoundtripTime;
                public object UserState;
            }
        }

        public void Dispose()
        {
            _recorder.Dispose();
            _speedMonior.Dispose();
        }
    }
}
