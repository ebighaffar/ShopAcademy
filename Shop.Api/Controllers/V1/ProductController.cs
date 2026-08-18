using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore.Attributes;
using Shop.Api.Constants;
using Shop.Api.Contracts;
using Shop.Api.Contracts.Product;
using Shop.Api.Mappers;
using Shop.Application.Services;

namespace Shop.Api.Controllers.V1;

[ApiVersion(1.0)]
public class ProductController(ProductService productService) : BaseController
{
    [HttpGet(ProductUriConstants.GetAll)]
    public async Task<ApiResult<List<ProductResponse>>> GetAll([FromQuery] ProductListFilter inputFilter,
        CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(inputFilter.ToDto(), cancellationToken);
        return products.Select(p => p.ToResponse()).ToList();
    }

    [HttpGet(ProductUriConstants.GetById)]
    public async Task<ApiResult<ProductResponse>> GetById(long id, long categoryId,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        var response = product.ToResponse();
        return response;
    }

    [HttpPost(ProductUriConstants.Create)]
    public async Task<ApiResult<ProductResponse>> Create(CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request.ToDto(), cancellationToken);
        var response = product.ToResponse();
        return ApiResult<ProductResponse>.Created(response, $"/api/v1/products/{response.Id}");
    }

    [HttpPut(ProductUriConstants.Update)]
    public async Task<ApiResult<ProductResponse>> Update(long id, UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateAsync(id, request.ToDto(), cancellationToken);
        return product.ToResponse();
    }

    [HttpDelete(ProductUriConstants.Delete)]
    public async Task<ApiResult> Delete(long id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        return ApiResult.NoContent();
    }
}