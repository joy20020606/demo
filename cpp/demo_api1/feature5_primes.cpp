#include "feature5_primes.h"
#include <iostream>
#include <vector>

// ============================================================
// 埃拉托斯特尼篩法 (Sieve of Eratosthenes)
// 原理：從 2 開始，把每個質數的所有倍數標記為合數
// 時間複雜度：O(n log log n)，比逐一測試快得多
// ============================================================

// 回傳 2 ~ n 之間的所有質數
std::vector<int> sieve(int n)
{
    // isPrime[i] = true 表示 i 是質數
    // 初始假設所有數字都是質數
    std::vector<bool> isPrime(n + 1, true);

    isPrime[0] = false; // 0 不是質數
    isPrime[1] = false; // 1 不是質數

    // 從 2 開始，對每個質數 p，把 p*p ~ n 之間 p 的倍數全部標記掉
    // 為什麼從 p*p 開始？因為 p*2, p*3, ..., p*(p-1) 已經被更小的質數篩過了
    for (int p = 2; (long long)p * p <= n; p++)
    {
        if (isPrime[p])
        {
            // 把 p 的倍數都標記為合數
            for (int multiple = p * p; multiple <= n; multiple += p)
            {
                isPrime[multiple] = false;
            }
        }
    }

    // 收集所有質數到結果陣列
    std::vector<int> primes;
    for (int i = 2; i <= n; i++)
    {
        if (isPrime[i]) primes.push_back(i);
    }
    return primes;
}

// ---- 示範入口 ----
void runPrimes()
{
    std::cout << "\n============================\n";
    std::cout << "  質數篩法練習\n";
    std::cout << "============================\n";

    int n;
    std::cout << "請輸入 N（找出 2 ~ N 的所有質數）: ";
    std::cin >> n;

    if (n < 2)
    {
        std::cout << "N 必須大於等於 2\n";
        return;
    }

    std::vector<int> primes = sieve(n);

    std::cout << "\n2 ~ " << n << " 之間共有 " << primes.size() << " 個質數：\n";

    int count = 0;
    for (int p : primes)
    {
        std::cout << p;
        count++;
        // 每 10 個換一行，方便閱讀
        if (count % 10 == 0) std::cout << "\n";
        else std::cout << "\t";
    }
    std::cout << "\n";
}
