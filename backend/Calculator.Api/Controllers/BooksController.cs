using Calculator.Api.Mapping;
using Calculator.Contracts.Books;
using Calculator.Services.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/books")]
public class BooksController(IBookService bookService, IDtoMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await bookService.GetAllAsync(cancellationToken);
        return Ok(mapper.ToBooks(books));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await bookService.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : Ok(mapper.ToBookResponse(book));
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(BookRequest request, CancellationToken cancellationToken)
    {
        var book = await bookService.CreateAsync(mapper.ToEntity(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, mapper.ToBookResponse(book));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookResponse>> Update(
        int id,
        BookRequest request,
        CancellationToken cancellationToken)
    {
        var book = await bookService.UpdateAsync(id, mapper.ToEntity(request), cancellationToken);
        return book is null ? NotFound() : Ok(mapper.ToBookResponse(book));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await bookService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
