using Identity.Api.Domain;
using Identity.Api.Dtos;
using Identity.Api.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Api;
using SharedKernel.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Api.Application;

public sealed class IdentityService
{
    private readonly IJsonRepository<User> _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public IdentityService(IJsonRepository<User> repository, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(candidate =>
            string.Equals(candidate.UserName, request.UserName, StringComparison.OrdinalIgnoreCase));

        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new ValidationException("invalid_credentials", "User name or password is invalid.");

        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var jwtKey = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, ToResponse(user));
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.DisplayName) ||
            string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("invalid_user", "User name, display name and password are required.");
        if (request.Password.Length < 8)
            throw new ValidationException("invalid_password", "Password must contain at least 8 characters.");
        if (!UserRoles.All.Contains(request.Role))
            throw new ValidationException("invalid_role", $"Role must be Administrator, Manager or Cashier.");

        var users = await _repository.GetAllAsync(cancellationToken);
        if (users.Any(user => user.UserName.Equals(request.UserName.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException("username_exists", "A user with this user name already exists.");

        var user = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            UserName = request.UserName.Trim(),
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRoles.All.Single(role => role.Equals(request.Role, StringComparison.OrdinalIgnoreCase)),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _repository.AddAsync(user, candidate => candidate.Id, cancellationToken);
        return ToResponse(user);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAsync(CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllAsync(cancellationToken);
        return users.Select(ToResponse).OrderBy(user => user.UserName).ToList();
    }

    public static UserResponse ToResponse(User user) =>
        new(user.Id, user.UserName, user.DisplayName, user.Role, user.IsActive, user.CreatedAt);
}
