using Calculator.Api.Mapping;
using Calculator.Contracts.Books;
using Calculator.Contracts.Calculations;
using Calculator.Data.Entities;
using Calculator.Services.Models;

namespace Calculator.Tests.Api;

public class DtoMapperTests
{
    private readonly IDtoMapper _mapper = new DtoMapper();

    private static UserCalcLog Calculation(int id, double result) => new()
    {
        Id = id,
        UserId = "user-1",
        Operation = "multiply",
        OperandA = 6,
        OperandB = 7,
        Result = result,
        // SQLite returns stored UTC times without a DateTimeKind.
        CreatedAtUtc = new DateTime(2026, 10, 12, 9, 30, 0, DateTimeKind.Unspecified),
    };

    [Fact]
    public void ToEntity_MapsOperandsAndOperationOnly()
    {
        var entity = _mapper.ToEntity(new CalculationRequest(6, 7, "multiply"));

        Assert.Equal(6, entity.OperandA);
        Assert.Equal(7, entity.OperandB);
        Assert.Equal("multiply", entity.Operation);
        Assert.Equal(0, entity.Id);
        Assert.Equal(0, entity.Result);
    }

    [Fact]
    public void ToCalculationResponse_ReturnsTheResult()
    {
        Assert.Equal(42, _mapper.ToCalculationResponse(Calculation(1, 42)).Result);
    }

    [Fact]
    public void ToHistory_MapsEveryItemAndMarksTimesAsUtc()
    {
        var history = _mapper.ToHistory([Calculation(2, 42), Calculation(1, 13)]);

        Assert.Equal([2, 1], history.Select(h => h.Id));
        var item = history[0];
        Assert.Equal(("multiply", 6d, 7d, 42d), (item.Operation, item.OperandA, item.OperandB, item.Result));
        Assert.Equal(DateTimeKind.Utc, item.CreatedAtUtc.Kind);
        Assert.Equal(new DateTime(2026, 10, 12, 9, 30, 0), item.CreatedAtUtc);
    }

    [Fact]
    public void ToLoginResponse_CopiesTheToken()
    {
        var expires = new DateTime(2026, 10, 12, 10, 0, 0, DateTimeKind.Utc);

        var response = _mapper.ToLoginResponse(new AuthResult("token", "user@example.com", expires));

        Assert.Equal(("token", "user@example.com", expires), (response.Token, response.Email, response.ExpiresAtUtc));
    }

    [Fact]
    public void ToEntity_Book_TakesTheNameAndLeavesIdUnset()
    {
        var book = _mapper.ToEntity(new BookRequest("Dune"));

        Assert.Equal((0, "Dune"), (book.Id, book.Name));
    }

    [Fact]
    public void ToBooks_MapsEveryBook()
    {
        var books = _mapper.ToBooks([new Book { Id = 1, Name = "Dune" }, new Book { Id = 2, Name = "Emma" }]);

        Assert.Equal([new BookResponse(1, "Dune"), new BookResponse(2, "Emma")], books);
    }
}
