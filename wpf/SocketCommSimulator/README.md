# SocketCommSimulator — 設備通訊模擬器

模擬設備間的 Socket TCP/IP 通訊，包含 Server/Client 雙端與訊息協議解析。

## 功能

- **TCP Server 模式**：監聽指定 Port，支援多 Client 同時連線，廣播訊息
- **TCP Client 模式**：連線至 Server，支援斷線自動重連
- **自訂訊息協議**：`Header(AA 55) + Length(2B) + Payload(nB) + Checksum(XOR)`
- **非同步 I/O**：全程 async/await + CancellationToken
- **訊息日誌**：即時顯示，支援 ASCII / HEX 格式切換
- **Heartbeat 監控**：定時 PING/PONG，超時自動重連

## 架構

```
MVVM
├── Models/
│   ├── ProtocolMessage   — 訊息封裝 / 解析
│   ├── ConnectionInfo    — 連線狀態 (ObservableObject)
│   └── LogEntry          — 日誌條目 (時間、方向、內容)
├── Services/
│   ├── TcpServerService  — 非同步 TCP Server，多 Client 管理
│   ├── TcpClientService  — 非同步 TCP Client，自動重連
│   ├── ProtocolService   — 協議封裝 / 串流解析
│   └── HeartbeatService  — PING/PONG 心跳監控
├── ViewModels/
│   ├── MainViewModel     — 根 ViewModel
│   ├── ServerViewModel   — Server 邏輯 + 命令
│   ├── ClientViewModel   — Client 邏輯 + 命令
│   └── LogViewModel      — 日誌集合管理
└── Views/
    ├── MainWindow        — TabControl (Server / Client)
    ├── ServerPanel       — Server UI (UserControl)
    ├── ClientPanel       — Client UI (UserControl)
    └── LogPanel          — 日誌顯示 (UserControl)
```

## 訊息協議格式

| 欄位     | 長度  | 說明                    |
|----------|-------|-------------------------|
| Header   | 2 B   | 固定 `0xAA 0x55`        |
| Length   | 2 B   | Payload 長度 (Big-Endian)|
| Payload  | N B   | 實際資料 (UTF-8 文字)    |
| Checksum | 1 B   | Payload 所有 Byte XOR   |

## 設計模式

- **MVVM** — CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`)
- **Observer Pattern** — `event Action<>` 解耦 Service 與 ViewModel
- **Adapter Pattern** — `ProtocolService` 作為串流 ↔ 訊息的轉接層

## Build & Run

### 需求

- .NET 8 SDK
- Windows（WPF 僅支援 Windows）

### 建置

```bash
cd WPF/SocketCommSimulator
dotnet build
```

### 執行

```bash
dotnet run --project SocketCommSimulator/SocketCommSimulator.csproj
```

### 使用方式

1. **Server 模式**：切換到「TCP Server」頁籤 → 設定 Port → 點「啟動」
2. **Client 模式**：切換到「TCP Client」頁籤 → 設定 IP/Port → 點「連線」
3. 在輸入框輸入訊息後按 Enter 或按鈕即可傳送
4. 日誌面板即時顯示收發訊息，點「切換格式」可在 ASCII / HEX 間切換
