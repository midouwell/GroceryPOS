using Products.Api.Domain;
using Products.Api.Dtos;
using SharedKernel.Api;
using SharedKernel.Persistence;

namespace Products.Api.Application;

public sealed class ProductService
{
    private readonly IJsonRepository<Product> _repository;

    public ProductService(IJsonRepository<Product> repository) => _repository = repository;

    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        await EnsureUniqueAsync(request.Sku, request.Barcode, null, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid().ToString("N"),
            Sku = request.Sku.Trim(),
            Barcode = request.Barcode.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            Unit = request.Unit.Trim(),
            Price = request.Price,
            TaxRate = request.TaxRate,
            IsActive = request.IsActive,
            CreatedAt = now
        };
        await _repository.AddAsync(product, candidate => candidate.Id, cancellationToken);
        return ToResponse(product);
    }

    public async Task<PagedResponse<ProductResponse>> GetAsync(
        string? search, string? category, bool? active, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = await _repository.GetAllAsync(cancellationToken);
        var filtered = query.Where(product =>
            (string.IsNullOrWhiteSpace(search) ||
             Contains(product.Name, search) || Contains(product.Sku, search) || Contains(product.Barcode, search)) &&
            (string.IsNullOrWhiteSpace(category) || product.Category.Equals(category, StringComparison.OrdinalIgnoreCase)) &&
            (!active.HasValue || product.IsActive == active.Value));

        var items = filtered.OrderBy(product => product.Name).ToList();
        var pageItems = items.Skip((page - 1) * pageSize).Take(pageSize).Select(ToResponse).ToList();
        return new PagedResponse<ProductResponse>(pageItems, page, pageSize, items.Count);
    }

    public async Task<ProductResponse> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(id, candidate => candidate.Id, cancellationToken)
            ?? throw new NotFoundException("product_not_found", "Product was not found.");
        return ToResponse(product);
    }

    public async Task<ProductResponse> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        var product = products.FirstOrDefault(candidate => candidate.Barcode.Equals(barcode.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException("product_not_found", "Product was not found.");
        return ToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(string id, ProductRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        await EnsureUniqueAsync(request.Sku, request.Barcode, id, cancellationToken);
        var existing = await _repository.GetByIdAsync(id, candidate => candidate.Id, cancellationToken)
            ?? throw new NotFoundException("product_not_found", "Product was not found.");
        var updated = new Product
        {
            Id = existing.Id,
            Sku = existing.Sku,
            Barcode = existing.Barcode,
            Name = existing.Name,
            Description = existing.Description,
            Category = existing.Category,
            Unit = existing.Unit,
            Price = existing.Price,
            TaxRate = existing.TaxRate,
            IsActive = existing.IsActive,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = existing.UpdatedAt
        };
        updated.Sku = request.Sku.Trim();
        updated.Barcode = request.Barcode.Trim();
        updated.Name = request.Name.Trim();
        updated.Description = request.Description.Trim();
        updated.Category = request.Category.Trim();
        updated.Unit = request.Unit.Trim();
        updated.Price = request.Price;
        updated.TaxRate = request.TaxRate;
        updated.IsActive = request.IsActive;
        updated.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(id, updated, candidate => candidate.Id, cancellationToken);
        return ToResponse(updated);
    }

    public async Task<ProductResponse> SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(id, candidate => candidate.Id, cancellationToken)
            ?? throw new NotFoundException("product_not_found", "Product was not found.");
        existing.IsActive = isActive;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(id, existing, candidate => candidate.Id, cancellationToken);
        return ToResponse(existing);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(product => product.Category).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToList();
    }

    private async Task EnsureUniqueAsync(string sku, string barcode, string? currentId, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        if (products.Any(product => product.Id != currentId && product.Sku.Equals(sku.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException("duplicate_sku", "A product with this SKU already exists.");
        if (products.Any(product => product.Id != currentId && product.Barcode.Equals(barcode.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException("duplicate_barcode", "A product with this barcode already exists.");
    }

    private static void Validate(ProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Sku)) errors[nameof(request.Sku)] = ["SKU is required."];
        if (string.IsNullOrWhiteSpace(request.Barcode)) errors[nameof(request.Barcode)] = ["Barcode is required."];
        if (string.IsNullOrWhiteSpace(request.Name)) errors[nameof(request.Name)] = ["Name is required."];
        if (string.IsNullOrWhiteSpace(request.Category)) errors[nameof(request.Category)] = ["Category is required."];
        if (string.IsNullOrWhiteSpace(request.Unit)) errors[nameof(request.Unit)] = ["Unit is required."];
        if (request.Price < 0) errors[nameof(request.Price)] = ["Price cannot be negative."];
        if (request.TaxRate < 0 || request.TaxRate > 1) errors[nameof(request.TaxRate)] = ["Tax rate must be between 0 and 1."];
        if (errors.Count > 0) throw new ValidationException("invalid_product", "The product request is invalid.", errors);
    }

    private static bool Contains(string value, string search) =>
        value.Contains(search, StringComparison.OrdinalIgnoreCase);

    public static ProductResponse ToResponse(Product product) =>
        new(product.Id, product.Sku, product.Barcode, product.Name, product.Description, product.Category,
            product.Unit, product.Price, product.TaxRate, product.IsActive, product.CreatedAt, product.UpdatedAt);
}
