using Enhanzer.Api.Entities;

namespace Enhanzer.Api.Interfaces;

public interface IItemRepository
{
    Task<List<ItemMaster>> GetAllAsync(CancellationToken cancellationToken = default);
}
