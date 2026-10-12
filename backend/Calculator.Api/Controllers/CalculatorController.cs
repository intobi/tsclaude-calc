using Calculator.Api.Extensions;
using Calculator.Api.Mapping;
using Calculator.Contracts.Calculations;
using Calculator.Services.Calculations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class CalculatorController(ICalculatorService calculatorService, IDtoMapper mapper) : ControllerBase
{
    /// <remarks>Invalid calculations (division by zero, overflow) are turned into 400 by
    /// <see cref="Infrastructure.CalculationExceptionHandler"/>.</remarks>
    [HttpPost("calculate")]
    public async Task<ActionResult<CalculationResponse>> Calculate(
        CalculationRequest request,
        CancellationToken cancellationToken)
    {
        var calculation = mapper.ToEntity(request);
        var saved = await calculatorService.CalculateAsync(User.GetUserId(), calculation, cancellationToken);
        return Ok(mapper.ToCalculationResponse(saved));
    }
}
