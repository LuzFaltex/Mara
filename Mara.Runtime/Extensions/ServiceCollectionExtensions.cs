using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mara.Runtime.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMara(this IServiceCollection services)
        {
            services.AddHttpClient();

            // services.AddCommandHelp();

            services.AddMemoryCache();

            services.AddHostedService<MaraBot>();

            return services;
        }
    }
}
