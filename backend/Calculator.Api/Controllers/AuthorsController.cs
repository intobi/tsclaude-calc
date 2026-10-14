using Calculator.Api.Mapping;
using Calculator.Contracts.Authors;
using Calculator.Services.Authors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/authors")]
public class AuthorsController(IAuthorService authorService, IDtoMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuthorResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var authors = await authorService.GetAllAsync(cancellationToken);
        return Ok(mapper.ToAuthors(authors));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var author = await authorService.GetByIdAsync(id, cancellationToken);
        return author is null ? NotFound() : Ok(mapper.ToAuthorResponse(author));
    }

    /// <remarks>A date of birth in the future gets 400 from <see cref="Infrastructure.LibraryExceptionHandler"/>.</remarks>
    [HttpPost]
    public async Task<ActionResult<AuthorResponse>> Create(AuthorRequest request, CancellationToken cancellationToken)
    {
        var author = await authorService.CreateAsync(mapper.ToEntity(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = author.Id }, mapper.ToAuthorResponse(author));
    }

    /// <remarks>A date of birth in the future gets 400 from <see cref="Infrastructure.LibraryExceptionHandler"/>.</remarks>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AuthorResponse>> Update(
        int id,
        AuthorRequest request,
        CancellationToken cancellationToken)
    {
        var author = await authorService.UpdateAsync(id, mapper.ToEntity(request), cancellationToken);
        return author is null ? NotFound() : Ok(mapper.ToAuthorResponse(author));
    }

    /// <remarks>An author who still has books gets 409 from <see cref="Infrastructure.LibraryExceptionHandler"/>.</remarks>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await authorService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
