
using DeliveryApp.Application.DTOs.Customers;
using DeliveryApp.Application.Interfaces;

namespace DeliveryApp.Adapter.Endpoints;
public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers").RequireAuthorization("Admin");

        group.MapGet("/", async (ICustomerService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, ICustomerService service, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is { } customer
                ? Results.Ok(customer)
                : Results.NotFound());

        group.MapPost("/", async (CreateCustomerRequest request, ICustomerService service, CancellationToken ct) =>
        {
            var customer = await service.CreateAsync(request, ct);
            return Results.Created($"/api/customers/{customer.Id}", customer);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest request, ICustomerService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct)));

        group.MapDelete("/{id:guid}", async (Guid id, ICustomerService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        return app;
    }
}
