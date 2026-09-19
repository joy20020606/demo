from functools import lru_cache

@lru_cache(maxsize=2)          # 最多記 2 筆(有括號 = 帶設定,跟 router.get(...) 同一招)
def slow_square(n):
    print(f"  真的在算 {n}...")
    return n * n

print(slow_square(3))   # 真的算
print(slow_square(3))   # 同參數 → 直接拿,不印「真的在算」
print(slow_square(4))   # 新參數 → 真的算
print(slow_square(5))   # 新參數 → 真的算,但滿了 → 丟掉最久沒用的 3
print(slow_square(3))   # 3 被丟了 → 又要重算
print(slow_square.cache_info())   # 內建統計:hits 幾次、misses 幾次