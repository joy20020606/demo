# RecipeEditor — 半導體製程配方編輯器

模擬半導體設備的製程配方（Recipe）編輯工具，支援步驟式流程編輯、參數驗證與 Undo/Redo。

## 功能

| 功能 | 說明 |
|------|------|
| 步驟管理 | 新增 6 種步驟類型（Deposition / Etch / Clean / Anneal / Transfer / Wait） |
| 拖放排序 | 在步驟列表中拖放即可重新排序；亦可使用 ▲ / ▼ 按鈕 |
| 參數編輯 | 每種步驟預設對應參數（溫度、壓力、氣體流量、功率等），即時顯示範圍驗證狀態 |
| Undo / Redo | 完整 Memento Pattern 實作，所有結構操作（新增、刪除、移動、複製步驟）均可復原 |
| 配方驗證 | 點擊 Validate 執行驗證引擎，輸出 Error / Warning / Info 三級結果 |
| JSON 匯入/匯出 | Open / Save 以 System.Text.Json 讀寫標準 JSON 格式 |

## 技術架構

```
架構模式：MVVM（CommunityToolkit.Mvvm 8.x）
設計模式：Command Pattern（RelayCommand）、Memento Pattern（UndoRedoManager）
序列化：System.Text.Json + JsonStringEnumConverter
```

### 專案結構

```
RecipeEditor/
├── Models/
│   ├── Recipe.cs           配方：名稱、版本、步驟列表（POCO，直接可序列化）
│   ├── RecipeStep.cs       步驟：類型 Enum、參數列表
│   └── StepParameter.cs    參數：名稱、值、單位、最小值、最大值
├── ViewModels/
│   ├── UndoRedoManager.cs  Memento Pattern：維護 undo/redo Stack<string>（JSON 快照）
│   ├── ParameterViewModel.cs
│   ├── StepViewModel.cs
│   ├── RecipeViewModel.cs  結構操作命令、ToJson/FromJson（Memento 快照點）
│   ├── MainViewModel.cs    Undo/Redo/Validate/Open/Save 命令
│   └── StepTypeHelper.cs   靜態輔助：提供 ComboBox 綁定的 StepType[] 陣列
├── Views/
│   ├── MainWindow.xaml/.cs
│   ├── StepEditor.xaml/.cs        含拖放邏輯的步驟列表 UserControl
│   ├── ParameterEditor.xaml/.cs   步驟屬性與參數編輯 UserControl
│   └── ValidationPanel.xaml/.cs   驗證結果展示 UserControl
├── Services/
│   ├── RecipeService.cs    JSON 非同步讀寫
│   └── ValidationService.cs 驗證引擎：範圍檢查、步驟相依性、重複名稱
└── Converters/
    ├── BoolToVisibilityConverter.cs
    └── NullToVisibilityConverter.cs
```

### Memento Pattern 實作說明

```
UndoRedoManager
  _undoStack: Stack<string>   (JSON snapshots)
  _redoStack: Stack<string>

每次結構操作前（AddStep / RemoveStep / MoveStep / DuplicateStep）：
  1. RecipeViewModel.SaveMemento() → 將目前 Recipe 序列化為 JSON → 推入 undoStack
  2. 執行操作

Undo：
  1. current = Recipe.ToJson()
  2. previous = undoStack.Pop()，current → redoStack.Push
  3. Recipe.FromJson(previous) → 重建所有 ViewModel

Redo：
  1. current = Recipe.ToJson()
  2. next = redoStack.Pop()，current → undoStack.Push
  3. Recipe.FromJson(next)
```

## Build & Run

**需求**：.NET 8 SDK、Windows（WPF）

```bash
cd WPF/RecipeEditor
dotnet build RecipeEditor.sln
dotnet run --project RecipeEditor/RecipeEditor.csproj
```

## 配方 JSON 格式範例

```json
{
  "name": "SiO2 Deposition",
  "version": "1.0.0",
  "author": "Process Engineer",
  "description": "PECVD silicon dioxide deposition",
  "steps": [
    {
      "name": "Clean Step",
      "stepType": "Clean",
      "durationSeconds": 120,
      "parameters": [
        { "name": "Temperature", "value": 80, "unit": "°C", "minValue": 20, "maxValue": 150 }
      ]
    },
    {
      "name": "Deposition Step",
      "stepType": "Deposition",
      "durationSeconds": 300,
      "parameters": [
        { "name": "Temperature", "value": 350, "unit": "°C",    "minValue": 200, "maxValue": 600  },
        { "name": "Pressure",    "value": 0.5, "unit": "mTorr", "minValue": 0.1, "maxValue": 10   },
        { "name": "SiH4 Flow",   "value": 100, "unit": "sccm",  "minValue": 0,   "maxValue": 500  },
        { "name": "N2O Flow",    "value": 200, "unit": "sccm",  "minValue": 0,   "maxValue": 1000 }
      ]
    }
  ]
}
```
