using System;
using System.Linq;
using Application.Services.Common;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Services
{
    internal static class Startup
    {
        /// <summary>
        /// Add Services to DO container automatically.
        /// </summary>
        /// <param name="services">Services</param>
        /// <returns>Service collection with registered services with their respective lifetime in the service container</returns>
        internal static IServiceCollection AddServices(this IServiceCollection services)
        {

            #region Transient Services

            var transientServiceType = typeof(ITransientService);

            // Get services inheriting transient service
            var transientServices = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(transientServiceType.IsAssignableFrom)
                .Where(p => p.IsClass && !p.IsAbstract)
                .Select(p => new
                {
                    Service = p.GetInterfaces().FirstOrDefault(),
                    Implementation = p
                });

            // Register each transient service for startup
            if (transientServices.Count() > 0)
            {
                Log.Information($"Registering {transientServices.Count()} Transient Service(s)");
                foreach (var transientService in transientServices)
                {
                    if (transientServiceType.IsAssignableFrom(transientService.Service))
                    {
                        services.AddTransient(transientService.Service, transientService.Implementation);
                    }
                }
            }

            #endregion

            #region Scoped Services

            var scopedServiceType = typeof(IScopedService);

            // Get services inheriting scoped service
            var scopedServices = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(scopedServiceType.IsAssignableFrom)
                .Where(p => p.IsClass && !p.IsAbstract)
                .Select(p =>
                {
                    var serviceInterface = p.GetInterfaces()
                        .Where(i => i != scopedServiceType && scopedServiceType.IsAssignableFrom(i))
                        .SelectMany(i => i.GetInterfaces().Prepend(i))
                        .FirstOrDefault(i => !scopedServiceType.IsAssignableFrom(i));

                    serviceInterface ??= p.GetInterfaces()
                        .FirstOrDefault(i => i != scopedServiceType);

                    return new
                    {
                        Service = serviceInterface,
                        Implementation = p
                    };
                })
                .Where(x => x.Service is not null);

            // Register each scoped service for startup
            if (scopedServices.Count() > 0)
            {
                Log.Information($"Registering {scopedServices.Count()} Scoped Service(s)");
            }
            foreach (var scopedService in scopedServices)
            {
                services.AddScoped(scopedService.Service, scopedService.Implementation);
            }

            #endregion Scoped Services

            #region Singleton Services

            var singletonServiceType = typeof(ISingletonService);

            // Get services inheriting singleton service
            var singletonServices = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(singletonServiceType.IsAssignableFrom)
                .Where(p => p.IsClass && !p.IsAbstract)
                .Select(p => new
                {
                    Service = p.GetInterfaces().FirstOrDefault(),
                    Implementation = p
                });

            // Register each singleton service for startup
            if (singletonServices.Count() > 0)
            {
                Log.Information($"Registering {singletonServices.Count()} Singleton Service(s)");
            }
            foreach (var singletonService in singletonServices)
            {
                if (singletonServiceType.IsAssignableFrom(singletonService.Service))
                {
                    services.AddSingleton(singletonService.Service, singletonService.Implementation);
                }
            }

            #endregion Singleton Services

            return services;

        }
    }
}
