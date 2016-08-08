using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MapStache.Web.Startup))]
namespace MapStache.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
