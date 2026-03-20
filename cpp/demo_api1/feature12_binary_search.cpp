#include "feature12_binary_search.h"
#include <iostream>
#include <vector>

// ============================================================
// 二分搜尋法 (Binary Search)
// 前提：陣列必須已排序
// 概念：每次把搜尋範圍砍半，大幅提升效率
// 時間複雜度：O(log n)，例如 100 萬筆資料最多只需 20 次比較
// ============================================================

// ---- 方法一：迭代版 ----
// 用 lo（低）和 hi（高）兩個邊界縮小搜尋範圍
// 回傳找到的索引，找不到回傳 -1
int bsIterative(const std::vector<int>& arr, int target)
{
    int lo = 0;                        // 搜尋範圍的左邊界
    int hi = (int)arr.size() - 1;     // 搜尋範圍的右邊界

    while (lo <= hi)
    {
        int mid = lo + (hi - lo) / 2; // 計算中間索引
        // 注意：用 lo + (hi-lo)/2 而不是 (lo+hi)/2，避免整數溢位

        if (arr[mid] == target)
        {
            return mid; // 找到了，回傳索引
        }
        else if (arr[mid] < target)
        {
            lo = mid + 1; // 目標在右半段，縮小左邊界
        }
        else
        {
            hi = mid - 1; // 目標在左半段，縮小右邊界
        }
    }

    return -1; // 沒找到
}

// ---- 方法二：遞迴版 ----
// 邏輯與迭代版相同，但用遞迴呼叫自己
int bsRecursive(const std::vector<int>& arr, int target, int lo, int hi)
{
    if (lo > hi) return -1; // 基底情況：搜尋範圍為空，找不到

    int mid = lo + (hi - lo) / 2;

    if (arr[mid] == target) return mid;         // 找到
    if (arr[mid] < target)  return bsRecursive(arr, target, mid + 1, hi); // 右半段
    else                    return bsRecursive(arr, target, lo, mid - 1); // 左半段
}

// ---- 示範入口 ----
void runBinarySearch()
{
    std::cout << "\n============================\n";
    std::cout << "  二分搜尋練習\n";
    std::cout << "============================\n";

    // 建立已排序的陣列（二分搜尋的前提）
    std::vector<int> arr = {2, 5, 8, 12, 16, 23, 38, 45, 56, 72, 91};

    std::cout << "\n已排序陣列：\n  [ ";
    for (int x : arr) std::cout << x << " ";
    std::cout << "]\n";

    // 測試幾個目標值
    std::vector<int> targets = {23, 72, 1, 91, 50};

    std::cout << "\n搜尋結果：\n";
    for (int t : targets)
    {
        int idx1 = bsIterative(arr, t);
        int idx2 = bsRecursive(arr, t, 0, (int)arr.size() - 1);

        if (idx1 != -1)
            std::cout << "  找 " << t << " -> 索引 " << idx1
                      << " (迭代)，索引 " << idx2 << " (遞迴)\n";
        else
            std::cout << "  找 " << t << " -> 找不到\n";
    }

    std::cout << "\n請輸入要搜尋的數字（-1 離開）：\n";
    int target;
    while (std::cin >> target && target != -1)
    {
        int idx = bsIterative(arr, target);
        if (idx != -1)
            std::cout << "  找到！索引 = " << idx << "\n";
        else
            std::cout << "  找不到\n";
    }
}
