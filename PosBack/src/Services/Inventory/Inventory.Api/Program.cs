using System.Text;
using Inventory.Api.Application;
using Inventory.Api.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Persistence;

namespace Inventory.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton<IJsonRepository<StockItem>>(serviceProvider =>
        {
            var options = builder.Configuration.GetSection("JsonPersistence:Stock").Get<JsonRepositoryOptions>() ?? new JsonRepositoryOptions { FileName = "stock.json", DataPath = "data/inventory" };
            return new JsonRepository<StockItem>(options, serviceProvider.GetRequiredService<ILogger<JsonRepository<StockItem>>>(), []);
        });
        builder.Services.AddSingleton<IJsonRepository<StockMovement>>(serviceProvider =>
        {
            var options = builder.Configuration.GetSection("JsonPersistence:Movements").Get<JsonRepositoryOptions>() ?? new JsonRepositoryOptions { FileName = "movements.json", DataPath = "data/inventory" };
            return new JsonRepository<StockMovement>(options, serviceProvider.GetRequiredService<ILogger<JsonRepository<StockMovement>>>(), []);
        });
        builder.Services.AddScoped<InventoryService>();

        var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey must be configured.");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
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
