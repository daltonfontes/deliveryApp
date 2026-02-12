using System.Net;
using System.Net.Http.Json;
using DeliveryApp.Application.DTOs.Categories;
using DeliveryApp.Application.DTOs.Products;
using DeliveryApp.Integration.Tests.Infrastructure;
using FluentAssertions;

namespace DeliveryApp.Integration.Tests.Endpoints;

public class ProductEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _adminClient;

    public ProductEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _adminClient = TestAuthHelper.CreateAuthenticatedClient(factory, "Admin");
    }

    private async Task<Guid> CreateCategoryAsync()
    {
        var response = await _adminClient.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest($"ProdCat-{Guid.NewGuid():N}"[..20], "Test"));
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        return body!.Id;
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithoutAuth()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Returns201_WithAdminToken()
    {
        var categoryId = await CreateCategoryAsync();
        var request = new CreateProductRequest("Test Product", "Desc", 29.90m, null, categoryId);

        var response = await _adminClient.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        var categoryId = await CreateCategoryAsync();
        var createResponse = await _adminClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Prod", "D", 10m, null, categoryId));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.GetAsync($"/api/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenExists()
    {
        var categoryId = await CreateCategoryAsync();
        var createResponse = await _adminClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Prod", "D", 10m, null, categoryId));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _adminClient.PutAsJsonAsync($"/api/products/{created!.Id}",
            new UpdateProductRequest("Updated", "New Desc", 45m, null, true, categoryId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Returns204_WhenExists()
    {
        var categoryId = await CreateCategoryAsync();
        var createResponse = await _adminClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("ToDelete", "D", 10m, null, categoryId));
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _adminClient.DeleteAsync($"/api/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
