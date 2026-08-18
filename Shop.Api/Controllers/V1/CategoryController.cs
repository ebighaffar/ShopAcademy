using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Constants;
using Shop.Api.Contracts;
using Shop.Api.Contracts.Category;
using Shop.Api.Mappers;
using Shop.Application.Services;

namespace Shop.Api.Controllers.V1;

[ApiVersion(1.0)]
public class CategoryController(CategoryService categoryService) : BaseController
{
    [HttpGet(CategoryUriConstants.GetAll)]
    public async Task<ApiResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetAllAsync(cancellationToken);
        return categories.Select(c => c.ToResponse()).ToList();
    }

    [HttpGet(CategoryUriConstants.GetById)]
    public async Task<ApiResult<CategoryResponse>> GetById(long id, CancellationToken cancellationToken)
    {
        var category = await categoryService.GetByIdAsync(id, cancellationToken);
        return category.ToResponse();
    }

    [HttpPost(CategoryUriConstants.Create)]
    public async Task<ApiResult<CategoryResponse>> Create(CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.CreateAsync(request.ToDto(), cancellationToken);
        var response = category.ToResponse();
        return ApiResult<CategoryResponse>.Created(response, null);
    }

    [HttpPut(CategoryUriConstants.Update)]
    public async Task<ApiResult<CategoryResponse>> Update(long id, UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.UpdateAsync(id, request.ToDto(), cancellationToken);
        return category.ToResponse();
    }

    [HttpDelete(CategoryUriConstants.Delete)]
    public async Task<ApiResult> Delete(long id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);
        return ApiResult.NoContent();
    }
}