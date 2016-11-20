using Newtonsoft.Json;
using MyShadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace MyShadowsocks.Model
{
    [Serializable]
    public class StatisticsStrategyConfiguration
    {
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const string ID = "com.shadowsocks.strategy.statistics";
        public bool StatisticsEnabled { get; set; } = false;
        public bool ByHourOfDay { get; set; } = true;
        public bool Ping { get; set; }

        public int ChoiceKeptMinutes { get; set; } = 10;
        public int DataCollectionMinute { get; set; } = 10;
        public int RepeatTimesNum { get; set; } = 4;

        private const string ConfigFile = "statistics-config.json";


        public Dictionary<string, float> calculations;

        public StatisticsStrategyConfiguration()
        {
            var properties = typeof(StatisticsRecord).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            calculations = properties.ToDictionary(p => p.Name, _ => (float)0);
        }

        public static StatisticsStrategyConfiguration Load()
        {
            try
            {
                var content = File.ReadAllText(ConfigFile);
                var configuration = JsonConvert.DeserializeObject<StatisticsStrategyConfiguration>(content);
                return configuration;
            }catch(FileNotFoundException)
            {
                var configuration = new StatisticsStrategyConfiguration();
                Save(configuration);
                return configuration;
            }catch(Exception e)
            {
                logger.Error(e.Message);
                return new StatisticsStrategyConfiguration();
            }
        }

        private static void Save(StatisticsStrategyConfiguration configuration)
        {
            try
            {
                var content = JsonConvert.SerializeObject(configuration, Formatting.Indented);
                File.WriteAllText(ConfigFile, content);
            }catch(Exception e)
            {
                logger.Error(e.Message);
            }
        }

        

    }
}
