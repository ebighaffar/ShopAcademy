using Shop.Application.Repository;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.SqlServer.EfCore.Repositories;

public class CategoryRepository(ShopDbContext context) : RepositoryBase<Category, long>(context), ICategoryRepository;