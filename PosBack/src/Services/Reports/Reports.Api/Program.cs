namespace Reports.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();
        builder.Services.AddAuthorization();
        var app = builder.Build();
        if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapHealthChecks("/health");
        app.MapGet("/api/service-info", () => Results.Ok(new { Service = "Reports.Api", Status = "Running" }));
        app.Run();
    }
}
