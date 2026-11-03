namespace Customers.Api.Dtos;

public sealed record CustomerRequest(string Name, string Email, string Phone);
public sealed record CustomerResponse(string Id, string Name, string Email, string Phone, bool IsActive, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record PagedCustomersResponse(IReadOnlyList<CustomerResponse> Items, int Page, int PageSize, int TotalCount);
