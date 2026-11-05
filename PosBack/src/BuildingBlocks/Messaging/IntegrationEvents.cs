namespace Messaging;

public abstract record IntegrationEvent(string EventId, DateTimeOffset OccurredAt, string CorrelationId);

public sealed record SaleCreatedEvent(string EventId, DateTimeOffset OccurredAt, string CorrelationId,
    string SaleId, string IdempotencyKey, int GrandTotalCents) : IntegrationEvent(EventId, OccurredAt, CorrelationId);

public sealed record PaymentCompletedEvent(string EventId, DateTimeOffset OccurredAt, string CorrelationId,
    string SaleId, string PaymentId, int AmountCents) : IntegrationEvent(EventId, OccurredAt, CorrelationId);

public sealed record PaymentFailedEvent(string EventId, DateTimeOffset OccurredAt, string CorrelationId,
    string SaleId, string Reason) : IntegrationEvent(EventId, OccurredAt, CorrelationId);

public sealed record InventoryStockUpdatedEvent(string EventId, DateTimeOffset OccurredAt, string CorrelationId,
    string ProductId, int QuantityDelta, int QuantityOnHand) : IntegrationEvent(EventId, OccurredAt, CorrelationId);

public sealed record SaleCompletedEvent(string EventId, DateTimeOffset OccurredAt, string CorrelationId,
    string SaleId, string SaleNumber, int GrandTotalCents) : IntegrationEvent(EventId, OccurredAt, CorrelationId);

public sealed record SaleCancelledEvent(string EventId, DateTimeOffset OccurredAt, string CorrelationId,
    string SaleId, string Reason) : IntegrationEvent(EventId, OccurredAt, CorrelationId);
