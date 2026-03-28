using Enhanzer.Api.Entities;

namespace Enhanzer.Api.Interfaces;

public interface ILocationRepository
{
    Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default);
}
