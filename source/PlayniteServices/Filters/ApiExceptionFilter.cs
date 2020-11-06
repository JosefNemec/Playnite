using System;
using System.Collections.Generic;
using System.IO;
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

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.HttpContext.Request.Path == "/playnite/users")
            {
                base.OnException(context);
                return;
            }

            logger.Error(context.Exception, $"Request failed: {context.HttpContext.Request.Method}, {context.HttpContext.Request.Path}");
            if (context.HttpContext.Request.Method == "POST" && context.HttpContext.Request.ContentLength > 0)
            {
                context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(context.HttpContext.Request.Body))
                {
                    logger.Error(await reader.ReadToEndAsync());
                }
            }

            context.Result = new JsonResult(new ErrorResponse(context.Exception));
            base.OnException(context);
        }
    }
}
