using Shop.Application.Repository;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class ProductRepository(ShopDbContext context) : RepositoryBase<Product>(context), IProductRepository;