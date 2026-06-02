"""Load raw text + page boundaries from an uploaded PDF or plain text."""

import io

from pypdf import PdfReader


class LoadedPage:
    def __init__(self, page_no: int, text: str):
        self.page_no = page_no
        self.text = text


def load_pdf(data: bytes) -> list[LoadedPage]:
    reader = PdfReader(io.BytesIO(data))
    pages: list[LoadedPage] = []
    for i, page in enumerate(reader.pages, start=1):
        text = (page.extract_text() or "").strip()
        if text:
            pages.append(LoadedPage(i, text))
    return pages


def load_text(data: bytes) -> list[LoadedPage]:
    return [LoadedPage(1, data.decode("utf-8", errors="ignore"))]


def load(filename: str, data: bytes) -> list[LoadedPage]:
    if filename.lower().endswith(".pdf"):
        return load_pdf(data)
    return load_text(data)
