import sys
import argparse
from pathlib import Path
from translator import DocxTranslator

def print_banner():
    print("="*50)
    print("       FormatFerry - Word Document Translator")
    print("="*50)

def main():
    print_banner()
    
    parser = argparse.ArgumentParser(description="Translate Word documents while preserving formatting.")
    parser.add_argument("input", help="Path to an input .docx file or a folder containing .docx files")
    parser.add_argument("-o", "--output", help="Path to the output .docx file (single-file mode only)")
    parser.add_argument("--out-dir", help="Output folder (required for folder mode if you do not want to overwrite defaults)")
    parser.add_argument("-l", "--lang", default="ms", help="Target language code (e.g., 'en', 'ms', 'ja', 'zh-TW'). Default is 'ms' (Malay)")
    parser.add_argument("-r", "--recursive", action="store_true", help="When input is a folder, search .docx files recursively")

    if len(sys.argv) == 1:
        parser.print_help()
        print("\nExamples:")
        print("  python main.py input.docx -l en")
        print("  python main.py . -l ms --recursive --out-dir translated")
        return

    args = parser.parse_args()

    translator = DocxTranslator(target_lang=args.lang)
    try:
        input_path = Path(args.input)

        if input_path.is_file():
            translator.translate_docx(str(input_path), args.output)
            return

        if input_path.is_dir():
            pattern = "**/*.docx" if args.recursive else "*.docx"
            files = sorted(input_path.glob(pattern))
            files = [p for p in files if p.is_file()]

            if not files:
                print(f"[!] No .docx files found in: {input_path}")
                return

            out_dir = Path(args.out_dir) if args.out_dir else input_path
            out_dir.mkdir(parents=True, exist_ok=True)

            print(f"Found {len(files)} .docx file(s). Starting translation to '{args.lang}'...")
            for file_path in files:
                output_name = f"{file_path.stem}_{args.lang}{file_path.suffix}"
                output_path = out_dir / output_name
                print(f"\n-> Processing: {file_path}")
                translator.translate_docx(str(file_path), str(output_path))

            print(f"\nAll done. Output folder: {out_dir}")
            return

        print(f"[!] Input path does not exist: {input_path}")
    except Exception as e:
        print(f"\n[!] An error occurred: {e}")

if __name__ == "__main__":
    main()
