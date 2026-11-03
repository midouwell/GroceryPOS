using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sales.Api.Application;
using Sales.Api.Domain;
using SharedKernel.Persistence;

namespace Sales.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<IJsonRepository<Sale>>(serviceProvider =>
        {
            var options = builder.Configuration.GetSection("JsonPersistence").Get<JsonRepositoryOptions>() ?? new JsonRepositoryOptions();
            return new JsonRepository<Sale>(options, serviceProvider.GetRequiredService<ILogger<JsonRepository<Sale>>>(), []);
        });
        builder.Services.AddHttpClient<ProductClient>(client =>
            client.BaseAddress = new Uri(builder.Configuration["Services:Products"] ?? "http://localhost:5102/"));
        builder.Services.AddHttpClient<InventoryClient>(client =>
            client.BaseAddress = new Uri(builder.Configuration["Services:Inventory"] ?? "http://localhost:5103/"));
        builder.Services.AddHttpClient<PaymentClient>(client =>
            client.BaseAddress = new Uri(builder.Configuration["Services:Payments"] ?? "http://localhost:5105/"));
        builder.Services.AddScoped<CheckoutService>();

        var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey must be configured.");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = true, ClockSkew = TimeSpan.Zero
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
