using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using MvcSample.Web.Filters;
using MvcSample.Web.Services;
using System.Linq;

#if NET45 
using Autofac;
using Microsoft.Framework.DependencyInjection.Autofac;
using Castle.Windsor;
using Microsoft.Framework.DependencyInjection.Windsor;
using Microsoft.Framework.OptionsModel;
using Castle.MicroKernel.Lifestyle;
#endif

namespace MvcSample.Web
{
    public class Startup
    {
        public void Configure(IBuilder app)
        {
            app.UseFileServer();
#if NET45
            var configuration = new Configuration()
                                    .AddJsonFile(@"App_Data\config.json")
                                    .AddEnvironmentVariables();

            string diSystem;

            if (configuration.TryGet("DependencyInjection", out diSystem))
            {
                var services = new ServiceCollection();

                var defaultServices = Microsoft.AspNet.Hosting.HostingServices.GetDefaultServices();
                foreach (var defaultService in defaultServices)
                {
                    services.Add(defaultService);
                }

                services.AddMvc();
                services.AddSingleton<PassThroughAttribute>();
                services.AddSingleton<UserNameService>();
                services.AddTransient<ITestService, TestService>();
                services.Add(OptionsServices.GetDefaultServices());

                if (diSystem.Equals("AutoFac", StringComparison.OrdinalIgnoreCase))
                {
                    app.UseMiddleware<MonitoringMiddlware>();

                    ContainerBuilder builder = new ContainerBuilder();

                    AutofacRegistration.Populate(
                     builder,
                     services,
                     fallbackServiceProvider: app.ApplicationServices);

                    builder.RegisterModule<MonitoringModule>();

                    var container = builder.Build();

                    app.UseServices(container.Resolve<IServiceProvider>());
                }
                else if(diSystem.Equals("Windsor", StringComparison.OrdinalIgnoreCase))
                {
                    var container = new WindsorContainer();
                    WindsorRegistration.Populate(container, services, app.ApplicationServices);
                    var sp = container.Resolve<IServiceProvider>();

                    container.BeginScope();

                    app.UseServices(sp);
                }
                else
                {
                    throw new ArgumentException("Unknown dependency injection container: " + diSystem);
                }
            }
            else
#endif
            {
                app.UseServices(services =>
                {
                    services.AddMvc();
                    services.AddSingleton<PassThroughAttribute>();
                    services.AddSingleton<UserNameService>();
                    services.AddTransient<ITestService, TestService>();
                });
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute("areaRoute", "{area:exists}/{controller}/{action}");

                routes.MapRoute(
                    "controllerActionRoute",
                    "{controller}/{action}",
                    new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    "controllerRoute",
                    "{controller}",
                    new { controller = "Home" });
            });
        }
    }
}
