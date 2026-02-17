using System.Security.Claims;
using DeliveryApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryApp.Adapter.Authorization;

public class OrderOwnerRequirement : IAuthorizationRequirement;

public class OrderOwnerAuthorizationHandler
    : AuthorizationHandler<OrderOwnerRequirement, Order>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrderOwnerRequirement requirement,
        Order order)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is not null && order.Customer?.UserId == userId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
