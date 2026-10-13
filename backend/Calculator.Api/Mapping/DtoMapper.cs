using Calculator.Contracts.Auth;
using Calculator.Contracts.Books;
using Calculator.Contracts.Calculations;
using Calculator.Contracts.History;
using Calculator.Data.Entities;
using Calculator.Services.Models;
using Riok.Mapperly.Abstractions;

namespace Calculator.Api.Mapping;

/// <summary>Mapperly implementation of <see cref="IDtoMapper"/>; the method bodies are generated at compile time.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class DtoMapper : IDtoMapper
{
    [MapProperty(nameof(CalculationRequest.A), nameof(UserCalcLog.OperandA))]
    [MapProperty(nameof(CalculationRequest.B), nameof(UserCalcLog.OperandB))]
    [MapperIgnoreTarget(nameof(UserCalcLog.Id))]
    [MapperIgnoreTarget(nameof(UserCalcLog.UserId))]
    [MapperIgnoreTarget(nameof(UserCalcLog.Result))]
    [MapperIgnoreTarget(nameof(UserCalcLog.CreatedAtUtc))]
    public partial UserCalcLog ToEntity(CalculationRequest request);

    public partial CalculationResponse ToCalculationResponse(UserCalcLog calculation);

    public partial HistoryItemResponse ToHistoryItem(UserCalcLog calculation);

    public partial IReadOnlyList<HistoryItemResponse> ToHistory(IReadOnlyList<UserCalcLog> calculations);

    public partial LoginResponse ToLoginResponse(AuthResult result);

    [MapperIgnoreTarget(nameof(Book.Id))]
    public partial Book ToEntity(BookRequest request);

    public partial BookResponse ToBookResponse(Book book);

    public partial IReadOnlyList<BookResponse> ToBooks(IReadOnlyList<Book> books);

    /// <summary>SQLite does not keep <see cref="DateTimeKind"/>; all stored times are UTC, so mark them as such.</summary>
    [UserMapping(Default = true)]
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
