using DeliveryApp.Application.DTOs.Customers;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DeliveryApp.Application.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repoMock = new();
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _sut = new CustomerService(_repoMock.Object);
    }

    private static Customer CreateCustomer(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "João Silva",
        Email = "joao@test.com",
        Phone = "11999999999",
        Address = "Rua A, 123",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsResponse_WhenCustomerExists()
    {
        var customer = CreateCustomer();
        _repoMock.Setup(r => r.GetByIdAsync(customer.Id, default)).ReturnsAsync(customer);

        var result = await _sut.GetByIdAsync(customer.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.Name.Should().Be(customer.Name);
        result.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCustomerDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Customer?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCustomers()
    {
        var customers = new[] { CreateCustomer(), CreateCustomer() };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(customers);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsResponse()
    {
        var request = new CreateCustomerRequest("Maria", "maria@test.com", "11888888888", "Rua B, 456");

        var result = await _sut.CreateAsync(request);

        result.Name.Should().Be("Maria");
        result.Email.Should().Be("maria@test.com");
        result.Id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), default), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsResponse_WhenCustomerExists()
    {
        var customer = CreateCustomer();
        _repoMock.Setup(r => r.GetByIdAsync(customer.Id, default)).ReturnsAsync(customer);
        var request = new UpdateCustomerRequest("Novo Nome", "novo@test.com", "11777777777", "Rua C, 789");

        var result = await _sut.UpdateAsync(customer.Id, request);

        result.Name.Should().Be("Novo Nome");
        result.Email.Should().Be("novo@test.com");
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenCustomerDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Customer?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateCustomerRequest("x", "x@x.com", "1", "a"));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_DeletesCustomer_WhenCustomerExists()
    {
        var customer = CreateCustomer();
        _repoMock.Setup(r => r.GetByIdAsync(customer.Id, default)).ReturnsAsync(customer);

        await _sut.DeleteAsync(customer.Id);

        _repoMock.Verify(r => r.DeleteAsync(customer, default), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenCustomerDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Customer?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
