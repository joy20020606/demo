"""Query expansion: HyDE and Multi-query.

HyDE        : LLM drafts a hypothetical answer; we retrieve against that
              (richer, answer-shaped vector than the bare question).
Multi-query : LLM paraphrases the question N ways; we union the retrieved sets.
"""

from app.llm import complete

_HYDE_SYSTEM = (
    "Write a short, factual paragraph that would directly answer the user's question, "
    "as if quoted from an academic source. Do not hedge. 3-4 sentences."
)

_MULTI_SYSTEM = (
    "Generate 3 alternative phrasings of the user's question that capture different "
    "wordings and sub-aspects. One per line, no numbering."
)


def hyde(query: str) -> str:
    return complete(_HYDE_SYSTEM, query, max_tokens=256, temperature=0.3).strip()


def multi_query(query: str) -> list[str]:
    out = complete(_MULTI_SYSTEM, query, max_tokens=256, temperature=0.5)
    variants = [line.strip().lstrip("-•* ").strip() for line in out.splitlines()]
    return [v for v in variants if len(v) > 5][:3]


def expand(query: str, use_hyde: bool, use_multi_query: bool) -> list[str]:
    """Return the list of query strings to actually search with (always includes the original)."""
    queries = [query]
    if use_hyde:
        queries.append(hyde(query))
    if use_multi_query:
        queries.extend(multi_query(query))
    return queries
