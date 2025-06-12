using Application;
using Application.IRepos;
using Application.IServices;
using Application.Services;
using Infra.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfraService(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper (AppDomain.CurrentDomain.GetAssemblies());
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(config.GetConnectionString("ShortLink_DB")));

            services.AddScoped<IShortUrlRepo, ShortUrlRepo>();
            services.AddScoped<IShortUrlService, ShortUrlService>();

            return services;
        }
    }
}
