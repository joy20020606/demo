from app.agent.citations import build_citations, strip_invalid_markers
from app.schemas import RetrievedBlock


def _blocks(n: int) -> list[RetrievedBlock]:
    return [
        RetrievedBlock(
            chunk_id=f"c{i}", document_id="d", content="x",
            title=f"Doc{i}", author=f"Author{i}", theory_tag=f"Theory{i}",
        )
        for i in range(1, n + 1)
    ]


def test_valid_citations_resolved():
    blocks = _blocks(2)
    answer = "Claim one [1]. Claim two [2]."
    citations, valid = build_citations(answer, blocks)
    assert valid == {1, 2}
    assert citations[0].source == "Doc1"
    assert citations[1].author == "Author2"


def test_hallucinated_marker_dropped():
    blocks = _blocks(1)
    answer = "Real [1]. Fabricated [5]."
    citations, valid = build_citations(answer, blocks)
    assert valid == {1}
    cleaned = strip_invalid_markers(answer, valid)
    assert "[5]" not in cleaned
    assert "[1]" in cleaned
