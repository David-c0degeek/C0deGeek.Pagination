using C0deGeek.Pagination.Light.Configuration;
using C0deGeek.Pagination.Light.Http;
using C0deGeek.Pagination.Light.Models;
using C0deGeek.Pagination.Light.Services;
using DemoWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DemoWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserControllerLight(
    LightPaginationService paginationService,
    IOptions<LightPaginationOptions> options,
    IUserService userService)
    : LightPaginationControllerBase(options.Value)
{
    private readonly LightPaginationOptions _options = options.Value;

    [HttpGet]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetItems(
        [FromQuery] LightPaginationParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = userService.GetItems();
        
        var result = await paginationService.CreatePaginatedResultAsync(
            items, 
            parameters,
            cancellationToken);
            
        return await CreatePaginatedResponseAsync(
            result, 
            _options,
            nameof(GetItems),
            new { parameters.PageSize });
    }
}