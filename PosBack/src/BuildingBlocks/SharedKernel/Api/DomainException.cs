namespace SharedKernel.Api;

public abstract class DomainException : Exception
{
    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public sealed class NotFoundException(string code, string message) : DomainException(code, message);
public sealed class ValidationException(string code, string message, IReadOnlyDictionary<string, string[]>? errors = null) : DomainException(code, message)
{
    public IReadOnlyDictionary<string, string[]>? Errors { get; } = errors;
}
public sealed class ConflictException(string code, string message) : DomainException(code, message);
public sealed class ForbiddenException(string code, string message) : DomainException(code, message);
