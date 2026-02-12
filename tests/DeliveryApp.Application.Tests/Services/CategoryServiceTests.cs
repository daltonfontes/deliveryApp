using DeliveryApp.Application.DTOs.Categories;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DeliveryApp.Application.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_repoMock.Object);
    }

    private static Category CreateCategory(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Bebidas",
        Description = "Bebidas em geral",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsResponse_WhenCategoryExists()
    {
        var category = CreateCategory();
        _repoMock.Setup(r => r.GetByIdAsync(category.Id, default)).ReturnsAsync(category);

        var result = await _sut.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Category?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        var categories = new[] { CreateCategory(), CreateCategory() };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(categories);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsResponse()
    {
        var request = new CreateCategoryRequest("Pizzas", "Pizzas artesanais");

        var result = await _sut.CreateAsync(request);

        result.Name.Should().Be("Pizzas");
        result.Description.Should().Be("Pizzas artesanais");
        result.Id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), default), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsResponse_WhenCategoryExists()
    {
        var category = CreateCategory();
        _repoMock.Setup(r => r.GetByIdAsync(category.Id, default)).ReturnsAsync(category);
        var request = new UpdateCategoryRequest("Novo Nome", "Nova Desc");

        var result = await _sut.UpdateAsync(category.Id, request);

        result.Name.Should().Be("Novo Nome");
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenCategoryDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Category?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateCategoryRequest("x", "y"));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_DeletesCategory_WhenCategoryExists()
    {
        var category = CreateCategory();
        _repoMock.Setup(r => r.GetByIdAsync(category.Id, default)).ReturnsAsync(category);

        await _sut.DeleteAsync(category.Id);

        _repoMock.Verify(r => r.DeleteAsync(category, default), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenCategoryDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Category?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
