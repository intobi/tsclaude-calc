using Calculator.Contracts.Auth;
using Calculator.Contracts.Calculations;
using Calculator.Contracts.History;
using Calculator.Data.Entities;
using Calculator.Services.Models;

namespace Calculator.Api.Mapping;

/// <summary>Converts between the frontend DTOs (Calculator.Contracts) and entities/service results.</summary>
public interface IDtoMapper
{
    /// <summary>A keypad calculation as a new history entity (result, user and time are set by the service).</summary>
    UserCalcLog ToEntity(CalculationRequest request);

    CalculationResponse ToCalculationResponse(UserCalcLog calculation);

    HistoryItemResponse ToHistoryItem(UserCalcLog calculation);

    IReadOnlyList<HistoryItemResponse> ToHistory(IReadOnlyList<UserCalcLog> calculations);

    LoginResponse ToLoginResponse(AuthResult result);
}
