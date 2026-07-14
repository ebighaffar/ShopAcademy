using Microsoft.AspNetCore.Mvc;
using Shop.Api.Contracts.Categories;
using Shop.Api.Mappers;
using Shop.Application.Services;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(CategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetAllAsync(cancellationToken);
        return Ok(categories.Select(c => c.ToResponse()));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CategoryResponse>> GetById(long id, CancellationToken cancellationToken)
    {
        var category = await categoryService.GetByIdAsync(id, cancellationToken);
        return Ok(category.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.CreateAsync(request.ToDto(), cancellationToken);
        var response = category.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CategoryResponse>> Update(long id, UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.UpdateAsync(id, request.ToDto(), cancellationToken);
        return Ok(category.ToResponse());
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}