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
    private readonly HttpClient _customerClient;
    private readonly HttpClient _unauthClient;

    public OrderEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _adminClient = TestAuthHelper.CreateAuthenticatedClient(factory, "Admin");
        _customerClient = TestAuthHelper.CreateAuthenticatedClient(factory, "Customer");
        _unauthClient = factory.CreateClient();
    }

    private async Task<Guid> CreateCustomerAsync()
    {
        var response = await _adminClient.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Order Customer", $"oc-{Guid.NewGuid():N}"[..30], "11999999999", "Rua A"));
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

        var response = await _customerClient.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(customerId, "Rua Entrega, 456", new List<OrderItemRequest>
            {
                new(productId, 2)
            }));
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }

    private async Task<OrderResponse> AdvanceOrderToStatusAsync(Guid orderId, OrderStatus targetStatus)
    {
        OrderResponse? result = null;

        if (targetStatus >= OrderStatus.Confirmed)
        {
            var r = await _customerClient.PatchAsync($"/api/orders/{orderId}/pay", null);
            result = await r.Content.ReadFromJsonAsync<OrderResponse>();
        }
        if (targetStatus >= OrderStatus.Preparing)
        {
            var r = await _adminClient.PatchAsync($"/api/orders/{orderId}/prepare", null);
            result = await r.Content.ReadFromJsonAsync<OrderResponse>();
        }
        if (targetStatus >= OrderStatus.Shipped)
        {
            var driverId = await CreateDriverAsync();
            var r = await _adminClient.PatchAsJsonAsync($"/api/orders/{orderId}/ship", new ShipOrderRequest(driverId));
            result = await r.Content.ReadFromJsonAsync<OrderResponse>();
        }

        return result!;
    }

    // ─── GET ─────────────────────────────────────────────────────────────────

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

    // ─── CREATE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_Returns201_WithCustomerToken()
    {
        var customerId = await CreateCustomerAsync();
        var productId = await CreateProductAsync();

        var response = await _customerClient.PostAsJsonAsync("/api/orders",
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

        var response = await _customerClient.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(Guid.NewGuid(), "Rua X", new List<OrderItemRequest> { new(productId, 1) }));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── PAY ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Pay_ReturnsOk_WithCustomerToken()
    {
        var order = await CreateOrderAsync();

        var response = await _customerClient.PatchAsync($"/api/orders/{order.Id}/pay", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task Pay_Returns401_WithoutToken()
    {
        var order = await CreateOrderAsync();

        var response = await _unauthClient.PatchAsync($"/api/orders/{order.Id}/pay", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── PREPARE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Prepare_ReturnsOk_WithAdminToken()
    {
        var order = await CreateOrderAsync();
        await AdvanceOrderToStatusAsync(order.Id, OrderStatus.Confirmed);

        var response = await _adminClient.PatchAsync($"/api/orders/{order.Id}/prepare", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be(OrderStatus.Preparing);
    }

    // ─── SHIP ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Ship_ReturnsOk_WithAdminToken()
    {
        var order = await CreateOrderAsync();
        await AdvanceOrderToStatusAsync(order.Id, OrderStatus.Preparing);
        var driverId = await CreateDriverAsync();

        var response = await _adminClient.PatchAsJsonAsync($"/api/orders/{order.Id}/ship",
            new ShipOrderRequest(driverId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be(OrderStatus.Shipped);
        body.DeliveryDriverId.Should().Be(driverId);
    }

    [Fact]
    public async Task Ship_Returns404_WhenDriverDoesNotExist()
    {
        var order = await CreateOrderAsync();
        await AdvanceOrderToStatusAsync(order.Id, OrderStatus.Preparing);

        var response = await _adminClient.PatchAsJsonAsync($"/api/orders/{order.Id}/ship",
            new ShipOrderRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── DELIVER ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Deliver_ReturnsOk_WithAdminToken()
    {
        var order = await CreateOrderAsync();
        await AdvanceOrderToStatusAsync(order.Id, OrderStatus.Shipped);

        var response = await _adminClient.PatchAsync($"/api/orders/{order.Id}/deliver", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be(OrderStatus.Delivered);
    }

    // ─── CANCEL ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancel_ReturnsOk_WhenOrderIsPending()
    {
        var order = await CreateOrderAsync();

        var response = await _customerClient.PatchAsync($"/api/orders/{order.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_Returns401_WithoutToken()
    {
        var order = await CreateOrderAsync();

        var response = await _unauthClient.PatchAsync($"/api/orders/{order.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Returns204_WithAdminToken()
    {
        var order = await CreateOrderAsync();

        var response = await _adminClient.DeleteAsync($"/api/orders/{order.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
