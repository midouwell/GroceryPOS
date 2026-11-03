namespace SharedKernel.Api;

public sealed record ApiError(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors = null);
