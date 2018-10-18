using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Blog_IT.Startup))]
namespace Blog_IT
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
