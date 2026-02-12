using DeliveryApp.Application.DTOs.DeliveryDrivers;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DeliveryApp.Application.Tests.Services;

public class DeliveryDriverServiceTests
{
    private readonly Mock<IDeliveryDriverRepository> _repoMock = new();
    private readonly DeliveryDriverService _sut;

    public DeliveryDriverServiceTests()
    {
        _sut = new DeliveryDriverService(_repoMock.Object);
    }

    private static DeliveryDriver CreateDriver(Guid? id = null, bool isAvailable = true) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Carlos Motorista",
        Phone = "11999999999",
        VehicleType = VehicleType.Motorcycle,
        IsAvailable = isAvailable,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsResponse_WhenDriverExists()
    {
        var driver = CreateDriver();
        _repoMock.Setup(r => r.GetByIdAsync(driver.Id, default)).ReturnsAsync(driver);

        var result = await _sut.GetByIdAsync(driver.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(driver.Id);
        result.VehicleType.Should().Be(VehicleType.Motorcycle);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenDriverDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((DeliveryDriver?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDrivers()
    {
        var drivers = new[] { CreateDriver(), CreateDriver() };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(drivers);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAvailableAsync_ReturnsOnlyAvailableDrivers()
    {
        var available = new[] { CreateDriver(isAvailable: true) };
        _repoMock.Setup(r => r.GetAvailableDriversAsync(default)).ReturnsAsync(available);

        var result = await _sut.GetAvailableAsync();

        result.Should().HaveCount(1);
        _repoMock.Verify(r => r.GetAvailableDriversAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsResponse()
    {
        var request = new CreateDeliveryDriverRequest("Pedro", "11888888888", VehicleType.Car);

        var result = await _sut.CreateAsync(request);

        result.Name.Should().Be("Pedro");
        result.VehicleType.Should().Be(VehicleType.Car);
        result.IsAvailable.Should().BeTrue();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<DeliveryDriver>(), default), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsResponse_WhenDriverExists()
    {
        var driver = CreateDriver();
        _repoMock.Setup(r => r.GetByIdAsync(driver.Id, default)).ReturnsAsync(driver);
        var request = new UpdateDeliveryDriverRequest("Novo Nome", "11777777777", VehicleType.Van, false);

        var result = await _sut.UpdateAsync(driver.Id, request);

        result.Name.Should().Be("Novo Nome");
        result.IsAvailable.Should().BeFalse();
        result.VehicleType.Should().Be(VehicleType.Van);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenDriverDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((DeliveryDriver?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateDeliveryDriverRequest("x", "1", VehicleType.Bicycle, true));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenDriverDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((DeliveryDriver?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
