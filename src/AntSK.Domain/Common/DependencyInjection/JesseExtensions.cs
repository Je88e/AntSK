using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BifrostiC.SparkDesk.ChatDoc.Common.Dependency;

namespace AntSK.Domain.Common.DependencyInjection
{
    public static class JesseExtensions
    {
        public static IServiceCollection AddJesseConfiguration(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddHttpClient("JCustom");

            services.AddChatDoc(configuration);
            return services;
        }

        public static WebApplication UseJesseSetting(this WebApplication app)
        {
            app.UseCors(option=>option.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            return app;
        }
    }
}
