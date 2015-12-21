using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Foundation.Server.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiCheckModelForNullAttribute : ActionFilterAttribute
    {
        private readonly Func<Dictionary<string, object>, bool> _validate;

        public ApiCheckModelForNullAttribute()
            : this(arguments =>
                arguments.ContainsValue(null))
        { }

        public ApiCheckModelForNullAttribute(Func<Dictionary<string, object>, bool> checkCondition)
        {
            _validate = checkCondition;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (_validate(actionContext.ActionArguments))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, string.Format("The parameter {0} cannot be null", actionContext.ActionArguments.First(o=> o.Value == null).Key));
            }
        }
    }
}