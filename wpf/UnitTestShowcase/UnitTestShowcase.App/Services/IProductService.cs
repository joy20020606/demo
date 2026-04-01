using UnitTestShowcase.App.Models;

namespace UnitTestShowcase.App.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(string name, decimal price, int stock, int categoryId);
    Task<Product> UpdateProductAsync(int id, string name, decimal price, int stock);
    Task DeleteProductAsync(int id);
    Task<IEnumerable<Product>> SearchProductsAsync(string keyword);
}
