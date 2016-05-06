using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ArabErp.Web.Startup))]
namespace ArabErp.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
