using FluentAssertions;
using Microsoft.Data.Sqlite;
using UnitTestShowcase.App.Models;
using UnitTestShowcase.App.Services;
using Xunit;

namespace UnitTestShowcase.Tests.IntegrationTests;

public class ProductRepositoryTests : IDisposable
{
    private readonly SqliteConnection _keepAliveConnection;
    private readonly ProductRepository _repository;
    private readonly string _dbName;

    public ProductRepositoryTests()
    {
        _dbName = $"testdb_{Guid.NewGuid():N}";
        var connectionString = $"Data Source={_dbName};Mode=Memory;Cache=Shared";

        _keepAliveConnection = new SqliteConnection(connectionString);
        _keepAliveConnection.Open();

        _repository = new ProductRepository(connectionString);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProduct()
    {
        var product = new Product { Name = "Test", Price = 9.99m, Stock = 10, CategoryId = 1 };

        var result = await _repository.AddAsync(product);

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPersistedProducts()
    {
        await _repository.AddAsync(new Product { Name = "A", Price = 1m, Stock = 1, CategoryId = 1 });
        await _repository.AddAsync(new Product { Name = "B", Price = 2m, Stock = 2, CategoryId = 1 });

        var products = (await _repository.GetAllAsync()).ToList();

        products.Should().HaveCountGreaterThanOrEqualTo(2);
        products.Should().Contain(p => p.Name == "A");
        products.Should().Contain(p => p.Name == "B");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectProduct()
    {
        var added = await _repository.AddAsync(new Product { Name = "FindMe", Price = 5m, Stock = 3, CategoryId = 1 });

        var result = await _repository.GetByIdAsync(added.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("FindMe");
        result.Price.Should().Be(5m);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(99999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyProduct()
    {
        var added = await _repository.AddAsync(new Product { Name = "Original", Price = 10m, Stock = 5, CategoryId = 1 });
        added.Name = "Updated";
        added.Price = 99m;

        var updated = await _repository.UpdateAsync(added);

        updated.Name.Should().Be("Updated");
        var fetched = await _repository.GetByIdAsync(added.Id);
        fetched!.Name.Should().Be("Updated");
        fetched.Price.Should().Be(99m);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct()
    {
        var added = await _repository.AddAsync(new Product { Name = "ToDelete", Price = 1m, Stock = 1, CategoryId = 1 });

        await _repository.DeleteAsync(added.Id);

        var result = await _repository.GetByIdAsync(added.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldIncludeCategory()
    {
        var added = await _repository.AddAsync(new Product { Name = "WithCat", Price = 5m, Stock = 1, CategoryId = 1 });

        var fetched = await _repository.GetByIdAsync(added.Id);

        fetched!.Category.Should().NotBeNull();
        fetched.Category!.Name.Should().Be("Electronics");
    }

    public void Dispose()
    {
        _keepAliveConnection.Dispose();
    }
}
