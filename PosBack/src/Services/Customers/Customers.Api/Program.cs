using System.Text;
using Customers.Api.Application;
using Customers.Api.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Persistence;

using Customers.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Customers.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton<IJsonRepository<Customer>>(serviceProvider =>
        {
            var options = builder.Configuration.GetSection("JsonPersistence").Get<JsonRepositoryOptions>() ?? new JsonRepositoryOptions();
            return new JsonRepository<Customer>(options, serviceProvider.GetRequiredService<ILogger<JsonRepository<Customer>>>(), []);
        });
        builder.Services.AddScoped<CustomerService>();
        builder.Services.AddDbContext<CustomersDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("CustomersDatabase")));

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
