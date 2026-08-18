using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Constants;
using Shop.Api.Contracts;
using Shop.Api.Contracts.Product;
using Shop.Api.Controllers;
using Shop.Api.Mappers;
using Shop.Api.Mappers.V2;
using Shop.Application.Services;
using ProductResponseV2 = Shop.Api.Contracts.V2.Product.ProductResponse;

namespace Shop.Api.Controllers.V2;

[ApiVersion(2.0)]
public class ProductController(ProductService productService) : ApiControllerBase
{
    [HttpGet(ProductUriConstants.GetAll)]
    public async Task<ApiResult<List<ProductResponseV2>>> GetAll(
        [FromQuery] ProductListFilter inputFilter,
        CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(inputFilter.ToDto(), cancellationToken);
        return products.Select(product => product.ToV2Response()).ToList();
    }

    [HttpGet(ProductUriConstants.GetById)]
    public async Task<ApiResult<ProductResponseV2>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product.ToV2Response();
    }

    [HttpPost(ProductUriConstants.Create)]
    public async Task<ApiResult<ProductResponseV2>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request.ToDto(), cancellationToken);
        var response = product.ToV2Response();
        return ApiResult<ProductResponseV2>.Created(response, $"/api/v2/products/{response.Id}");
    }

    [HttpPut(ProductUriConstants.Update)]
    public async Task<ApiResult<ProductResponseV2>> Update(
        long id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateAsync(id, request.ToDto(), cancellationToken);
        return product.ToV2Response();
    }

    [HttpDelete(ProductUriConstants.Delete)]
    public async Task<ApiResult> Delete(long id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        return ApiResult.NoContent();
    }
}
