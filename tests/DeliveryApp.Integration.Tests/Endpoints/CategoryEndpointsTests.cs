using System.Net;
using System.Net.Http.Json;
using DeliveryApp.Application.DTOs.Categories;
using DeliveryApp.Integration.Tests.Infrastructure;
using FluentAssertions;

namespace DeliveryApp.Integration.Tests.Endpoints;

public class CategoryEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _adminClient;

    public CategoryEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _adminClient = TestAuthHelper.CreateAuthenticatedClient(factory, "Admin");
    }

    private CreateCategoryRequest UniqueCategory() =>
        new($"Cat-{Guid.NewGuid():N}"[..20], "Test category");

    [Fact]
    public async Task GetAll_ReturnsOk_WithoutAuth()
    {
        var response = await _client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Returns201_WithAdminToken()
    {
        var request = UniqueCategory();

        var response = await _adminClient.PostAsJsonAsync("/api/categories", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        body!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        var createResponse = await _adminClient.PostAsJsonAsync("/api/categories", UniqueCategory());
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        var response = await _client.GetAsync($"/api/categories/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        var response = await _client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenExists()
    {
        var createResponse = await _adminClient.PostAsJsonAsync("/api/categories", UniqueCategory());
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        var update = new UpdateCategoryRequest("Updated Name", "Updated Desc");

        var response = await _adminClient.PutAsJsonAsync($"/api/categories/{created!.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        body!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Delete_Returns204_WhenExists()
    {
        var createResponse = await _adminClient.PostAsJsonAsync("/api/categories", UniqueCategory());
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        var response = await _adminClient.DeleteAsync($"/api/categories/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
