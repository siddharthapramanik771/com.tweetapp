using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.tweetapp.Mongodb;

namespace com.tweetapp.Kafka
{
    public class Consumer : BackgroundService
    {
        private readonly ConsumerConfig config;
        private readonly ILogger<Consumer> logger;
        private readonly IDbRequest dbrequest;

        public Consumer(IConfiguration config, ILogger<Consumer> logger, IDbRequest dbRequest)
        {
            this.config = new ConsumerConfig
            {
                GroupId = "webapi-integration",
                BootstrapServers = config.GetConnectionString("KafkaBootstrapServers"),
                SaslMechanism = SaslMechanism.Plain,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = config.GetConnectionString("KafkaKey"),
                SaslPassword = config.GetConnectionString("KafkaSecret"),
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            this.logger = logger;
            this.dbrequest = dbRequest;

        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var builder = new ConsumerBuilder<string, string>(config).Build())
            {
                
                builder.Subscribe(Global.request_types);
                try
                {
                    await Task.Run(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var consumer = builder.Consume(cancellationToken);
                            if (!await dbrequest.process_request(consumer.Message.Key,consumer.Message.Value))
                                logger.LogError("Message was not inserted");
                            else
                                logger.LogInformation("Message Processed");

                        }
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                    builder.Close();
                }
            }
        }
    }
}
