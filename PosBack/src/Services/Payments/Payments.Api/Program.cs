using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Payments.Api.Application;
using Payments.Api.Domain;
using SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;
using Payments.Api.Infrastructure.Persistence;

namespace Payments.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton<IJsonRepository<Payment>>(serviceProvider =>
        {
            var options = builder.Configuration.GetSection("JsonPersistence").Get<JsonRepositoryOptions>() ?? new JsonRepositoryOptions();
            return new JsonRepository<Payment>(options, serviceProvider.GetRequiredService<ILogger<JsonRepository<Payment>>>(), []);
        });
        builder.Services.AddSingleton<CashPaymentGateway>();
        builder.Services.AddSingleton<MockCardPaymentGateway>();
        builder.Services.AddScoped<PaymentService>();
        builder.Services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentsDatabase")));
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
