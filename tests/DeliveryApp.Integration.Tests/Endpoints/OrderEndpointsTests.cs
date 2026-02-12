using System.Net;
using System.Net.Http.Json;
using DeliveryApp.Application.DTOs.Categories;
using DeliveryApp.Application.DTOs.Customers;
using DeliveryApp.Application.DTOs.DeliveryDrivers;
using DeliveryApp.Application.DTOs.Orders;
using DeliveryApp.Application.DTOs.Products;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Integration.Tests.Infrastructure;
using FluentAssertions;

namespace DeliveryApp.Integration.Tests.Endpoints;

public class OrderEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _adminClient;
    private readonly HttpClient _unauthClient;

    public OrderEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _adminClient = TestAuthHelper.CreateAuthenticatedClient(factory, "Admin");
        _unauthClient = factory.CreateClient();
    }

    private async Task<Guid> CreateCustomerAsync()
    {
        var response = await _adminClient.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Order Customer", $"oc-{Guid.NewGuid():N}@test.com"[..30], "11999999999", "Rua A"));
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        return body!.Id;
    }

    private async Task<Guid> CreateProductAsync()
    {
        var catResponse = await _adminClient.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest($"OrdCat-{Guid.NewGuid():N}"[..20], "Test"));
        var cat = await catResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        var response = await _adminClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Order Product", "Desc", 25.00m, null, cat!.Id));
        var body = await response.Content.ReadFromJsonAsync<ProductResponse>();
        return body!.Id;
    }

    private async Task<Guid> CreateDriverAsync()
    {
        var response = await _adminClient.PostAsJsonAsync("/api/delivery-drivers",
            new CreateDeliveryDriverRequest("Order Driver", "11888888888", VehicleType.Motorcycle));
        var body = await response.Content.ReadFromJsonAsync<DeliveryDriverResponse>();
        return body!.Id;
    }

    private async Task<OrderResponse> CreateOrderAsync()
    {
        var customerId = await CreateCustomerAsync();
        var productId = await CreateProductAsync();
        var customerClient = TestAuthHelper.CreateAuthenticatedClient(_factory, "Customer");

        var response = await customerClient.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(customerId, "Rua Entrega, 456", new List<OrderItemRequest>
            {
                new(productId, 2)
            }));
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithAdminToken()
    {
        var response = await _adminClient.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_Returns401_WithoutToken()
    {
        var response = await _unauthClient.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Returns201_WithCustomerToken()
    {
        var customerId = await CreateCustomerAsync();
        var productId = await CreateProductAsync();
        var customerClient = TestAuthHelper.CreateAuthenticatedClient(_factory, "Customer");

        var response = await customerClient.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(customerId, "Rua B", new List<OrderItemRequest> { new(productId, 1) }));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be(OrderStatus.Pending);
        body.TotalAmount.Should().Be(25.00m);
    }

    [Fact]
    public async Task Create_Returns404_WhenCustomerDoesNotExist()
    {
        var productId = await CreateProductAsync();
        var customerClient = TestAuthHelper.CreateAuthenticatedClient(_factory, "Customer");

        var response = await customerClient.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(Guid.NewGuid(), "Rua X", new List<OrderItemRequest> { new(productId, 1) }));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsOk_WithAdminToken()
    {
        var order = await CreateOrderAsync();

        var response = await _adminClient.PatchAsJsonAsync($"/api/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Confirmed));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task AssignDriver_ReturnsOk_WithAdminToken()
    {
        var order = await CreateOrderAsync();
        var driverId = await CreateDriverAsync();

        var response = await _adminClient.PatchAsJsonAsync($"/api/orders/{order.Id}/assign-driver",
            new AssignDriverRequest(driverId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.DeliveryDriverId.Should().Be(driverId);
    }

    [Fact]
    public async Task Delete_Returns204_WithAdminToken()
    {
        var order = await CreateOrderAsync();

        var response = await _adminClient.DeleteAsync($"/api/orders/{order.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
