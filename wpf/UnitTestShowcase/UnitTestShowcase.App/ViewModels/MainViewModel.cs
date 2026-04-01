using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UnitTestShowcase.App.Services;

namespace UnitTestShowcase.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProductService _productService;

    [ObservableProperty]
    private ObservableCollection<ProductViewModel> _products = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteProductCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateProductCommand))]
    private ProductViewModel? _selectedProduct;

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private string _newProductName = string.Empty;

    [ObservableProperty]
    private decimal _newProductPrice;

    [ObservableProperty]
    private int _newProductStock;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isLoading;

    public MainViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading...";
        try
        {
            var products = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<ProductViewModel>(
                products.Select(p => new ProductViewModel(p)));
            StatusMessage = $"Loaded {Products.Count} products.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        IsLoading = true;
        StatusMessage = "Searching...";
        try
        {
            var products = await _productService.SearchProductsAsync(SearchKeyword);
            Products = new ObservableCollection<ProductViewModel>(
                products.Select(p => new ProductViewModel(p)));
            StatusMessage = $"Found {Products.Count} products.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddProductAsync()
    {
        try
        {
            var product = await _productService.CreateProductAsync(
                NewProductName, NewProductPrice, NewProductStock, 1);
            Products.Add(new ProductViewModel(product));
            NewProductName = string.Empty;
            NewProductPrice = 0;
            NewProductStock = 0;
            StatusMessage = $"Product '{product.Name}' added.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrUpdate))]
    public async Task DeleteProductAsync()
    {
        if (SelectedProduct is null) return;
        try
        {
            await _productService.DeleteProductAsync(SelectedProduct.Id);
            var removed = SelectedProduct;
            Products.Remove(removed);
            SelectedProduct = null;
            StatusMessage = $"Product '{removed.Name}' deleted.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOrUpdate))]
    public async Task UpdateProductAsync()
    {
        if (SelectedProduct is null) return;
        try
        {
            var updated = await _productService.UpdateProductAsync(
                SelectedProduct.Id, SelectedProduct.Name,
                SelectedProduct.Price, SelectedProduct.Stock);
            StatusMessage = $"Product '{updated.Name}' updated.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private bool CanDeleteOrUpdate() => SelectedProduct is not null;
}
