using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TermometroDaSala.Api.Contratos;

namespace TermometroDaSala.Api.Filtros;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ExigeChaveCoordenacaoAttribute : Attribute, IAsyncAuthorizationFilter
{
    public const string Cabecalho = "X-Chave-Coordenacao";

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var esperada = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ChaveCoordenacao"] ?? "";
        var enviada = context.HttpContext.Request.Headers[Cabecalho].ToString();
        var a = Encoding.UTF8.GetBytes(esperada);
        var b = Encoding.UTF8.GetBytes(enviada);
        if (a.Length == 0 || a.Length != b.Length || !CryptographicOperations.FixedTimeEquals(a, b))
            context.Result = new UnauthorizedObjectResult(new MotivoSaida("chave-invalida"));
        return Task.CompletedTask;
    }
}
