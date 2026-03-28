using Enhanzer.Api.Data;
using Enhanzer.Api.Entities;
using Enhanzer.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Repositories;

public class LocationRepository(ApplicationDbContext dbContext) : ILocationRepository
{
    public Task<List<Location>> GetAllAsync(CancellationToken cancellationToken = default) =>
        dbContext.Locations
            .AsNoTracking()
            .OrderBy(location => location.Name)
            .ToListAsync(cancellationToken);
}
