# 內部部署 SOP(範例文件)

本文件描述 Northwind Logistics 內部系統的標準部署流程,適用於所有 .NET 服務。

## 1. 環境分層

我們有三個環境:Development、Staging、Production。Development 由工程師本機或 Docker 跑;Staging 部署在 Azure App Service 的 `nw-api-staging` 上;Production 在 `nw-api-prod`。Staging 與 Production 使用不同的 Azure SQL 資料庫,連線字串一律存放在 Azure Key Vault,不允許寫在 appsettings.json 或環境變數中。

## 2. 發布流程

所有變更必須經過 Pull Request,至少一位 reviewer 核准後才能合併到 `main`。合併到 `main` 會自動觸發 GitHub Actions:先執行 `dotnet test`,測試全部通過後執行 `dotnet publish`,再部署到 Staging。部署到 Production 需要在 GitHub Actions 的 environment 上按下人工核准按鈕,核准人必須是 Tech Lead 或 Release Manager。

## 3. 資料庫變更

資料庫使用 EF Core Code First。任何 schema 變更必須附帶 migration 檔案並在 PR 中一併提交。部署管線會在服務啟動前執行 `dotnet ef database update`。禁止直接在 Production 資料庫手動改 schema;如遇緊急修復,必須事後補 migration 並在 Confluence 記錄事件。

## 4. 回滾

如果 Production 部署後 15 分鐘內 Application Insights 的失敗率超過 2%,值班工程師應立即回滾。回滾方式是在 App Service 的 Deployment Center 選擇上一個成功的部署並 Redeploy。資料庫 migration 若已套用,回滾程式碼前要先確認新舊版本的 schema 相容;不相容時改為前滾修復(roll forward)。

## 5. 密鑰管理

所有 API key、連線字串、憑證統一放在 Key Vault。App Service 透過系統指派的 Managed Identity 讀取,不使用 client secret。新增密鑰時需在 Key Vault 設定至少 90 天的到期提醒。本機開發使用 .NET User Secrets,不得將任何密鑰提交到 git。

## 6. 監控與值班

每個服務都接 Application Insights,並設定三個警示:HTTP 5xx 比率超過 2%、P95 延遲超過 2 秒、相依服務(SQL、OpenAI)失敗率超過 5%。值班工程師每週輪替,排班表在 Confluence 的「On-call」頁面。收到警示後 10 分鐘內必須在 #incident 頻道回應。
