using FluentAssertions;
using Moq;
using UnitTestShowcase.App.Models;
using UnitTestShowcase.App.Services;
using Xunit;

namespace UnitTestShowcase.Tests.ServiceTests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _service = new ProductService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldDelegateToRepository()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "A", Price = 10m, Stock = 5, CategoryId = 1 }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = await _service.GetAllProductsAsync();

        result.Should().HaveCount(1);
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnCorrectProduct()
    {
        var product = new Product { Id = 3, Name = "Widget", Price = 5m, Stock = 10, CategoryId = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(product);

        var result = await _service.GetProductByIdAsync(3);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Widget");
    }

    [Fact]
    public async Task CreateProductAsync_ValidData_ShouldCallRepository()
    {
        var created = new Product { Id = 1, Name = "Item", Price = 9.99m, Stock = 5, CategoryId = 1 };
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync(created);

        var result = await _service.CreateProductAsync("Item", 9.99m, 5, 1);

        result.Should().NotBeNull();
        result.Name.Should().Be("Item");
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Theory]
    [InlineData("", 10, 5)]
    [InlineData("  ", 10, 5)]
    public async Task CreateProductAsync_EmptyName_ShouldThrowArgumentException(
        string name, decimal price, int stock)
    {
        await _service.Invoking(s => s.CreateProductAsync(name, price, stock, 1))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public async Task CreateProductAsync_NegativePrice_ShouldThrowArgumentException()
    {
        await _service.Invoking(s => s.CreateProductAsync("Item", -1m, 5, 1))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*price*");
    }

    [Fact]
    public async Task CreateProductAsync_NegativeStock_ShouldThrowArgumentException()
    {
        await _service.Invoking(s => s.CreateProductAsync("Item", 10m, -1, 1))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*stock*");
    }

    [Fact]
    public async Task CreateProductAsync_ShouldTrimName()
    {
        Product? captured = null;
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => captured = p)
            .ReturnsAsync((Product p) => p);

        await _service.CreateProductAsync("  Widget  ", 10m, 1, 1);

        captured!.Name.Should().Be("Widget");
    }

    [Fact]
    public async Task UpdateProductAsync_ProductNotFound_ShouldThrow()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        await _service.Invoking(s => s.UpdateProductAsync(99, "X", 1m, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*99*");
    }

    [Fact]
    public async Task UpdateProductAsync_ValidData_ShouldUpdateFields()
    {
        var existing = new Product { Id = 1, Name = "Old", Price = 5m, Stock = 1, CategoryId = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);

        var result = await _service.UpdateProductAsync(1, "New Name", 99m, 50);

        result.Name.Should().Be("New Name");
        result.Price.Should().Be(99m);
        result.Stock.Should().Be(50);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldCallRepository()
    {
        _mockRepo.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);

        await _service.DeleteProductAsync(5);

        _mockRepo.Verify(r => r.DeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task SearchProductsAsync_EmptyKeyword_ShouldReturnAll()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Alpha", Price = 1m, Stock = 1, CategoryId = 1 },
            new() { Id = 2, Name = "Beta", Price = 2m, Stock = 1, CategoryId = 1 }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = await _service.SearchProductsAsync("");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchProductsAsync_WithKeyword_ShouldFilterByName()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Widget Pro", Price = 1m, Stock = 1, CategoryId = 1 },
            new() { Id = 2, Name = "Gadget", Price = 2m, Stock = 1, CategoryId = 1 }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = await _service.SearchProductsAsync("widget");

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Widget Pro");
    }
}
