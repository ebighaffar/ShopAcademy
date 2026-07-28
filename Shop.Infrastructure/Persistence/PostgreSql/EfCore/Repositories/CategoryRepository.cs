using Shop.Application.Repositories;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

public class CategoryRepository(ShopDbContext context) : RepositoryBase<Category>(context), ICategoryRepository;