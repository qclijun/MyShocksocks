using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShadowsocks.Model
{
    public class StatisticsRecord
    {
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string ServerIdentifier { get; set; }

        public int? AverageLatency;
        public int? MinLatency;
        public int? MaxLatency;

        private bool EmptyLatencyData
            => (AverageLatency == null) && (MinLatency == null) && (MaxLatency == null);

        public int? AverageInboundSpeed;
        public int? MinInboundSpeed;
        public int? MaxInboundSpeed;

        private bool EmptyInboundSpeedData
            => (AverageInboundSpeed == null) & (MinInboundSpeed == null) && (MaxInboundSpeed == null);

        public int? AverageOutboundSpeed;
        public int? MinOutboundSpeed;
        public int? MaxOutboundSpeed;

        private bool EmptyOutboundSpeedData
            => (AverageOutboundSpeed == null) & (MinOutboundSpeed == null) && (MaxOutboundSpeed == null);


        public int? AverageResponse;
        public int? MinResponse;
        public int? MaxResponse;
        public float? PackageLoss;

        private bool EmptyResponseData
            => (AverageResponse == null) && (MinResponse == null) && (MaxResponse == null) && (PackageLoss == null);

        public bool IsEmptyData()
        {
            return EmptyInboundSpeedData && EmptyOutboundSpeedData && EmptyResponseData && EmptyLatencyData;
        }

        public StatisticsRecord()
        {

        }

        private static void StateList(List<int> records, out int? min, out int? max, out int? average)
        {
            if (records == null || records.Count == 0)
            {
                min = null; max = null; average = null;
                return;
            }
            int sum = records[0];
            min = records[0];
            max = records[0];
            for (int i = 1; i < records.Count; ++i)
            {
                sum += records[i];
                if (records[i] < min) min = records[i];
                else if (records[i] > max) max = records[i];
            }
            average = sum / records.Count;
        }

        public StatisticsRecord(string identifier, ICollection<int> inboundSpeedRecords,
            ICollection<int> outboundSpeedRecords, ICollection<int> latencyRecords)
        {
            ServerIdentifier = identifier;
            var inbound = inboundSpeedRecords?.Where(s => s > 0).ToList();

            StateList(inbound, out MinInboundSpeed, out MaxInboundSpeed, out AverageInboundSpeed);

            var outbound = outboundSpeedRecords?.Where(s => s > 0).ToList();

            StateList(outbound, out MinOutboundSpeed, out MaxOutboundSpeed, out AverageOutboundSpeed);

            var latency = latencyRecords?.Where(s => s > 0).ToList();

            StateList(latency, out MinLatency, out MaxLatency, out AverageLatency);

        }

        public StatisticsRecord(string identifier, ICollection<int?> responseRecords)
        {
            ServerIdentifier = identifier;
            SetResponse(responseRecords);
        }

        public void SetResponse(ICollection<int?> responseRecords)
        {
            if (responseRecords == null) return;
            var records = responseRecords.Where(response => response != null)
                .Select(response => response.Value).ToList();
            if (!records.Any()) return;
            AverageResponse = (int?)records.Average();
            MinResponse = records.Min();
            MaxResponse = records.Max();
            PackageLoss = responseRecords.Count(response => response != null) / (float)responseRecords.Count;
        }

    }
}
