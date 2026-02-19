using AzuriteUI.Web.Services.Azurite.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzuriteUI.Web.Services.Azurite.ExceptionHandlers;

public class AzuriteServiceExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is AzuriteServiceException azuriteEx)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Azurite Service Error",
                Detail = azuriteEx.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7807"
            };
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
        return false;
    }
}
