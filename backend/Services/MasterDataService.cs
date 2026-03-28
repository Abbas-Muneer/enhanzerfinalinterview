using Enhanzer.Api.DTOs;
using Enhanzer.Api.Interfaces;

namespace Enhanzer.Api.Services;

public class MasterDataService(
    ILocationRepository locationRepository,
    IItemRepository itemRepository) : IMasterDataService
{
    public async Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        var locations = await locationRepository.GetAllAsync(cancellationToken);
        return locations.Select(location => new LocationDto
        {
            Id = location.Id,
            Code = location.Code,
            Name = location.Name
        }).ToList();
    }

    public async Task<IReadOnlyList<ItemDto>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        var items = await itemRepository.GetAllAsync(cancellationToken);
        return items.Select(item => new ItemDto
        {
            Id = item.Id,
            Name = item.Name
        }).ToList();
    }
}
