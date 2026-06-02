"""Golden Q/A set for RAGAS. Hand-written against the sample papers in /sample_docs.

Keep small but real (15-25 items in practice). Each item names the source we
expect to be retrieved so we can also report a coarse retrieval-hit rate.
"""

from pydantic import BaseModel


class GoldenItem(BaseModel):
    question: str
    ground_truth: str
    expected_source: str


GOLDEN_SET: list[GoldenItem] = [
    GoldenItem(
        question="What problem does the Transformer architecture solve compared to RNNs?",
        ground_truth=(
            "The Transformer replaces recurrence with self-attention, enabling far more "
            "parallelisation and better modelling of long-range dependencies than RNNs."
        ),
        expected_source="Attention Is All You Need",
    ),
    GoldenItem(
        question="What is the core mechanism introduced by the Transformer?",
        ground_truth="Multi-head self-attention that relates all positions in a sequence.",
        expected_source="Attention Is All You Need",
    ),
    GoldenItem(
        question="Why is positional encoding needed in the Transformer?",
        ground_truth=(
            "Because self-attention is permutation-invariant, positional encodings inject "
            "information about token order."
        ),
        expected_source="Attention Is All You Need",
    ),
    # ... extend to 15-25 items across 3-4 ingested papers for a fuller eval.
]
