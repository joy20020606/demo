using CommunityToolkit.Mvvm.ComponentModel;
using UnitTestShowcase.App.Models;

namespace UnitTestShowcase.App.ViewModels;

public partial class ProductViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private decimal _price;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private int _stock;

    [ObservableProperty]
    private int _categoryId;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Price >= 0 && Stock >= 0;

    public ProductViewModel() { }

    public ProductViewModel(Product product)
    {
        _id = product.Id;
        _name = product.Name;
        _price = product.Price;
        _stock = product.Stock;
        _categoryId = product.CategoryId;
        _categoryName = product.Category?.Name ?? string.Empty;
    }

    public Product ToModel() => new()
    {
        Id = Id,
        Name = Name,
        Price = Price,
        Stock = Stock,
        CategoryId = CategoryId
    };
}
