using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Newtonsoft.Json;

namespace Foundation.Server.Infrastructure.Filters
{
    /// <summary>
    /// Adds ModelState and a friendly Message to the response
    /// </summary>
    public class ApiHandleErrorAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext aContext)
        {
            var message = aContext.Exception.Message;
            var request = aContext.ActionContext.Request;

            var errorList = aContext.ActionContext.ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var model = new
            {
                Message = message,
                ModelState = errorList,
            };

            var json = JsonConvert.SerializeObject(model);

            aContext.Response = request.CreateResponse(HttpStatusCode.InternalServerError, model);

            aContext.Response.Headers.Add("Message", json);
        }

    }
}