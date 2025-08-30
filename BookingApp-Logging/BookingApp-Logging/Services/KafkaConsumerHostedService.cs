using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System.Text.Json;
using BookingApp_Logging.Models;

namespace BookingApp_Logging.Services
{
    public class KafkaConsumerHostedService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAdminClient _adminClient;
        private const string TopicName = "error-logs-topic";

        public KafkaConsumerHostedService(IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            var admingConfig = new AdminClientConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"]
            };
            _adminClient = new AdminClientBuilder(admingConfig).Build();

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "logging-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CreateTopicIfNotExists(TopicName, 1);
            _consumer.Subscribe(TopicName);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    var errorLogDto = JsonSerializer.Deserialize<ErrorLogDto>(consumeResult.Message.Value);
                    using(var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<Data.LoggingContext>();
                        var errorLog = new ErrorLog
                        {
                            ErrorMessage = errorLogDto.ErrorMessage,
                            StackTrace = errorLogDto.StackTrace,
                            Timestamp = errorLogDto.Timestamp,
                            SourceService = errorLogDto.SourceService,
                            ExceptionType = errorLogDto.ExceptionType
                        };

                        dbContext.ErrorLogs.Add(errorLog);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch(ConsumeException e)
                {
                    Console.WriteLine($"Consume error: {e.Error.Reason}");
                }
            }
        }

        private async Task CreateTopicIfNotExists(string topicName, int numPartitions)
        {
            try
            {
                await _adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                    new TopicSpecification { Name = topicName, NumPartitions = numPartitions, ReplicationFactor = 1 }
                });
            }
            catch (CreateTopicsException e)
            {
                if (e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    Console.WriteLine($"Topic '{topicName}' already exists. Skipping creation.");
                }
                else
                {
                    Console.WriteLine($"Error creating topic");
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _adminClient.Dispose();
            base.Dispose();
        }
    }
}
