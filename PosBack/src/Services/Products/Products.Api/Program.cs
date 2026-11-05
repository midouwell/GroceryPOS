using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Products.Api.Application;
using Products.Api.Domain;
using SharedKernel.Persistence;

using Microsoft.EntityFrameworkCore;
using Products.Api.Infrastructure.Persistence;

namespace Products.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();
        var seedProducts = Array.Empty<Product>();
        builder.Services.AddSingleton<IJsonRepository<Product>>(serviceProvider =>
        {
            var options = builder.Configuration.GetSection("JsonPersistence").Get<JsonRepositoryOptions>() ?? new JsonRepositoryOptions();
            return new JsonRepository<Product>(options, serviceProvider.GetRequiredService<ILogger<JsonRepository<Product>>>(), seedProducts);
        });
        builder.Services.AddScoped<ProductService>();
        builder.Services.AddDbContext<ProductsDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsDatabase")));

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
