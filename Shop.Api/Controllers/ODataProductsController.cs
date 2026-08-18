namespace Shop.Api.Controllers;

/* OData is temporarily disabled.
/// <summary>
/// Read-only, version-neutral OData endpoint for products.
/// </summary>
[ApiVersionNeutral]
public sealed class ProductsController(
    IProductRepository productRepository
) : ODataController
{
    /// <summary>
    /// Supports $select, $filter, $orderby, $expand, $count, $top, and $skip.
    /// </summary>
    [EnableQuery(MaxExpansionDepth = 2)]
    public IQueryable<Product> Get()
    {
        return productRepository.GetAllAsQueryable();
    }

    /// <summary>
    /// Returns one product through the conventional /odata/Products(key) route.
    /// </summary>
    [EnableQuery(MaxExpansionDepth = 2)]
    public SingleResult<Product> Get(long key)
    {
        return SingleResult.Create(
            productRepository.GetAllAsQueryable().Where(product => product.Id == key));
    }
}
*/
