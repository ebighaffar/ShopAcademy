using Shop.Application.Repository;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.SqlServer.EfCore.Repositories;

public class ProductRepository(ShopDbContext context) : RepositoryBase<Product, long>(context), IProductRepository;
