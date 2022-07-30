using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace com.tweetapp.Kafka
{
    public class Producer : IProducer
    {

        private readonly ProducerConfig config;
        private readonly ILogger<Producer> logger;
        private readonly string topic = "db_request";
        public Producer(IConfiguration config, ILogger<Producer> logger)
        {
            this.config = new ProducerConfig
            {
                BootstrapServers = config.GetConnectionString("KafkaBootstrapServers"),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = config.GetConnectionString("KafkaKey"),
                SaslPassword = config.GetConnectionString("KafkaSecret")
            };
            this.logger = logger;
        }

        public async Task<bool> SendRequestToKafkaAsync(string key,string message)
        {
            
            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                try
                {
                    var result = await producer.ProduceAsync(key, new Message<string, string> {Key=key, Value = message });
                    return true;
                }
                catch (ProduceException<string, string> e)
                {
                    logger.LogError(e.Error.Reason, e);
                }
            }
            return false;
        }
    }
}
