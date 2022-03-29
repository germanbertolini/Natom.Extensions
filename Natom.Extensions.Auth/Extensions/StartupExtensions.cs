using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Natom.Extensions.Auth.Attributes;
using Natom.Extensions.Auth.Entities;
using Natom.Extensions.Auth.PackageConfig;
using Natom.Extensions.Auth.Profile;
using Natom.Extensions.Auth.Services;
using Natom.Extensions.Cache.Services;
using Natom.Extensions.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Natom.AccessMonitor.Extensions
{
    public static class StartupExtensions
    {
        private static bool _hostedServiceAdded = false;

        public static IServiceCollection AddAuthService(this IServiceCollection service, string scope)
        {
            service.AddSingleton<AuthService>();
            service.AddSingleton(new AuthServiceConfig { Scope = scope });
            service.AddSingleton(new Mapper(MappingProfile.Build()));

            service.AddScoped<AccessToken>();

            return service;
        }

        public static IApplicationBuilder InitPermissions(this IApplicationBuilder service, Assembly assembly, AuthService authService)
        {
            var permissions = new List<TienePermisoAttribute>();
            var controllerType = typeof(Controller);
            var controllerTypeAPI = typeof(ControllerBase);
            var controllers = assembly.GetTypes()
                                        .Where(p => (controllerType.IsAssignableFrom(p) || controllerTypeAPI.IsAssignableFrom(p)));

            foreach (var controller in controllers)
            {
                var controllerPermissionAttribute = controller.GetCustomAttribute<TienePermisoAttribute>();
                var endpoints = controller.GetMethods();

                foreach (var endpoint in endpoints)
                {
                    var endpointPermissionAttribute = endpoint.GetCustomAttribute<TienePermisoAttribute>();
                    if (controllerPermissionAttribute != null)
                    {
                        var endpointName = endpoint.GetCustomAttribute<ActionNameAttribute>()?.Name ?? endpoint.Name;
                        authService.RegisterEndpointPermission(controller.FullName + "." + endpointName, controllerPermissionAttribute.Permiso);
                    }
                    else if (endpointPermissionAttribute != null)
                    {
                        var endpointName = endpoint.GetCustomAttribute<ActionNameAttribute>()?.Name ?? endpoint.Name;
                        authService.RegisterEndpointPermission(controller.FullName + "." + endpointName, endpointPermissionAttribute.Permiso);
                    }
                }
            }

            return service;
        }
    }
}