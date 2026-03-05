# 00_demo 專案集合

這是一個包含多語言（TypeScript / Python）示範與工具程式的專案集合。

## 專案結構

此專案主要切分為以下幾個子專案：

### 1. `ts/demo1` (TypeScript / NestJS)

這是一個基於 [NestJS](https://nestjs.com/) 建立的後端 API 服務示範專案。
包含了一些基本的服務模組、控制器以及資料庫交互介面示範。

**如何執行與除錯：**

- 建議使用 Visual Studio Code 開啟本專案。
- 已經設定好 `.vscode/launch.json`，您可以直接在「執行與偵錯 (Run and Debug)」面板選擇 `Debug demo1 (NestJS)` 來啟動服務並進行除錯。

### 2. `python/demo1_FormatFerry` (Python)

這是一個使用 Python 撰寫的 Word 檔案 (`.docx`) 翻譯工具。
特色是可以讀取 Word 檔案，將段落與表格內容翻譯為其他語言，並且**保留原本的檔案格式 (Formatting)** 輸出成新的 Word 檔案。

**相關技術與套件：**

- `python-docx`
- `deep-translator`

---

## 環境與開發規範

- 本專案包含了全域的 `.gitignore` 檔案以確保暫存檔不會被錯誤地 commit。
- 本專案受到特定 API 開發規範（Menieres API 開發規範憲法）的約束，尤其在 C# 與系統架構的相關應用上，請參考專案內的相關文件。
