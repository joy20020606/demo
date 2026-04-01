# UnitTestShowcase

WPF 專案第十個範例，展示完整的單元測試與整合測試架構。

## 專案說明

以簡單的產品管理應用為基礎，示範 MVVM 架構下的測試策略：

- **ViewModel 單元測試**：驗證屬性通知、命令邏輯
- **Service 層測試**：使用 Moq 模擬外部相依
- **Repository 整合測試**：SQLite In-Memory 資料庫

## 專案架構

```
UnitTestShowcase/
├── UnitTestShowcase.sln
├── .github/workflows/ci.yml        # GitHub Actions CI/CD
├── UnitTestShowcase.App/           # WPF 主程式
│   ├── Models/
│   │   ├── Product.cs
│   │   └── Category.cs
│   ├── ViewModels/
│   │   ├── MainViewModel.cs        # 主頁面 ViewModel (CRUD 操作)
│   │   └── ProductViewModel.cs     # 產品 ViewModel (屬性通知)
│   ├── Views/
│   │   └── MainWindow.xaml         # 主視窗
│   └── Services/
│       ├── IProductRepository.cs   # Repository 介面
│       ├── ProductRepository.cs    # SQLite 實作
│       ├── IProductService.cs      # Service 介面
│       └── ProductService.cs       # 業務邏輯
└── UnitTestShowcase.Tests/         # 測試專案
    ├── ViewModelTests/
    │   ├── ProductViewModelTests.cs # ViewModel 屬性與通知測試
    │   └── MainViewModelTests.cs    # 命令邏輯與狀態測試
    ├── ServiceTests/
    │   └── ProductServiceTests.cs   # Service 層 Moq 測試
    └── IntegrationTests/
        └── ProductRepositoryTests.cs # SQLite In-Memory 整合測試
```

## 測試架構說明

### ViewModel 單元測試 (xUnit + FluentAssertions)

驗證：
- `ObservableProperty` 屬性變更通知
- `RelayCommand` 可執行條件 (`CanExecute`)
- 複雜屬性依賴（如 `IsValid`）

### Service 層測試 (xUnit + Moq)

驗證：
- Repository 呼叫次數與參數
- 業務邏輯驗證（名稱不可空、價格不可負）
- 例外拋出條件

### 整合測試 (xUnit + SQLite In-Memory)

驗證：
- CRUD 操作的完整資料流
- 資料庫 JOIN 查詢正確性
- 不存在記錄時的回傳值

## 技術棧

| 類別 | 套件 |
|------|------|
| WPF Framework | .NET 8.0-windows |
| MVVM | CommunityToolkit.Mvvm 8.3.2 |
| DI | Microsoft.Extensions.DependencyInjection |
| 資料庫 | Microsoft.Data.Sqlite |
| 測試框架 | xUnit 2.7.0 |
| Mock | Moq 4.20.70 |
| 斷言 | FluentAssertions 6.12.0 |
| 覆蓋率 | Coverlet 6.0.2 |

## Build & Run

### 建置

```bash
dotnet build UnitTestShowcase.sln
```

### 執行主程式

```bash
dotnet run --project UnitTestShowcase.App/UnitTestShowcase.App.csproj
```

### 執行測試

```bash
dotnet test UnitTestShowcase.Tests/UnitTestShowcase.Tests.csproj
```

### 產生覆蓋率報告

```bash
dotnet test UnitTestShowcase.Tests/UnitTestShowcase.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage/

# 安裝 reportgenerator 工具
dotnet tool install -g dotnet-reportgenerator-globaltool

# 產生 HTML 報告
reportgenerator -reports:./coverage/coverage.opencover.xml -targetdir:./coverage/html -reporttypes:Html

# 開啟報告
start ./coverage/html/index.html
```

## CI/CD

使用 GitHub Actions（`.github/workflows/ci.yml`），於每次 push/PR 時：

1. Restore 相依套件
2. Build Release 版本
3. 執行測試並收集覆蓋率
4. 上傳覆蓋率報告 Artifact
