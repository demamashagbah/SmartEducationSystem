using Microsoft.Extensions.DependencyInjection;
using SmartEducation.Application.Interfaces;
using SmartEducation.Persistence.Repositories;

namespace SmartEducation.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
        {
            // 1. Register the Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // 2. Register the Specific Subject Repository
            services.AddScoped<ISubjectRepository, SubjectRepository>();

            // 3. Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
