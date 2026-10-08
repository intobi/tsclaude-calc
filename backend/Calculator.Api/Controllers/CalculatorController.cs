using System.Security.Claims;
using Calculator.Api.Dtos;
using Calculator.Api.Entities;
using Calculator.Api.Repositories;
using Calculator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class CalculatorController(ICalculatorService calculatorService, IUserCalcLogRepository calcLogRepository)
    : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<ActionResult<CalculationResponse>> Calculate(CalculationRequest request)
    {
        double result;
        try
        {
            result = calculatorService.Calculate(request.A, request.B, request.Operation);
        }
        catch (Exception ex) when (ex is DivisionByZeroCalculatorException or UnknownOperationException)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await calcLogRepository.AddAsync(new UserCalcLog
        {
            UserId = userId,
            Operation = request.Operation,
            OperandA = request.A,
            OperandB = request.B,
            Result = result,
            CreatedAtUtc = DateTime.UtcNow,
        });

        return Ok(new CalculationResponse(result));
    }
}
