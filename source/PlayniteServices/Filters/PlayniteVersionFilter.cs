using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Filters
{
    public class PlayniteVersionFilter : ActionFilterAttribute
    {
        private AppSettings appSettings;

        public PlayniteVersionFilter(IOptions<AppSettings> settings)
        {
            appSettings = settings.Value;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (appSettings.RestrictPlayniteVersion && appSettings.RestrictedPlayniteVersions?.Any() == true)
            {
                var allowRequest = false;
                if (context.HttpContext.Request.Headers.TryGetValue("Playnite-Version", out var headerVer))
                {
                    if (appSettings.RestrictedPlayniteVersions.Contains(headerVer))
                    {
                        allowRequest = true;
                    }
                }

                if (!allowRequest)
                {
                    context.Result = new JsonResult(new ErrorResponse("Bad version request."));
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
