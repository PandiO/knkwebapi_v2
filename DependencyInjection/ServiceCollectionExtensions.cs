using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Services;

namespace knkwebapi_v2.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Centralized registration for application services and repositories.
        /// - Add explicit registrations for special cases.
        /// - Use simple convention scanning to auto-register IThing -> Thing for types named *Repository or *Service.
        /// Consider adding Scrutor (NuGet) for richer scanning and lifetime control when needed.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration? configuration = null)
        {
            // explicit registrations (recommended for clarity)
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDomainRepository, DomainRepository>();
            services.AddScoped<IDomainService, DomainService>();
            services.AddScoped<IFormConfigurationRepository, FormConfigurationRepository>();
            services.AddScoped<IFormConfigurationService, FormConfigurationService>();
            services.AddScoped<IFormStepRepository, FormStepRepository>();
            services.AddScoped<IFormStepService, FormStepService>();
            services.AddScoped<IFormFieldRepository, FormFieldRepository>();
            services.AddScoped<IFormFieldService, FormFieldService>();
            services.AddScoped<IFormSubmissionProgressRepository, FormSubmissionProgressRepository>();
            services.AddScoped<IFormSubmissionProgressService, FormSubmissionProgressService>();

            // convention-based registrations for other services/repositories in the same assembly
            var asm = Assembly.GetExecutingAssembly();
            var candidates = asm.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && (t.Name.EndsWith("Repository") || t.Name.EndsWith("Service")));

            foreach (var impl in candidates)
            {
                var iface = impl.GetInterfaces().FirstOrDefault(i => i.Name == "I" + impl.Name);
                if (iface != null)
                {
                    // avoid double-registering explicit ones
                    if (!services.Any(sd => sd.ServiceType == iface)) 
                    {
                        services.AddScoped(iface, impl);
                    }
                }
            }

            return services;
        }
    }
}
