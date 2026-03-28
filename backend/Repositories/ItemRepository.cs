using Enhanzer.Api.Data;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Repositories;

public class ItemRepository(ApplicationDbContext dbContext) : IItemRepository
{
    public Task<List<ItemMaster>> GetAllAsync(CancellationToken cancellationToken = default) =>
        dbContext.Items
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
}
