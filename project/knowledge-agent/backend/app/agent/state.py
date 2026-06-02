from typing import TypedDict

from app.schemas import Citation, RagConfig, RetrievedBlock


class AgentState(TypedDict, total=False):
    query: str
    config: RagConfig
    expanded_queries: list[str]
    retrieved: list[RetrievedBlock]
    answer: str
    citations: list[Citation]
    retried: bool
