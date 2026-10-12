using Calculator.Api.Dtos;
using Calculator.Api.Extensions;
using Calculator.Contracts.Calculations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class CalculatorController(ICalculatorService calculatorService) : ControllerBase
{
    /// <remarks>Invalid calculations (division by zero, overflow) are turned into 400 by
    /// <see cref="Infrastructure.CalculationExceptionHandler"/>.</remarks>
    [HttpPost("calculate")]
    public async Task<ActionResult<CalculationResponse>> Calculate(
        CalculationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await calculatorService.CalculateAsync(
            User.GetUserId(), request.A, request.B, request.Operation, cancellationToken);
        return Ok(new CalculationResponse(result));
    }
}
