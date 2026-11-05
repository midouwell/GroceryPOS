using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Messaging;

public interface IEventBus
{
    void Publish<TEvent>(TEvent eventData, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent;
}

public sealed class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly ILogger<RabbitMqEventBus> _logger;

    public RabbitMqEventBus(IConfiguration configuration, ILogger<RabbitMqEventBus> logger)
    {
        _logger = logger;
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:Host"] ?? "localhost",
                Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672,
                UserName = configuration["RabbitMq:Username"] ?? "guest",
                Password = configuration["RabbitMq:Password"] ?? "guest",
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("grocerypos.events", ExchangeType.Topic, durable: true);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "RabbitMQ is unavailable; integration events will not be published.");
        }
    }

    public void Publish<TEvent>(TEvent eventData, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
    {
        if (_channel is null)
            return;
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = eventData.EventId;
        properties.CorrelationId = eventData.CorrelationId;
        properties.ContentType = "application/json";
        var routingKey = typeof(TEvent).Name.Replace("Event", string.Empty);
        var json = JsonSerializer.Serialize(eventData, typeof(TEvent));
        _channel.BasicPublish("grocerypos.events", routingKey, properties, Encoding.UTF8.GetBytes(json));
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
