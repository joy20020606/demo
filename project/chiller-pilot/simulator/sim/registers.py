MAX_RAW = {1: 0xFFFF, 2: 0xFFFFFFFF}


def encode(value: float, scale: float, words: int) -> list[int]:
    raw = round(value / scale)
    raw = max(0, min(MAX_RAW[words], raw))
    if words == 1:
        return [raw]
    return [(raw >> 16) & 0xFFFF, raw & 0xFFFF]


def decode(raw_words: list[int], scale: float) -> float:
    raw = 0
    for word in raw_words:
        raw = (raw << 16) | word
    return raw * scale
