using DeliveryApp.Application.DTOs.Products;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DeliveryApp.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repoMock.Object);
    }

    private static Product CreateProduct(Guid? id = null, bool isActive = true) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Pizza Margherita",
        Description = "Pizza clássica",
        Price = 35.90m,
        ImageUrl = "https://img.com/pizza.jpg",
        IsActive = isActive,
        CategoryId = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsResponse_WhenProductExists()
    {
        var product = CreateProduct();
        _repoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Price.Should().Be(35.90m);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        var products = new[] { CreateProduct(), CreateProduct() };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(products);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveProducts()
    {
        var activeProducts = new[] { CreateProduct(isActive: true) };
        _repoMock.Setup(r => r.GetActiveProductsAsync(default)).ReturnsAsync(activeProducts);

        var result = await _sut.GetActiveAsync();

        result.Should().HaveCount(1);
        _repoMock.Verify(r => r.GetActiveProductsAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsResponse()
    {
        var categoryId = Guid.NewGuid();
        var request = new CreateProductRequest("Hamburguer", "Artesanal", 29.90m, null, categoryId);

        var result = await _sut.CreateAsync(request);

        result.Name.Should().Be("Hamburguer");
        result.Price.Should().Be(29.90m);
        result.CategoryId.Should().Be(categoryId);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsResponse_WhenProductExists()
    {
        var product = CreateProduct();
        _repoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        var request = new UpdateProductRequest("Novo Nome", "Nova Desc", 45.00m, null, true, product.CategoryId);

        var result = await _sut.UpdateAsync(product.Id, request);

        result.Name.Should().Be("Novo Nome");
        result.Price.Should().Be(45.00m);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateProductRequest("x", "y", 1, null, true, Guid.NewGuid()));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
