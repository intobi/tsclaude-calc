using System.Security.Claims;
using Calculator.Api.Dtos;
using Calculator.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/history")]
public class HistoryController(IUserCalcLogRepository calcLogRepository) : ControllerBase
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 200;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HistoryItemResponse>>> Get([FromQuery] int limit = DefaultLimit)
    {
        if (limit is < 1 or > MaxLimit)
        {
            return BadRequest(new ErrorResponse($"limit must be between 1 and {MaxLimit}"));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var logs = await calcLogRepository.GetLatestForUserAsync(userId, limit);

        // SQLite does not preserve DateTimeKind; values are stored as UTC.
        return Ok(logs
            .Select(log => new HistoryItemResponse(
                log.Id,
                log.Operation,
                log.OperandA,
                log.OperandB,
                log.Result,
                DateTime.SpecifyKind(log.CreatedAtUtc, DateTimeKind.Utc)))
            .ToList());
    }
}
