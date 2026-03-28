using Enhanzer.Api.Entities;
using Enhanzer.Api.Interfaces;
using Enhanzer.Api.Services;
using Moq;

namespace Enhanzer.Api.Tests;

public class MasterDataServiceTests
{
    [Fact]
    public async Task GetMasterDataAsync_ReturnsSeededLocationsAndItems()
    {
        var locationRepository = new Mock<ILocationRepository>();
        var itemRepository = new Mock<IItemRepository>();

        locationRepository.Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Location { Id = 1, Code = "LOC001", Name = "Warehouse A" }
            ]);

        itemRepository.Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ItemMaster { Id = 1, Name = "Mango" }
            ]);

        var service = new MasterDataService(locationRepository.Object, itemRepository.Object);

        var locations = await service.GetLocationsAsync();
        var items = await service.GetItemsAsync();

        Assert.Single(locations);
        Assert.Single(items);
        Assert.Equal("LOC001", locations[0].Code);
        Assert.Equal("Mango", items[0].Name);
    }
}
