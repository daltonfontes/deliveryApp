using DeliveryApp.Application.DTOs.Orders;
using DeliveryApp.Application.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DeliveryApp.Application.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IDeliveryDriverRepository> _driverRepoMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _orderRepoMock.Object,
            _customerRepoMock.Object,
            _productRepoMock.Object,
            _driverRepoMock.Object);
    }

    private static Customer CreateCustomer() => new()
    {
        Id = Guid.NewGuid(), Name = "João", Email = "joao@test.com",
        Phone = "11999999999", Address = "Rua A", CreatedAt = DateTime.UtcNow
    };

    private static Product CreateProduct(decimal price = 25.00m) => new()
    {
        Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza test",
        Price = price, IsActive = true, CategoryId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow
    };

    private static DeliveryDriver CreateDriver() => new()
    {
        Id = Guid.NewGuid(), Name = "Carlos", Phone = "11888888888",
        VehicleType = VehicleType.Motorcycle, IsAvailable = true, CreatedAt = DateTime.UtcNow
    };

    private static Order CreateOrder(Guid? customerId = null)
        => Order.Create(
            customerId ?? Guid.NewGuid(),
            "Rua A, 123",
            new List<OrderItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 25.00m }
            });

    private static Order CreateOrderInStatus(OrderStatus status, Guid? customerId = null)
    {
        var order = CreateOrder(customerId);
        if (status >= OrderStatus.Confirmed) order.Pay();
        if (status >= OrderStatus.Preparing) order.Prepare();
        if (status >= OrderStatus.Shipped)   order.Ship(Guid.NewGuid());
        return order;
    }

    // ─── GET ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsResponse_WhenOrderExists()
    {
        var order = CreateOrder();
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.GetByIdAsync(order.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrders()
    {
        var orders = new[] { CreateOrder(), CreateOrder() };
        _orderRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(orders);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ReturnsOrdersForCustomer()
    {
        var customerId = Guid.NewGuid();
        var orders = new[] { CreateOrder(customerId) };
        _orderRepoMock.Setup(r => r.GetOrdersByCustomerIdAsync(customerId, default)).ReturnsAsync(orders);

        var result = await _sut.GetByCustomerIdAsync(customerId);

        result.Should().HaveCount(1);
    }

    // ─── CREATE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CreatesOrder_WithValidData()
    {
        var customer = CreateCustomer();
        var product = CreateProduct(30.00m);

        _customerRepoMock.Setup(r => r.GetByIdAsync(customer.Id, default)).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetIdsAsync(It.IsAny<IEnumerable<Guid>>(), default))
            .ReturnsAsync(new List<Product> { product });

        var createdOrder = CreateOrder(customer.Id);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync(createdOrder);

        var request = new CreateOrderRequest(customer.Id, "Rua B, 456", new List<OrderItemRequest> { new(product.Id, 2) });

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>(), default), Times.Once);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundException_WhenCustomerDoesNotExist()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Customer?)null);

        var request = new CreateOrderRequest(Guid.NewGuid(), "Rua X", new List<OrderItemRequest> { new(Guid.NewGuid(), 1) });

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Customer*");
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
    {
        var customer = CreateCustomer();
        _customerRepoMock.Setup(r => r.GetByIdAsync(customer.Id, default)).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetIdsAsync(It.IsAny<IEnumerable<Guid>>(), default))
            .ReturnsAsync(new List<Product>());

        var request = new CreateOrderRequest(customer.Id, "Rua X", new List<OrderItemRequest> { new(Guid.NewGuid(), 1) });

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Product*");
    }

    [Fact]
    public async Task CreateAsync_CalculatesTotalAmountCorrectly()
    {
        var customer = CreateCustomer();
        var product1 = CreateProduct(10.00m);
        var product2 = CreateProduct(20.00m);

        _customerRepoMock.Setup(r => r.GetByIdAsync(customer.Id, default)).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetIdsAsync(It.IsAny<IEnumerable<Guid>>(), default))
            .ReturnsAsync(new List<Product> { product1, product2 });

        Order? capturedOrder = null;
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>(), default))
            .Callback<Order, CancellationToken>((o, _) => capturedOrder = o)
            .ReturnsAsync((Order o, CancellationToken _) => o);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(() => capturedOrder ?? CreateOrder());

        var request = new CreateOrderRequest(customer.Id, "Rua X", new List<OrderItemRequest>
        {
            new(product1.Id, 3),  // 3 * 10 = 30
            new(product2.Id, 2)   // 2 * 20 = 40
        });

        await _sut.CreateAsync(request);

        capturedOrder.Should().NotBeNull();
        capturedOrder!.TotalAmount.Should().Be(70.00m);
    }

    // ─── PAY ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PayOrderAsync_ConfirmsOrder_WhenOrderExists()
    {
        var order = CreateOrder();
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.PayOrderAsync(order.Id);

        result.Status.Should().Be(OrderStatus.Confirmed);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task PayOrderAsync_ThrowsNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);

        var act = () => _sut.PayOrderAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── PREPARE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task PrepareOrderAsync_PreparesOrder_WhenOrderIsConfirmed()
    {
        var order = CreateOrderInStatus(OrderStatus.Confirmed);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.PrepareOrderAsync(order.Id);

        result.Status.Should().Be(OrderStatus.Preparing);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task PrepareOrderAsync_ThrowsNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);

        var act = () => _sut.PrepareOrderAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── SHIP ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ShipOrderAsync_ShipsOrder_WhenBothExist()
    {
        var order = CreateOrderInStatus(OrderStatus.Preparing);
        var driver = CreateDriver();
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);
        _driverRepoMock.Setup(r => r.GetByIdAsync(driver.Id, default)).ReturnsAsync(driver);

        var result = await _sut.ShipOrderAsync(order.Id, new ShipOrderRequest(driver.Id));

        result.Status.Should().Be(OrderStatus.Shipped);
        result.DeliveryDriverId.Should().Be(driver.Id);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ShipOrderAsync_ThrowsNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);

        var act = () => _sut.ShipOrderAsync(Guid.NewGuid(), new ShipOrderRequest(Guid.NewGuid()));

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Order*");
    }

    [Fact]
    public async Task ShipOrderAsync_ThrowsNotFoundException_WhenDriverDoesNotExist()
    {
        var order = CreateOrderInStatus(OrderStatus.Preparing);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);
        _driverRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((DeliveryDriver?)null);

        var act = () => _sut.ShipOrderAsync(order.Id, new ShipOrderRequest(Guid.NewGuid()));

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*DeliveryDriver*");
    }

    // ─── DELIVER ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeliverOrderAsync_DeliversOrder_WhenOrderIsShipped()
    {
        var order = CreateOrderInStatus(OrderStatus.Shipped);
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.DeliverOrderAsync(order.Id);

        result.Status.Should().Be(OrderStatus.Delivered);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeliverOrderAsync_ThrowsNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);

        var act = () => _sut.DeliverOrderAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── CANCEL ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelOrderAsync_CancelsOrder_WhenOrderIsPending()
    {
        var order = CreateOrder();
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.CancelOrderAsync(order.Id);

        result.Status.Should().Be(OrderStatus.Cancelled);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ThrowsNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);

        var act = () => _sut.CancelOrderAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenOrderDoesNotExist()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
