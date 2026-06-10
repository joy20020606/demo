from collections.abc import Callable

import httpx
from tenacity import (
    retry,
    retry_if_exception_type,
    stop_after_attempt,
    wait_exponential,
)

TRANSIENT_EXCEPTIONS = (httpx.TransportError, httpx.HTTPStatusError)


def fetch_retry[T](fn: Callable[..., T]) -> Callable[..., T]:
    return retry(
        stop=stop_after_attempt(3),
        wait=wait_exponential(multiplier=0.5, max=8),
        retry=retry_if_exception_type(TRANSIENT_EXCEPTIONS),
        reraise=True,
    )(fn)
