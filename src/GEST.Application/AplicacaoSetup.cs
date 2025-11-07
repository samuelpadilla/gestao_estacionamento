using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEST.Application;

public static class AplicacaoSetup
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services, IConfiguration config)
    {


        return services;
    }
}
