using FluentAssertions;
using Moq;
using UnitTestShowcase.App.Models;
using UnitTestShowcase.App.Services;
using UnitTestShowcase.App.ViewModels;
using Xunit;

namespace UnitTestShowcase.Tests.ViewModelTests;

public class MainViewModelTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly MainViewModel _vm;

    public MainViewModelTests()
    {
        _mockService = new Mock<IProductService>();
        _vm = new MainViewModel(_mockService.Object);
    }

    [Fact]
    public void InitialState_ShouldHaveEmptyProducts()
    {
        _vm.Products.Should().BeEmpty();
        _vm.SelectedProduct.Should().BeNull();
        _vm.StatusMessage.Should().Be("Ready");
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadProductsAsync_ShouldPopulateProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "A", Price = 10m, Stock = 5, CategoryId = 1 },
            new() { Id = 2, Name = "B", Price = 20m, Stock = 3, CategoryId = 1 }
        };
        _mockService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(products);

        await _vm.LoadProductsAsync();

        _vm.Products.Should().HaveCount(2);
        _vm.StatusMessage.Should().Contain("2");
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadProductsAsync_WhenServiceThrows_ShouldSetErrorStatus()
    {
        _mockService.Setup(s => s.GetAllProductsAsync())
            .ThrowsAsync(new Exception("DB error"));

        await _vm.LoadProductsAsync();

        _vm.StatusMessage.Should().Contain("Error");
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Widget", Price = 5m, Stock = 10, CategoryId = 1 }
        };
        _mockService.Setup(s => s.SearchProductsAsync("Widget")).ReturnsAsync(products);

        _vm.SearchKeyword = "Widget";
        await _vm.SearchAsync();

        _vm.Products.Should().HaveCount(1);
        _vm.Products[0].Name.Should().Be("Widget");
    }

    [Fact]
    public async Task AddProductAsync_ShouldAddToCollection()
    {
        var newProduct = new Product { Id = 10, Name = "New Item", Price = 15m, Stock = 2, CategoryId = 1 };
        _mockService.Setup(s => s.CreateProductAsync("New Item", 15m, 2, 1))
            .ReturnsAsync(newProduct);

        _vm.NewProductName = "New Item";
        _vm.NewProductPrice = 15m;
        _vm.NewProductStock = 2;
        await _vm.AddProductAsync();

        _vm.Products.Should().HaveCount(1);
        _vm.Products[0].Name.Should().Be("New Item");
        _vm.NewProductName.Should().BeEmpty();
        _vm.StatusMessage.Should().Contain("New Item");
    }

    [Fact]
    public async Task DeleteProductAsync_WithNullSelected_ShouldDoNothing()
    {
        _vm.SelectedProduct = null;
        await _vm.DeleteProductAsync();

        _mockService.Verify(s => s.DeleteProductAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldRemoveFromCollection()
    {
        var product = new Product { Id = 1, Name = "ToDelete", Price = 1m, Stock = 1, CategoryId = 1 };
        _mockService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync([product]);
        _mockService.Setup(s => s.DeleteProductAsync(1)).Returns(Task.CompletedTask);

        await _vm.LoadProductsAsync();
        _vm.SelectedProduct = _vm.Products[0];
        await _vm.DeleteProductAsync();

        _vm.Products.Should().BeEmpty();
        _vm.SelectedProduct.Should().BeNull();
    }

    [Fact]
    public void SelectedProduct_Change_ShouldRaisePropertyChanged()
    {
        var changedProps = new List<string?>();
        _vm.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName);

        _vm.SelectedProduct = new ProductViewModel { Name = "X", Price = 1m, Stock = 1 };

        changedProps.Should().Contain(nameof(MainViewModel.SelectedProduct));
    }
}
