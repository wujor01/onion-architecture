using Microsoft.Extensions.DependencyInjection;
using BHX.Sale.Application.Interfaces;
using BHX.Sale.Application.Services;

namespace BHX.Sale.Application
{
    public static class DependencyInjections
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
        }
    }
}