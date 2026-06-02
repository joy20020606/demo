"""LangGraph StateGraph: retrieve → generate → validate_citations (→ retry once).

Hand-built rather than langgraph.prebuilt create_react_agent (now deprecated in
favour of langchain.agents.create_agent) — an explicit graph demonstrates the
control flow and citation guardrail more clearly.
"""

import time

from langgraph.graph import END, StateGraph
from sqlalchemy.orm import Session

from app.agent.citations import build_citations, strip_invalid_markers
from app.agent.state import AgentState
from app.agent.tools import retrieve_knowledge
from app.llm import complete
from app.schemas import ChatResponse, RagConfig

_GEN_SYSTEM = (
    "你是嚴謹的知識庫研究助理。只能根據提供的內容區塊回答，"
    "每一個事實主張後面都必須加上來源標記 [n]（n 是區塊編號）。"
    "若內容不足以回答，明確說「現有資料不足以回答」，不要編造。"
    "用繁體中文回答，並在引用時點出作者與理論（若區塊有提供）。"
)


def _format_context(blocks) -> str:
    parts = []
    for i, b in enumerate(blocks, start=1):
        header = f"[{i}] 來源:《{b.title}》"
        if b.author:
            header += f"｜作者: {b.author}"
        if b.theory_tag:
            header += f"｜理論: {b.theory_tag}"
        if b.page_no:
            header += f"｜頁: {b.page_no}"
        parts.append(f"{header}\n{b.content}")
    return "\n\n".join(parts)


def build_graph(session: Session):
    def retrieve_node(state: AgentState) -> AgentState:
        config: RagConfig = state["config"]
        blocks, queries = retrieve_knowledge(session, state["query"], config)
        return {"retrieved": blocks, "expanded_queries": queries}

    def generate_node(state: AgentState) -> AgentState:
        blocks = state["retrieved"]
        if not blocks:
            return {"answer": "現有資料不足以回答。", "citations": []}
        context = _format_context(blocks)
        user = f"問題：{state['query']}\n\n可用內容區塊：\n{context}"
        answer = complete(_GEN_SYSTEM, user, max_tokens=1024)
        return {"answer": answer}

    def validate_node(state: AgentState) -> AgentState:
        blocks = state["retrieved"]
        citations, valid_ns = build_citations(state.get("answer", ""), blocks)
        answer = strip_invalid_markers(state["answer"], valid_ns)
        return {"answer": answer, "citations": citations}

    def needs_retry(state: AgentState) -> str:
        grounded = bool(state.get("citations"))
        if grounded or state.get("retried"):
            return END
        return "relax"

    def relax_node(state: AgentState) -> AgentState:
        config = state["config"].model_copy(
            update={"similarity_threshold": 0.0, "final_k": state["config"].final_k + 2}
        )
        return {"config": config, "retried": True}

    g = StateGraph(AgentState)
    g.add_node("retrieve", retrieve_node)
    g.add_node("generate", generate_node)
    g.add_node("validate", validate_node)
    g.add_node("relax", relax_node)

    g.set_entry_point("retrieve")
    g.add_edge("retrieve", "generate")
    g.add_edge("generate", "validate")
    g.add_conditional_edges("validate", needs_retry, {END: END, "relax": "relax"})
    g.add_edge("relax", "retrieve")
    return g.compile()


def run_agent(session: Session, query: str, config: RagConfig) -> ChatResponse:
    start = time.perf_counter()
    graph = build_graph(session)
    final: AgentState = graph.invoke({"query": query, "config": config, "retried": False})
    latency_ms = int((time.perf_counter() - start) * 1000)
    return ChatResponse(
        answer=final.get("answer", ""),
        citations=final.get("citations", []),
        blocks=final.get("retrieved", []),
        expanded_queries=final.get("expanded_queries", []),
        latency_ms=latency_ms,
    )
