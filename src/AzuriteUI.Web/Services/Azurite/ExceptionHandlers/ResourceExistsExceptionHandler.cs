using AzuriteUI.Web.Services.Azurite.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AzuriteUI.Web.Services.Azurite.ExceptionHandlers;

public class ResourceExistsExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ResourceExistsException existsEx)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Resource Already Exists",
                Detail = existsEx.Message,
                Status = StatusCodes.Status409Conflict,
                Type = "https://tools.ietf.org/html/rfc7807"
            };
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
        return false;
    }
}
