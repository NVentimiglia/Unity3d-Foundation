using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Foundation.Server.Infrastructure.Filters
{
    public class AuthorizeApplicationId : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var password = AppConfig.Settings.ApplicationId;
            if (string.IsNullOrEmpty(password))
                return;

            var header = actionContext.Request.Headers.GetValues("APPLICATIONID");

            if (header == null|| header.FirstOrDefault() != password)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "ApplicationId is required");
            }
        }
    }
}