using Customers.Api.Domain;
using Customers.Api.Dtos;
using SharedKernel.Api;
using SharedKernel.Persistence;

namespace Customers.Api.Application;

public sealed class CustomerService
{
    private readonly IJsonRepository<Customer> _repository;

    public CustomerService(IJsonRepository<Customer> repository) => _repository = repository;

    public async Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var emailExists = (await _repository.GetAllAsync(cancellationToken))
            .Any(customer => customer.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (emailExists)
            throw new ConflictException("email_exists", "A customer with this email already exists.");

        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _repository.AddAsync(customer, candidate => candidate.Id, cancellationToken);
        return ToResponse(customer);
    }

    public async Task<PagedCustomersResponse> GetAsync(string? search, bool? active, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var customers = await _repository.GetAllAsync(cancellationToken);
        var filtered = customers.Where(customer =>
            (string.IsNullOrWhiteSpace(search) ||
             customer.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
             customer.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
             customer.Phone.Contains(search)) &&
            (!active.HasValue || customer.IsActive == active.Value)).ToList();
        var items = filtered.OrderBy(customer => customer.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).Select(ToResponse).ToList();
        return new PagedCustomersResponse(items, page, pageSize, filtered.Count);
    }

    public async Task<CustomerResponse> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(id, candidate => candidate.Id, cancellationToken)
            ?? throw new NotFoundException("customer_not_found", "Customer was not found.");
        return ToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(string id, CustomerRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var existing = await _repository.GetByIdAsync(id, candidate => candidate.Id, cancellationToken)
            ?? throw new NotFoundException("customer_not_found", "Customer was not found.");
        var emailExists = (await _repository.GetAllAsync(cancellationToken)).Any(customer =>
            customer.Id != id && customer.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (emailExists)
            throw new ConflictException("email_exists", "A customer with this email already exists.");

        existing.Name = request.Name.Trim();
        existing.Email = request.Email.Trim();
        existing.Phone = request.Phone.Trim();
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(id, existing, candidate => candidate.Id, cancellationToken);
        return ToResponse(existing);
    }

    public async Task<CustomerResponse> SetActiveAsync(string id, bool active, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(id, candidate => candidate.Id, cancellationToken)
            ?? throw new NotFoundException("customer_not_found", "Customer was not found.");
        customer.IsActive = active;
        customer.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(id, customer, candidate => candidate.Id, cancellationToken);
        return ToResponse(customer);
    }

    private static void Validate(CustomerRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name)) errors[nameof(request.Name)] = ["Name is required."];
        if (string.IsNullOrWhiteSpace(request.Email)) errors[nameof(request.Email)] = ["Email is required."];
        if (errors.Count > 0) throw new ValidationException("invalid_customer", "The customer request is invalid.", errors);
    }

    private static CustomerResponse ToResponse(Customer customer) =>
        new(customer.Id, customer.Name, customer.Email, customer.Phone, customer.IsActive, customer.CreatedAt, customer.UpdatedAt);
}
