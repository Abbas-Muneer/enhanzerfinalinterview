using Enhanzer.Api.DTOs;

namespace Enhanzer.Api.Interfaces;

public interface IMasterDataService
{
    Task<IReadOnlyList<LocationDto>> GetLocationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);
}
