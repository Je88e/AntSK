using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AntSK.Domain.Common.DependencyInjection
{
    public static class JesseExtensions
    {
        public static IServiceCollection AddJesseConfiguration(this IServiceCollection services)
        {
            services.AddHttpClient("JCustom");
            return services;
        }

        public static WebApplication UseJesseSetting(this WebApplication app)
        {
            app.UseCors(option=>option.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            return app;
        }
    }
}
