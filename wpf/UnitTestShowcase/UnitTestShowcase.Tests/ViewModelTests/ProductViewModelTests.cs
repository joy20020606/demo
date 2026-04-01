using FluentAssertions;
using UnitTestShowcase.App.Models;
using UnitTestShowcase.App.ViewModels;
using Xunit;

namespace UnitTestShowcase.Tests.ViewModelTests;

public class ProductViewModelTests
{
    [Fact]
    public void DefaultProductViewModel_ShouldBeInvalid()
    {
        var vm = new ProductViewModel();
        vm.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ProductViewModelWithName_ShouldBeValid()
    {
        var vm = new ProductViewModel();
        vm.Name = "Test Product";
        vm.Price = 10m;
        vm.Stock = 5;

        vm.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ProductViewModel_ConstructedFromModel_ShouldMapCorrectly()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Widget",
            Price = 9.99m,
            Stock = 100,
            CategoryId = 2,
            Category = new Category { Id = 2, Name = "Electronics" }
        };

        var vm = new ProductViewModel(product);

        vm.Id.Should().Be(1);
        vm.Name.Should().Be("Widget");
        vm.Price.Should().Be(9.99m);
        vm.Stock.Should().Be(100);
        vm.CategoryId.Should().Be(2);
        vm.CategoryName.Should().Be("Electronics");
    }

    [Fact]
    public void ToModel_ShouldReturnCorrectProduct()
    {
        var vm = new ProductViewModel
        {
            Id = 5,
            Name = "Gadget",
            Price = 49.99m,
            Stock = 20,
            CategoryId = 1
        };

        var model = vm.ToModel();

        model.Id.Should().Be(5);
        model.Name.Should().Be("Gadget");
        model.Price.Should().Be(49.99m);
        model.Stock.Should().Be(20);
        model.CategoryId.Should().Be(1);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("Valid Name", true)]
    public void IsValid_DependsOnName(string name, bool expected)
    {
        var vm = new ProductViewModel { Name = name, Price = 10m, Stock = 0 };
        vm.IsValid.Should().Be(expected);
    }

    [Fact]
    public void PropertyChanged_ShouldFireWhenNameChanges()
    {
        var vm = new ProductViewModel();
        var changedProperties = new List<string?>();
        vm.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        vm.Name = "New Name";

        changedProperties.Should().Contain(nameof(ProductViewModel.Name));
        changedProperties.Should().Contain(nameof(ProductViewModel.IsValid));
    }

    [Fact]
    public void PropertyChanged_ShouldFireWhenPriceChanges()
    {
        var vm = new ProductViewModel();
        var changedProperties = new List<string?>();
        vm.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        vm.Price = 99.99m;

        changedProperties.Should().Contain(nameof(ProductViewModel.Price));
        changedProperties.Should().Contain(nameof(ProductViewModel.IsValid));
    }
}
