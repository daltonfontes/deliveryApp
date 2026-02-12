using System.Net;
using System.Net.Http.Json;
using DeliveryApp.Application.DTOs.DeliveryDrivers;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Integration.Tests.Infrastructure;
using FluentAssertions;

namespace DeliveryApp.Integration.Tests.Endpoints;

public class DeliveryDriverEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _adminClient;

    public DeliveryDriverEndpointsTests(CustomWebApplicationFactory factory)
    {
        _adminClient = TestAuthHelper.CreateAuthenticatedClient(factory, "Admin");
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _adminClient.GetAsync("/api/delivery-drivers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Returns201()
    {
        var request = new CreateDeliveryDriverRequest("Driver Test", "11999999999", VehicleType.Motorcycle);

        var response = await _adminClient.PostAsJsonAsync("/api/delivery-drivers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        var response = await _adminClient.GetAsync($"/api/delivery-drivers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenExists()
    {
        var createResponse = await _adminClient.PostAsJsonAsync("/api/delivery-drivers",
            new CreateDeliveryDriverRequest("Driver", "11999999999", VehicleType.Car));
        var created = await createResponse.Content.ReadFromJsonAsync<DeliveryDriverResponse>();

        var response = await _adminClient.PutAsJsonAsync($"/api/delivery-drivers/{created!.Id}",
            new UpdateDeliveryDriverRequest("Updated Driver", "11888888888", VehicleType.Van, false));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Returns204_WhenExists()
    {
        var createResponse = await _adminClient.PostAsJsonAsync("/api/delivery-drivers",
            new CreateDeliveryDriverRequest("ToDelete", "11999999999", VehicleType.Bicycle));
        var created = await createResponse.Content.ReadFromJsonAsync<DeliveryDriverResponse>();

        var response = await _adminClient.DeleteAsync($"/api/delivery-drivers/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
