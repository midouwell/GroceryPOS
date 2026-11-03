using System.Text;
using Identity.Api.Application;
using Identity.Api.Domain;
using Identity.Api.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Persistence;

namespace Identity.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();

        var hasher = new Pbkdf2PasswordHasher();
        var seedUsers = new[]
        {
            new User { Id = "user-admin", UserName = "admin", DisplayName = "Development Administrator", PasswordHash = hasher.Hash("Admin123!"), Role = UserRoles.Administrator, IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new User { Id = "user-manager", UserName = "manager", DisplayName = "Development Manager", PasswordHash = hasher.Hash("Manager123!"), Role = UserRoles.Manager, IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new User { Id = "user-cashier", UserName = "cashier", DisplayName = "Development Cashier", PasswordHash = hasher.Hash("Cashier123!"), Role = UserRoles.Cashier, IsActive = true, CreatedAt = DateTimeOffset.UtcNow }
        };
        builder.Services.AddSingleton<IJsonRepository<User>>(serviceProvider =>
        {
            var data = builder.Configuration.GetSection("JsonPersistence").Get<JsonRepositoryOptions>()
                ?? new JsonRepositoryOptions();
            return new JsonRepository<User>(data, serviceProvider.GetRequiredService<ILogger<JsonRepository<User>>>(), seedUsers);
        });
        builder.Services.AddSingleton<IPasswordHasher>(hasher);
        builder.Services.AddScoped<IdentityService>();

        var jwtKey = builder.Configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey must be configured.");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        builder.Services.AddAuthorization();

        var app = builder.Build();
        if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.Run();
    }
}
