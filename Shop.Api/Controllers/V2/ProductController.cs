using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Constants;
using Shop.Api.Contracts;
using Shop.Api.Contracts.Product;
using Shop.Api.Mappers;
using Shop.Api.Mappers.V2;
using Shop.Application.Services;
using ProductResponseV2 = Shop.Api.Contracts.V2.Product.ProductResponse;

namespace Shop.Api.Controllers.V2;

[ApiVersion(2.0)]
public class ProductController(ProductService productService) : BaseController
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
    
}
