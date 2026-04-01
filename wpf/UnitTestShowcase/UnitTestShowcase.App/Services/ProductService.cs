using UnitTestShowcase.App.Models;

namespace UnitTestShowcase.App.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(string name, decimal price, int stock, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative.", nameof(stock));

        var product = new Product
        {
            Name = name.Trim(),
            Price = price,
            Stock = stock,
            CategoryId = categoryId
        };
        return await _repository.AddAsync(product);
    }

    public async Task<Product> UpdateProductAsync(int id, string name, decimal price, int stock)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Product with id {id} not found.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative.", nameof(stock));

        existing.Name = name.Trim();
        existing.Price = price;
        existing.Stock = stock;
        return await _repository.UpdateAsync(existing);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword)
    {
        var all = await _repository.GetAllAsync();
        if (string.IsNullOrWhiteSpace(keyword))
            return all;
        return all.Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
