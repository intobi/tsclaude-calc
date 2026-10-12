using Calculator.Api.Extensions;
using Calculator.Api.Mapping;
using Calculator.Contracts;
using Calculator.Contracts.History;
using Calculator.Services.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/history")]
public class HistoryController(IHistoryService historyService, IDtoMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HistoryItemResponse>>> Get(
        CancellationToken cancellationToken,
        [FromQuery] int limit = IHistoryService.DefaultLimit)
    {
        if (limit is < 1 or > IHistoryService.MaxLimit)
        {
            return BadRequest(new ErrorResponse($"limit must be between 1 and {IHistoryService.MaxLimit}"));
        }

        var calculations = await historyService.GetLatestAsync(User.GetUserId(), limit, cancellationToken);
        return Ok(mapper.ToHistory(calculations));
    }
}
