using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ArabErp.Web.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
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
