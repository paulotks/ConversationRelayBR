using ConversartionRelayBR.Models.WebSocket.TwilioSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Security;

namespace ConversartionRelayBR.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateTwilioRequestAttribute : TypeFilterAttribute
    {
        public ValidateTwilioRequestAttribute() : base(typeof(ValidateTwilioRequestFilter))
        {
        }
    }

    internal class ValidateTwilioRequestFilter : IAsyncActionFilter
    {
        private readonly RequestValidator _requestValidator;
        private readonly TwilioSettings _twilioSettings;
        private readonly bool _isEnabled;

        public ValidateTwilioRequestFilter(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
            var authToken = _twilioSettings.TwilioAuthToken;
            _requestValidator = new RequestValidator(authToken);
            _isEnabled = _twilioSettings.RequestValidationEnabled;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!_isEnabled)
            {
                await next();
                return;
            }

            var httpContext = context.HttpContext;
            var request = httpContext.Request;

            var requestUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            Dictionary<string, string> parameters = null;

            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync(httpContext.RequestAborted).ConfigureAwait(false);
                parameters = form.ToDictionary(p => p.Key, p=> p.Value.ToString());
            }

            var signature = request.Headers["X-Twilio-Signature"];
            var isValid = _requestValidator.Validate(requestUrl, parameters, signature);

            if (!isValid)
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;

            }
            await next();
        }

    }
}
