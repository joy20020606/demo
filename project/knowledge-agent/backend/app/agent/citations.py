"""Citation assembly + validation.

The generate prompt forces a [n] marker after every claim. We then map each
[n] back to a retrieved block and resolve {source, author, theory}. Any [n]
the model invented with no backing block is dropped — this is the guardrail
behind the company's "citation accuracy" metric.
"""

import re

from app.schemas import Citation, RetrievedBlock

_CITE_RE = re.compile(r"\[(\d+)\]")


def build_citations(answer: str, blocks: list[RetrievedBlock]) -> tuple[list[Citation], set[int]]:
    used = {int(n) for n in _CITE_RE.findall(answer)}
    valid: list[Citation] = []
    valid_ns: set[int] = set()
    for n in sorted(used):
        if 1 <= n <= len(blocks):
            b = blocks[n - 1]
            valid.append(
                Citation(
                    n=n,
                    chunk_id=b.chunk_id,
                    source=b.title,
                    author=b.author,
                    theory=b.theory_tag,
                    page_no=b.page_no,
                )
            )
            valid_ns.add(n)
    return valid, valid_ns


def strip_invalid_markers(answer: str, valid_ns: set[int]) -> str:
    def repl(m: re.Match) -> str:
        return m.group(0) if int(m.group(1)) in valid_ns else ""

    return _CITE_RE.sub(repl, answer)
