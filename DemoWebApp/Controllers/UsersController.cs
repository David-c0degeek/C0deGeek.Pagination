using C0deGeek.Pagination;
using C0deGeek.Pagination.Core;
using C0deGeek.Pagination.Extensions;
using C0deGeek.Pagination.Http;
using C0deGeek.Pagination.Services;
using DemoWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Polly.CircuitBreaker;

namespace DemoWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(PaginationService<User, ApplicationDbContext> paginationService) : PaginationControllerBase
{
    [HttpGet]
    [EnableRateLimiting("fixed")]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] PaginationParameters parameters,
        [FromHeader(Name = "If-None-Match")] string? ifNoneMatch = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            parameters.ETag = ifNoneMatch?.Trim('"');

            var result = await paginationService.GetPagedDataAsync(
                parameters,
                cancellationToken);

            if (result is null)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            SetPaginationHeaders(result);

            var response = new PaginationResponse<User>(
                result.Items,
                result,
                GenerateLinks(result, nameof(GetUsers), new { parameters.PageSize })
            );

            if (!Request.Headers.AcceptEncoding.Contains("gzip"))
            {
                return Ok(response);
            }
                
            Response.Headers.ContentEncoding = "gzip";
            var compressed = await ResponseCompression.CompressResponse(response);
            return File(compressed, "application/json");
        }
        catch (RateLimitExceededException)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, "Too many requests");
        }
        catch (BrokenCircuitException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service temporarily unavailable");
        }
    }
}