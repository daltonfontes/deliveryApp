namespace DeliveryApp.Application.Services;

using DeliveryApp.Application.DTOs.Products;
using DeliveryApp.Application.Interfaces;
using DeliveryApp.Application.Mappers;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;

public class ProductService(IProductRepository productRepository) : IProductService
{
    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ProductMapper.MapToResponse(product);
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return products.Select(ProductMapper.MapToResponse);
    }

    public async Task<IEnumerable<ProductResponse>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetActiveProductsAsync(cancellationToken);
        return products.Select(ProductMapper.MapToResponse);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(request.Name, request.Description, request.Price, request.ImageUrl, request.CategoryId);

        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        return ProductMapper.MapToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product", id);

        product.Update(request.Name, request.Description, request.Price, request.ImageUrl, request.CategoryId);

        if (request.IsActive && !product.IsActive)
            product.Activate();
        else if (!request.IsActive && product.IsActive)
            product.Deactivate();

        await productRepository.UpdateAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        return ProductMapper.MapToResponse(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product", id);

        await productRepository.DeleteAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
    }
}
