# 00_demo

這是一個多語言示範專案集合，包含：
- TypeScript / NestJS API 範例
- Python Word 翻譯工具（FormatFerry）

## 專案目錄

### `ts/demo1` (NestJS)

NestJS 後端 API 示範，含 module/controller/service 結構。

快速啟動：

```bash
cd ts/demo1
npm install
npm run start:dev
```

### `python/demo1_FormatFerry` (Python)

Word (`.docx`) 翻譯工具，可翻譯段落與表格內容並盡量保留原格式。

快速啟動（原始 Python）：

```bash
cd python/demo1_FormatFerry
python main.py .\input.docx -l ms
```

打包為執行檔：

```bash
cd python/demo1_FormatFerry
pyinstaller --onefile --name FormatFerry main.py
```

打包後可用：

```bash
.\dist\FormatFerry.exe .\input.docx -l en
```

或使用 `dist/run_translate.bat`（固定設定、可雙擊執行）。

## Git 與忽略規則

根目錄已提供 `.gitignore`，已包含 Python 打包常見輸出（如 `.venv/`、`build/`、`*.spec`）。
