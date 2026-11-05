using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Reports.Api.Application;

namespace Reports.Api;

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
        builder.Services.AddHttpClient("Sales", client => client.BaseAddress = new Uri(builder.Configuration["Services:Sales"] ?? "http://localhost:5104/"))
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));
        builder.Services.AddHttpClient("Inventory", client => client.BaseAddress = new Uri(builder.Configuration["Services:Inventory"] ?? "http://localhost:5103/"))
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));
        builder.Services.AddScoped<ReportingService>(provider => new ReportingService(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient("Sales"),
            provider.GetRequiredService<IHttpClientFactory>().CreateClient("Inventory"),
            provider.GetRequiredService<IHttpContextAccessor>()));

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
