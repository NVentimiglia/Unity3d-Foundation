using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using System.Linq;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace Foundation.Server.Infrastructure.Filters
{

    /// <summary>
    /// Sets the Model State for Bad Requests 
    /// </summary>
    public class ApiBadRequestAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext aContext, CancellationToken cancellationToken)
        {
            if (aContext.Response == null || aContext.Response.IsSuccessStatusCode)
                return;
            
            var errorList = aContext.ActionContext.ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var m = await aContext.Response.Content.ReadAsStringAsync();
            var m2 = JsonConvert.DeserializeObject<JObject>(m)["Message"];

            var model = new
            {
                Message = m2,
                ModelState = errorList,
            };
           

            var json = JsonConvert.SerializeObject(model);

            aContext.Response.Headers.Add("Message", json);
        }
    }
}