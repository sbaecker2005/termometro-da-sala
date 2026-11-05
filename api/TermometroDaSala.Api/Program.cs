using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("Sala")
    ?? "Server=localhost;Database=TermometroDaSala;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<SalaDb>(o => o.UseSqlServer(cs));
builder.Services.AddSingleton(TimeProvider.System);

if (builder.Environment.IsEnvironment("Demo"))
    builder.Services.AddScoped<IPrevisorRua>(sp => new PrevisorRuaDemo(sp.GetRequiredService<IHostEnvironment>().ContentRootPath));
else
    builder.Services.AddScoped<IPrevisorRua, PrevisorRuaBaseline>();

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
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
