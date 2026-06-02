"""Provider clients: OpenAI for embeddings, Anthropic (Claude) for generation.

Claude has no embedding endpoint, so embeddings come from OpenAI
text-embedding-3-small while generation/agent reasoning runs on Claude.
"""

from functools import lru_cache

from anthropic import Anthropic
from openai import OpenAI

from app.config import get_settings

_s = get_settings()


@lru_cache
def _openai() -> OpenAI:
    return OpenAI(api_key=_s.openai_api_key)


@lru_cache
def _anthropic() -> Anthropic:
    return Anthropic(api_key=_s.anthropic_api_key)


def embed_texts(texts: list[str]) -> list[list[float]]:
    if not texts:
        return []
    resp = _openai().embeddings.create(model=_s.embedding_model, input=texts)
    return [d.embedding for d in resp.data]


def embed_query(text: str) -> list[float]:
    return embed_texts([text])[0]


def complete(system: str, user: str, max_tokens: int = 1024, temperature: float = 0.2) -> str:
    msg = _anthropic().messages.create(
        model=_s.anthropic_model,
        max_tokens=max_tokens,
        temperature=temperature,
        system=system,
        messages=[{"role": "user", "content": user}],
    )
    return "".join(block.text for block in msg.content if block.type == "text")
