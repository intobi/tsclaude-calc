using Calculator.Api.Dtos;
using Calculator.Api.Extensions;
using Calculator.Services.Calculations;
using Calculator.Services.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class CalculatorController(ICalculatorService calculatorService) : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<ActionResult<CalculationResponse>> Calculate(
        CalculationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await calculatorService.CalculateAsync(
                User.GetUserId(), request.A, request.B, request.Operation, cancellationToken);
            return Ok(new CalculationResponse(result));
        }
        catch (CalculationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }
}
