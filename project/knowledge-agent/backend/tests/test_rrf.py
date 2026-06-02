from app.retrieval.hybrid import RRF_K, _fuse
from app.schemas import RetrievedBlock


def _block(cid: str, **kw) -> RetrievedBlock:
    return RetrievedBlock(
        chunk_id=cid, document_id="d", content="x", title="t", **kw
    )


def test_rrf_rewards_agreement():
    # block "a" ranks #1 in both lists; "b" only in one. "a" must win.
    vec = [_block("a", vector_score=0.9), _block("b", vector_score=0.8)]
    kw = [_block("a", keyword_score=2.0), _block("c", keyword_score=1.0)]
    merged = _fuse(vec, kw)
    assert merged["a"].rrf_score > merged["b"].rrf_score
    assert merged["a"].rrf_score == 1 / (RRF_K + 1) + 1 / (RRF_K + 1)


def test_rrf_preserves_stage_scores():
    vec = [_block("a", vector_score=0.7)]
    kw = [_block("a", keyword_score=1.5)]
    merged = _fuse(vec, kw)
    assert merged["a"].vector_score == 0.7
    assert merged["a"].keyword_score == 1.5
