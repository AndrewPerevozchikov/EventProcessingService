using System.Text.Json;
using Confluent.Kafka;
using EventProcessingService.Models;

namespace EventProcessingService.Services;

public class KafkaConsumer : IHostedService
    {
        private readonly IEventObservable _eventObservable;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaConsumer> _logger;
        private IConsumer<Ignore, string>? _consumer;
        private Task? _consumingTask;

        public KafkaConsumer(IEventObservable eventObservable, IConfiguration configuration, ILogger<KafkaConsumer> logger)
        {
            _eventObservable = eventObservable;
            _configuration = configuration;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken ct)
        {
            var bootstrapServers = _configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092";
            var topic = _configuration["KAFKA_TOPIC"] ?? "user-events";
            var groupId = _configuration["KAFKA_GROUP_ID"] ?? "event-processing-group";

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                EnableAutoOffsetStore = false
            };

            try
            {
                _consumer = new ConsumerBuilder<Ignore, string>(config)
                    .SetErrorHandler((_, error) => 
                        _logger.LogError("Ошибка Kafka: {Error}", error.Reason))
                    .Build();

                _consumer.Subscribe(topic);

                _consumingTask = Task.Factory.StartNew(
                    () => StartConsuming(ct), 
                    ct, 
                    TaskCreationOptions.LongRunning, 
                    TaskScheduler.Default);

                _logger.LogInformation("Kafka Consumer запущен. Topic: {Topic}, Group: {GroupId}", topic, groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при запуске Kafka Consumer");
                throw;
            }

            return Task.CompletedTask;
        }

        private void StartConsuming(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer?.Consume(ct);
                    
                    if (consumeResult?.Message?.Value != null)
                    {
                        ProcessMessage(consumeResult.Message.Value);
                        _consumer?.StoreOffset(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Ошибка при получении сообщения из Kafka");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении сообщений");
                }
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var userEvent = JsonSerializer.Deserialize<UserEvent>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (userEvent != null)
                {
                    _eventObservable.PublishEvent(userEvent);
                }
                else
                {
                    _logger.LogWarning("Не удалось десериализовать сообщение: {Message}", message);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка десериализации JSON сообщения: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки сообщения");
            }
        }
        

        public async Task StopAsync(CancellationToken ct)
        {
            if (_consumingTask != null)
            {
                await _consumingTask;
            }

            _consumer?.Close();
            _consumer?.Dispose();
        }
    }