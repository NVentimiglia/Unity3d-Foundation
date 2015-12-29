using System;
using System.Data.Entity;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Foundation.Server.Infrastructure;
using Foundation.Server.Models;

namespace Foundation.Server
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // DEV MODE, new DB each load
            Database.SetInitializer(new DropCreateDatabaseAlways<AppDatabase>());
            //Force refresh
            new AppDatabase().Users.Find("");

            //

            // Disable Database.SetInitializer in production
            // Database.SetInitializer(null);

            // Alternatively, enable migrations
            // https://msdn.microsoft.com/en-us/data/jj591621.aspx?f=255&MSPPError=-2147217396
        }

        // Custom User Identity / Principal
        protected void Application_OnAuthenticateRequest(object sender, EventArgs e)
        {
            // Get Custom User Session
            var ticket = AppAuthorization.GetSession();

            if (ticket == null)
            {
                return;
            }

            // Use Custom User Session to update Asp.net Identity
            var user = new FoundationPrincipal
            {
                Identity = new FoundationIdentity
                {
                    Name = ticket.UserId,
                    IsAuthenticated = ticket.IsAuthenticated,
                }
            };

            HttpContext.Current.User = user;
        }

    }
}
