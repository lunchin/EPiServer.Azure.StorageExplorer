using EPiServer.Authorization;
using EPiServer.Azure.StorageExplorer;
using EPiServer.Shell.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace EPiServer.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorageExplorer(this IServiceCollection services)
        {
            services.AddSingleton<IStorageService, StorageService>();
            services.Configure<ProtectedModuleOptions>(
                pm =>
                {
                    if (!pm.Items.Any(i => i.Name.Equals("EPiServer.Azure.StorageExplorer", System.StringComparison.OrdinalIgnoreCase)))
                    {
                        pm.Items.Add(new ModuleDetails() { Name = "EPiServer.Azure.StorageExplorer" });
                    }
                });
            services.Configure<AuthorizationOptions>(x => x.TryAddPolicy("episerver:storage", p => p.RequireRole("CmsAdmins")));
            services.Configure<StaticFileOptions>(x =>
            {
                if (x.ContentTypeProvider is FileExtensionContentTypeProvider fe)
                {
                    if (!fe.Mappings.ContainsKey(".woff2"))
                    {
                        fe.Mappings.Add(".woff2", "font/woff2");
                    }
                    if (!fe.Mappings.ContainsKey(".woff"))
                    {
                        fe.Mappings.Add(".woff", "font/woff");
                    }
                }
            });
            return services;
        }
    }
}
