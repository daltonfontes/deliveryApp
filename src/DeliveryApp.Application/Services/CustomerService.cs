namespace DeliveryApp.Application.Services;

using DeliveryApp.Application.DTOs.Customers;
using DeliveryApp.Application.Interfaces;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;

public class CustomerService(ICustomerRepository customerRepository) : ICustomerService
{
    public async Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : MapToResponse(customer);
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(MapToResponse);
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = Customer.Create(request.Name, request.Email, request.Phone, request.Address);

        await customerRepository.AddAsync(customer, cancellationToken);
        await customerRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        customer.Update(request.Name, request.Email, request.Phone, request.Address);

        await customerRepository.UpdateAsync(customer, cancellationToken);
        await customerRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        await customerRepository.DeleteAsync(customer, cancellationToken);
        await customerRepository.SaveChangesAsync(cancellationToken);
    }

    private static CustomerResponse MapToResponse(Customer c) =>
        new(c.Id, c.Name, c.Email, c.Phone, c.Address, c.CreatedAt, c.UpdatedAt);
}
