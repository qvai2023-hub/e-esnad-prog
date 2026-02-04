using Owin;
using Microsoft.Owin;

using MyWebApplication;

[assembly: OwinStartup(typeof(Startup))]
namespace MyWebApplication
{
    public class Startup

    {
        public void Configuration(IAppBuilder   app)
        {
            app.MapSignalR();
        }
    }
}