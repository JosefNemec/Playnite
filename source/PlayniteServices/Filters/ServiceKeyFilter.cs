using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Filters
{
    public class ServiceKeyFilter : ActionFilterAttribute
    {
        private UpdatableAppSettings appSettings;

        public ServiceKeyFilter(UpdatableAppSettings settings)
        {
            appSettings = settings;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var allowRequest = false;
            if (context.HttpContext.Request.Headers.TryGetValue("Service-Key", out var headerVer) &&
                appSettings.Settings.ServiceKey == headerVer)
            {
                allowRequest = true;
            }

            if (!allowRequest)
            {
                context.Result = new JsonResult(new ErrorResponse("Bad request."));
            }

            base.OnActionExecuting(context);
        }
    }
}
