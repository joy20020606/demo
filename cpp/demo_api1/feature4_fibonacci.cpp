#include "feature4_fibonacci.h"
#include <iostream>
#include <vector>

// ============================================================
// 費波那契數列 (Fibonacci Sequence)
// 定義：F(0)=0, F(1)=1, F(n) = F(n-1) + F(n-2)
// ============================================================

// ---- 方法一：遞迴 (Recursion) ----
// 優點：程式碼簡潔、直觀
// 缺點：重複計算大量子問題，時間複雜度 O(2^n)，n>40 會非常慢
long long fibRecursive(int n)
{
    // 基底情況：F(0)=0, F(1)=1
    if (n <= 1) return n;
    // 遞迴拆解：F(n) = F(n-1) + F(n-2)
    return fibRecursive(n - 1) + fibRecursive(n - 2);
}

// ---- 方法二：迭代 (Iteration) ----
// 用迴圈取代遞迴，從底部往上計算
// 時間複雜度 O(n)，空間複雜度 O(1)
long long fibIterative(int n)
{
    if (n <= 1) return n;

    long long prev = 0; // F(n-2)
    long long curr = 1; // F(n-1)

    for (int i = 2; i <= n; i++)
    {
        long long next = prev + curr; // 計算下一個值
        prev = curr;                  // 往前移動一格
        curr = next;
    }
    return curr;
}

// ---- 方法三：動態規劃 (Dynamic Programming) ----
// 用陣列把每個子問題的答案存起來（記憶化），避免重複計算
// 時間複雜度 O(n)，空間複雜度 O(n)
long long fibDP(int n)
{
    if (n <= 1) return n;

    // dp[i] 儲存 F(i) 的結果
    std::vector<long long> dp(n + 1, 0);
    dp[0] = 0;
    dp[1] = 1;

    for (int i = 2; i <= n; i++)
    {
        dp[i] = dp[i - 1] + dp[i - 2]; // 直接查表，不重複計算
    }
    return dp[n];
}

// ---- 示範入口 ----
void runFibonacci()
{
    std::cout << "\n============================\n";
    std::cout << "  費波那契數列練習\n";
    std::cout << "============================\n";

    int n;
    std::cout << "請輸入 n（建議 0~40）: ";
    std::cin >> n;

    if (n < 0 || n > 60)
    {
        std::cout << "請輸入 0~60 之間的數字\n";
        return;
    }

    std::cout << "\n[遞迴]  F(" << n << ") = " << fibRecursive(n) << "\n";
    std::cout << "[迭代]  F(" << n << ") = " << fibIterative(n) << "\n";
    std::cout << "[動態]  F(" << n << ") = " << fibDP(n) << "\n";

    std::cout << "\n前 " << n + 1 << " 個費波那契數（迭代）：\n";
    for (int i = 0; i <= n; i++)
    {
        std::cout << "  F(" << i << ") = " << fibIterative(i) << "\n";
    }
}
