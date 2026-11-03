namespace Identity.Api.Dtos;

public sealed record LoginRequest(string UserName, string Password);
public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAt, UserResponse User);
public sealed record CreateUserRequest(string UserName, string DisplayName, string Password, string Role);
public sealed record UserResponse(string Id, string UserName, string DisplayName, string Role, bool IsActive, DateTimeOffset CreatedAt);
