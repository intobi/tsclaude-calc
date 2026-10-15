using Calculator.Api.Mapping;
using Calculator.Contracts.Auth;
using Calculator.Contracts.Authors;
using Calculator.Contracts.Books;
using Calculator.Contracts.Calculations;
using Calculator.Data.Entities;
using Calculator.Services.Models;
using DtoGender = Calculator.Contracts.Authors.Gender;
using EntityGender = Calculator.Data.Entities.Gender;

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
    public void ToForgotPasswordResponse_CopiesTheTokenAndExpiry()
    {
        var expires = new DateTime(2026, 10, 15, 9, 30, 0, DateTimeKind.Utc);

        Assert.Equal(new ForgotPasswordResponse("token", expires),
            _mapper.ToForgotPasswordResponse(new PasswordResetTicket("token", expires)));
        Assert.Equal(new ForgotPasswordResponse(null, null),
            _mapper.ToForgotPasswordResponse(PasswordResetTicket.NoAccount));
    }

    private static readonly Author Herbert = new() { Id = 7, FirstName = "Frank", LastName = "Herbert", Gender = EntityGender.Male, DateOfBirth = new DateOnly(1920, 10, 8) };
    private static readonly AuthorResponse HerbertResponse = new(7, "Frank", "Herbert", DtoGender.Male, new DateOnly(1920, 10, 8));

    [Fact]
    public void ToEntity_Book_TakesTheNameAndAuthorIdAndLeavesIdUnset()
    {
        var book = _mapper.ToEntity(new BookRequest("Dune", 7));

        Assert.Equal((0, "Dune", 7), (book.Id, book.Name, book.AuthorId));
    }

    [Fact]
    public void ToBooks_MapsEveryBookWithItsAuthor()
    {
        var books = _mapper.ToBooks([
            new Book { Id = 1, Name = "Dune", AuthorId = 7, Author = Herbert },
            new Book { Id = 2, Name = "Dune Messiah", AuthorId = 7, Author = Herbert },
        ]);

        Assert.Equal([new BookResponse(1, "Dune", HerbertResponse), new BookResponse(2, "Dune Messiah", HerbertResponse)], books);
    }

    [Fact]
    public void ToEntity_Author_TakesEveryFieldAndLeavesIdUnset()
    {
        var author = _mapper.ToEntity(new AuthorRequest("Jane", "Austen", DtoGender.Female, new DateOnly(1775, 12, 16)));

        Assert.Equal((0, "Jane", "Austen", EntityGender.Female, new DateOnly(1775, 12, 16)),
            (author.Id, author.FirstName, author.LastName, author.Gender, author.DateOfBirth));
    }

    [Fact]
    public void ToAuthors_MapsEveryAuthor()
    {
        Assert.Equal([HerbertResponse], _mapper.ToAuthors([Herbert]));
    }
}
