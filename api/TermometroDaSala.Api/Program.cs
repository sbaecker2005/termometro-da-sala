using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("escrita", http =>
        RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "sem-ip",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

var cs = builder.Configuration.GetConnectionString("Sala")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:Sala.");
builder.Services.AddDbContext<SalaDb>(o => o.UseSqlServer(cs));
builder.Services.AddSingleton(TimeProvider.System);

if (builder.Environment.IsEnvironment("Demo"))
    builder.Services.AddScoped<IPrevisorRua>(sp => new PrevisorRuaDemo(sp.GetRequiredService<IHostEnvironment>().ContentRootPath));
else
    builder.Services.AddScoped<IPrevisorRua>(sp =>
    {
        var raiz = sp.GetRequiredService<IHostEnvironment>().ContentRootPath;
        var caminho = Path.GetFullPath(builder.Configuration["Modelo:Caminho"] ?? Path.Combine("modelo", "modelo.json"), raiz);
        return new PrevisorRuaModelo(sp.GetRequiredService<SalaDb>(), sp.GetRequiredService<ILogger<PrevisorRuaModelo>>(), caminho);
    });

builder.Services.AddScoped<ServicoLeituras>();
builder.Services.AddScoped<ConsultaPrevisao>();

if (!builder.Configuration.GetValue("Mqtt:Desligado", false))
    builder.Services.AddHostedService<AssinanteMqtt>();

var app = builder.Build();

if (app.Configuration.GetValue("Banco:Migrar", true))
{
    using var escopo = app.Services.CreateScope();
    escopo.ServiceProvider.GetRequiredService<SalaDb>().Database.Migrate();
}

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
