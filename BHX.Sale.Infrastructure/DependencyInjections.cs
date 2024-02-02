using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BHX.Sale.Application.Core.Services;
using BHX.Sale.Domain.Core.Repositories;
using BHX.Sale.Infrastructure.Repositories;
using BHX.Sale.Infrastructure.Services;
using BHX.Sale.Domain.Enums;

namespace BHX.Sale.Infrastructure
{
    public static class DependencyInjections
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped(typeof(IBaseRepositoryAsync<>), typeof(BaseRepositoryAsync<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ILoggerService, LoggerService>();
        }
    }
}