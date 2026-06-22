using Template.Services.Shared;
using Microsoft.Extensions.DependencyInjection;
using Template.Services.Shared;
using Template.Web.SignalR;

namespace Template.Web
{
    public class Container
    {
        public static void RegisterTypes(IServiceCollection container)
        {
            // Registration of all the database services you have
            container.AddScoped<SharedService>();
            container.AddScoped<AppuntoService>();

            // Registration of SignalR events
            container.AddScoped<IPublishDomainEvents, SignalrPublishDomainEvents>();
        }
    }
}
