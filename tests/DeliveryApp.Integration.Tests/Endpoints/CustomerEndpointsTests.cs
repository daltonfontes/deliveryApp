using System.Net;
using System.Net.Http.Json;
using DeliveryApp.Application.DTOs.Customers;
using DeliveryApp.Integration.Tests.Infrastructure;
using FluentAssertions;

namespace DeliveryApp.Integration.Tests.Endpoints;

public class CustomerEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _adminClient;

    public CustomerEndpointsTests(CustomWebApplicationFactory factory)
    {
        _adminClient = TestAuthHelper.CreateAuthenticatedClient(factory, "Admin");
    }

    private CreateCustomerRequest UniqueCustomer() =>
        new("Test Customer", $"cust-{Guid.NewGuid():N}@test.com"[..30], "11999999999", "Rua Test, 123");

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _adminClient.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Returns201()
    {
        var response = await _adminClient.PostAsJsonAsync("/api/customers", UniqueCustomer());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        var createResponse = await _adminClient.PostAsJsonAsync("/api/customers", UniqueCustomer());
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var response = await _adminClient.GetAsync($"/api/customers/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        var response = await _adminClient.GetAsync($"/api/customers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenExists()
    {
        var createResponse = await _adminClient.PostAsJsonAsync("/api/customers", UniqueCustomer());
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var response = await _adminClient.PutAsJsonAsync($"/api/customers/{created!.Id}",
            new UpdateCustomerRequest("Updated", "updated@test.com", "11888888888", "Rua Nova"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Returns204_WhenExists()
    {
        var createResponse = await _adminClient.PostAsJsonAsync("/api/customers", UniqueCustomer());
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var response = await _adminClient.DeleteAsync($"/api/customers/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
