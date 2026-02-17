
using DeliveryApp.Application.DTOs.Orders;
using DeliveryApp.Application.Interfaces;

namespace DeliveryApp.Adapter.Endpoints;
public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", async (IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct))).RequireAuthorization("Admin");

        group.MapGet("/{id:guid}", async (Guid id, IOrderService service, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is { } order
                ? Results.Ok(order)
                : Results.NotFound()).RequireAuthorization("OrderOwnerOrAdmin");

        group.MapGet("/customer/{customerId:guid}", async (Guid customerId, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.GetByCustomerIdAsync(customerId, ct))).RequireAuthorization("OrderOwnerOrAdmin");

        group.MapPost("/", async (CreateOrderRequest request, IOrderService service, CancellationToken ct) =>
        {
            var order = await service.CreateAsync(request, ct);
            return Results.Created($"/api/orders/{order.Id}", order);
        }).RequireAuthorization("Customer");

        group.MapPatch("/{id:guid}/pay", async (Guid id, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.PayOrderAsync(id, ct))).RequireAuthorization("Customer");

        group.MapPatch("/{id:guid}/prepare", async (Guid id, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.PrepareOrderAsync(id, ct))).RequireAuthorization("Admin");

        group.MapPatch("/{id:guid}/ship", async (Guid id, ShipOrderRequest request, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.ShipOrderAsync(id, request, ct))).RequireAuthorization("Admin");

        group.MapPatch("/{id:guid}/deliver", async (Guid id, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.DeliverOrderAsync(id, ct))).RequireAuthorization("Admin");

        group.MapPatch("/{id:guid}/cancel", async (Guid id, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.CancelOrderAsync(id, ct))).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IOrderService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization("Admin");

        return app;
    }
}
