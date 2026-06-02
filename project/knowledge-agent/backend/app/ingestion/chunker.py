"""Three chunking strategies, switchable so the eval dashboard can compare them.

- fixed         : token-window splitting with overlap (RecursiveCharacterTextSplitter-style)
- semantic      : embedding-distance boundaries (split where adjacent sentences diverge)
- propositional : LLM decomposes paragraphs into atomic, self-contained propositions
"""

import re

import tiktoken

from app.config import get_settings
from app.ingestion.loader import LoadedPage
from app.llm import complete, embed_texts

_s = get_settings()
_enc = tiktoken.get_encoding("cl100k_base")


class Chunk:
    def __init__(self, content: str, index: int, page_no: int | None, method: str):
        self.content = content
        self.index = index
        self.page_no = page_no
        self.method = method
        self.token_count = len(_enc.encode(content))


def _split_sentences(text: str) -> list[str]:
    parts = re.split(r"(?<=[.!?。！？])\s+", text.strip())
    return [p.strip() for p in parts if p.strip()]


def chunk_fixed(pages: list[LoadedPage]) -> list[Chunk]:
    size, overlap = _s.chunk_size, _s.chunk_overlap
    chunks: list[Chunk] = []
    idx = 0
    for page in pages:
        tokens = _enc.encode(page.text)
        start = 0
        while start < len(tokens):
            window = tokens[start : start + size]
            content = _enc.decode(window).strip()
            if content:
                chunks.append(Chunk(content, idx, page.page_no, "fixed"))
                idx += 1
            start += size - overlap
    return chunks


def _cosine(a: list[float], b: list[float]) -> float:
    dot = sum(x * y for x, y in zip(a, b))
    na = sum(x * x for x in a) ** 0.5
    nb = sum(y * y for y in b) ** 0.5
    return dot / (na * nb + 1e-9)


def chunk_semantic(pages: list[LoadedPage], threshold: float = 0.55) -> list[Chunk]:
    chunks: list[Chunk] = []
    idx = 0
    for page in pages:
        sentences = _split_sentences(page.text)
        if not sentences:
            continue
        embeddings = embed_texts(sentences)
        buffer = [sentences[0]]
        for i in range(1, len(sentences)):
            sim = _cosine(embeddings[i - 1], embeddings[i])
            if sim < threshold:
                chunks.append(Chunk(" ".join(buffer), idx, page.page_no, "semantic"))
                idx += 1
                buffer = []
            buffer.append(sentences[i])
        if buffer:
            chunks.append(Chunk(" ".join(buffer), idx, page.page_no, "semantic"))
            idx += 1
    return chunks


_PROP_SYSTEM = (
    "You decompose academic text into atomic, self-contained propositions. "
    "Each proposition states a single fact and is understandable without surrounding context. "
    "Return one proposition per line, no numbering, no commentary."
)


def chunk_propositional(pages: list[LoadedPage]) -> list[Chunk]:
    chunks: list[Chunk] = []
    idx = 0
    for page in pages:
        # cap each LLM call to a manageable window of the page
        windows = chunk_fixed([page])
        for w in windows:
            out = complete(_PROP_SYSTEM, w.content, max_tokens=1024, temperature=0.0)
            for line in out.splitlines():
                prop = line.strip().lstrip("-•* ").strip()
                if len(prop) > 20:
                    chunks.append(Chunk(prop, idx, page.page_no, "propositional"))
                    idx += 1
    return chunks


def chunk(pages: list[LoadedPage], method: str = "fixed") -> list[Chunk]:
    if method == "semantic":
        return chunk_semantic(pages)
    if method == "propositional":
        return chunk_propositional(pages)
    return chunk_fixed(pages)
