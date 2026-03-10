# FormatFerry - Word 文件翻譯器

這是一個基於 Python 的 Word 文件翻譯工具，能夠將 `.docx` 檔案中的內容翻譯成指定語系，並盡可能保留原始的段落格式。

## 功能特點

- **自動翻譯**：使用 `deep-translator` (Google Translate API) 進行翻譯。
- **格式保留**：儘量維持段落樣式（粗體、斜體、字體大小等）。
- **表格支援**：可處理 Word 檔案中的表格內容。
- **簡單易用**：內建命令列介面 (CLI)。

## 安裝需求

```bash
pip install python-docx deep-translator
```

## 使用方法

在終端機中執行：

```bash
python main.py [檔案或資料夾路徑] -l [目標語言代碼]
```

### 範例

翻譯成馬來文 (預設):

```bash
python main.py input.docx -l ms
```

翻譯成英文:

```bash
python main.py input.docx -l en
```

翻譯成日文:

```bash
python main.py input.docx -l ja
```

翻譯資料夾內所有 Word 檔（不含子資料夾）:

```bash
python main.py ./docs -l ms --out-dir ./translated
```

遞迴翻譯資料夾（含子資料夾）:

```bash
python main.py ./docs -l en --recursive --out-dir ./translated
```

## 打包成 EXE

```bash
pyinstaller --onefile --name FormatFerry main.py
```

產生後可執行：

```bash
./dist/FormatFerry.exe input.docx -l ms
./dist/FormatFerry.exe ./docs -l en --recursive --out-dir ./translated
```

如果有額外資源檔（例如 `assets/`），請在打包時加入：

```bash
pyinstaller --onefile --name FormatFerry --add-data "assets;assets" main.py
```

## 目錄結構

- `main.py`: 程式入口，負責解析命令列參數。
- `translator.py`: 翻譯核心邏輯，包含格式保留處理。
- `requirements.txt`: 專案依賴。
- `demo_input.docx`: 測試用的範例文件。
