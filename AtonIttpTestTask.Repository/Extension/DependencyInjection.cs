using AtonIttpTestTask.Domain.Interfaces;
using AtonIttpTestTask.Repository.DataBases;
using AtonIttpTestTask.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtonIttpTestTask.Repository.Extension
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddDbContext<ApplicationDbContext>();

            return services;
        }
    }
}
