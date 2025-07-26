using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Booking.Web.Attributes
{
    public class ConditionalAntiforgeryAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _exemptActions;

        public ConditionalAntiforgeryAttribute(params string[] exemptActions)
        {
            _exemptActions = exemptActions ?? Array.Empty<string>();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            
            // Check if this action should be exempt from antiforgery validation
            var isExempt = _exemptActions.Contains(actionName, StringComparer.OrdinalIgnoreCase) ||
                          IsPaymentCallback(context.HttpContext.Request.Path) ||
                          IsStripeWebhook(context.HttpContext.Request);

            if (!isExempt && IsStateChangingRequest(context.HttpContext.Request))
            {
                var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
                try
                {
                    await antiforgery.ValidateRequestAsync(context.HttpContext);
                }
                catch (AntiforgeryValidationException)
                {
                    context.Result = new BadRequestObjectResult("Invalid antiforgery token");
                    return;
                }
            }

            await next();
        }

        private static bool IsPaymentCallback(PathString path)
        {
            var pathValue = path.Value?.ToLower();
            return pathValue?.Contains("/payment/") == true ||
                   pathValue?.Contains("/booking/confirmation") == true ||
                   pathValue?.Contains("/stripe/") == true ||
                   pathValue?.Contains("/webhook") == true;
        }

        private static bool IsStripeWebhook(HttpRequest request)
        {
            return request.Headers.ContainsKey("Stripe-Signature");
        }

        private static bool IsStateChangingRequest(HttpRequest request)
        {
            return request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                   request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                   request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
                   request.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
        }
    }
}