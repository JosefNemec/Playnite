using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Playnite.SDK;

namespace PlayniteServices.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private static ILogger logger = LogManager.GetLogger();

        public override void OnException(ExceptionContext context)
        {
            logger.Error(context.Exception, $"Request failed: {context.HttpContext.Request.Method}, {context.HttpContext.Request.Path}");
            context.Result = new JsonResult(new ErrorResponse(context.Exception));
            base.OnException(context);
        }
    }
}
