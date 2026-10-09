using Calculator.Api.Dtos;
using Calculator.Api.Extensions;
using Calculator.Services.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/history")]
public class HistoryController(IHistoryService historyService) : ControllerBase
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

        var items = await historyService.GetLatestAsync(User.GetUserId(), limit, cancellationToken);
        return Ok(items
            .Select(item => new HistoryItemResponse(
                item.Id, item.Operation, item.OperandA, item.OperandB, item.Result, item.CreatedAtUtc))
            .ToList());
    }
}
