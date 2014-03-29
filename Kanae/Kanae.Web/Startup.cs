using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Kanae.Web.Startup))]
namespace Kanae.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
