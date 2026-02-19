using AzuriteUI.Web.Services.Azurite.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AzuriteUI.Web.Services.Azurite.ExceptionHandlers;

public class RangeNotSatisfiableExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is RangeNotSatisfiableException rangeEx)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Range Not Satisfiable",
                Detail = rangeEx.Message,
                Status = StatusCodes.Status416RangeNotSatisfiable,
                Type = "https://tools.ietf.org/html/rfc7807"
            };
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
        return false;
    }
}
