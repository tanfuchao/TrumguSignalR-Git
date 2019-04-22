using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(TrumguSignalR.Startup))]
namespace TrumguSignalR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //测试
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888

            //将UserFactory注入
            var userFactory = new UserFactory();
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => userFactory);

            var config = new HubConfiguration
            {
                EnableJSONP = true,
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            };
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR(config);
        }
    }

}
