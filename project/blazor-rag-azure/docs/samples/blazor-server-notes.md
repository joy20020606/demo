# Blazor Server 開發須知(範例文件)

這份筆記整理團隊在 Blazor Server 專案上的共識與常見問題。

## 執行模型

Blazor Server 的 C# 程式碼在伺服器執行,瀏覽器只負責顯示與回報事件。每個使用者連線會透過 SignalR 建立一個 circuit,元件的狀態存放在伺服器記憶體中,直到連線關閉。因此伺服器記憶體會隨同時在線人數線性成長,估算容量時以每個 circuit 約 250 KB 到 1 MB 計算。

## Render mode

.NET 8 之後的 Blazor Web App 範本預設是靜態 SSR,元件不會有互動行為。要讓 `@onclick` 之類的事件生效,必須在 App.razor 的 `<Routes>` 與 `<HeadOutlet>` 加上 `@rendermode="InteractiveServer"`,或在個別頁面宣告。忘記設定時按鈕不會有任何反應,也不會出現錯誤訊息,這是新人最常卡住的地方。

## DbContext 的生命週期

在 Blazor Server 中,Scoped 服務的範圍是整個 circuit 而不是單一 HTTP 請求。直接在元件注入 `AppDbContext` 會讓同一個 DbContext 活到使用者關閉頁面,可能累積大量追蹤實體,也可能在兩個事件同時觸發時發生並行存取例外。團隊規範是注入 `IDbContextFactory<AppDbContext>`,每個操作用 `await using var db = await factory.CreateDbContextAsync()` 建立短命的 DbContext。

## 資料繫結

`@bind` 預設在 `onchange` 事件同步,也就是輸入框失去焦點時才更新欄位。若需要即時反應,加上 `@bind:event="oninput"`,但要注意每次按鍵都會經過 SignalR 往返伺服器。對於搜尋框這類需求,建議搭配 debounce 避免每個字元都查詢資料庫。

## 重新渲染時機

元件會在事件處理方法的 `await` 點與方法結束時自動重新渲染。若在背景執行緒或 Timer 回呼中修改狀態,必須手動呼叫 `StateHasChanged()`,並用 `InvokeAsync` 切回渲染同步內容,否則畫面不會更新。

## CSS 隔離

元件專屬樣式放在同名的 `.razor.css` 檔,建置時會自動加上範圍屬性,避免與其他元件的 class 名稱衝突。子元件的元素不會繼承父元件的範圍屬性,要穿透時使用 `::deep` 選擇器。全站共用的樣式才放在 `wwwroot/app.css`。

## 部署到 Azure App Service

Blazor Server 需要 WebSocket,部署到 App Service 時要在設定中開啟 Web sockets,否則會退回 long polling,互動延遲明顯變高。若要水平擴充到多個執行個體,必須開啟 ARR affinity 讓同一個使用者固定連到同一台,或改用 Azure SignalR Service。
