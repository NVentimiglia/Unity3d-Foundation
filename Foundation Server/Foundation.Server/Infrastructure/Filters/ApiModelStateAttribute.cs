using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Linq;
using Newtonsoft.Json;

namespace Foundation.Server.Infrastructure.Filters
{
    public class ApiValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext aContext)
        {
            if (!aContext.ModelState.IsValid)
            {
                var errorList = aContext.ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var model = new
                {
                    Message = "The request is invalid.",
                    ModelState = errorList,
                };
                var json = JsonConvert.SerializeObject(model);

                aContext.Response = aContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, aContext.ModelState);
                aContext.Response.Headers.Add("Message", json);
            }
        }

    }


}