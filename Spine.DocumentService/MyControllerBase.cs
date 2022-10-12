using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Spine.DocumentService
{
    public interface IMyControllerBase
    {
        Guid? CompanyId { get; set; }
        Guid? UserId { get; set; }
        string Username { get; set; }

    }

    [SetUserPropertiesFilter]
    public class MyControllerBase : ControllerBase, IMyControllerBase
    {
        public Guid? CompanyId { get; set; }
        public Guid? UserId { get; set; }
        public string Username { get; set; }

    }

    public class SetUserPropertiesFilter : Attribute, IAsyncActionFilter
    {
        public SetUserPropertiesFilter()
        {
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
        {
            var user = actionContext.HttpContext.User;
            if (actionContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var controller = actionContext.Controller as IMyControllerBase;
                if (controller != null)
                {
                    controller.Username = user.Identity.Name;
                    if (user.HasClaim(c => c.Type == ClaimTypes.NameIdentifier)) // "UserId"
                    {
                        var userId = Guid.Parse(user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
                        controller.UserId = userId;
                    }
                    if (user.HasClaim(c => c.Type == "CompanyId"))
                    {
                        var companyId = Guid.Parse(user.Claims.FirstOrDefault(c => c.Type == "CompanyId").Value);
                        controller.CompanyId = companyId;
                    }
                }
            }

            await next();
        }

    }
}
