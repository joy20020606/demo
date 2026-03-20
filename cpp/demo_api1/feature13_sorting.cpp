#include "feature13_sorting.h"
#include <iostream>
#include <vector>
#include <cstdlib>
#include <ctime>

// ============================================================
// 合併排序 (Merge Sort) + 快速排序 (Quick Sort)
// 兩者都使用分治法（Divide and Conquer）：
//   1. 分割（Divide）：把問題拆成更小的子問題
//   2. 征服（Conquer）：遞迴解決子問題
//   3. 合併（Combine）：把子問題的結果合起來
// ============================================================

// ============================================================
// 合併排序 (Merge Sort)
// 時間複雜度：O(n log n)，穩定排序
// 缺點：需要額外的 O(n) 空間
// ============================================================

// merge：把兩個已排序的子陣列 arr[lo..mid] 和 arr[mid+1..hi] 合併
static void merge(std::vector<int>& arr, int lo, int mid, int hi)
{
    // 把原陣列複製到暫存空間
    std::vector<int> temp(arr.begin() + lo, arr.begin() + hi + 1);

    int leftLen = mid - lo + 1; // 左半段長度
    int i = 0;                  // 左半段的索引（相對於 temp）
    int j = leftLen;            // 右半段的索引（相對於 temp）
    int k = lo;                 // 寫回原陣列的位置

    // 比較左右兩半，依序把較小的放回原陣列
    while (i < leftLen && j < (int)temp.size())
    {
        if (temp[i] <= temp[j])
            arr[k++] = temp[i++]; // 左邊較小，取左邊
        else
            arr[k++] = temp[j++]; // 右邊較小，取右邊
    }

    // 把剩餘的元素補回去
    while (i < leftLen)        arr[k++] = temp[i++];
    while (j < (int)temp.size()) arr[k++] = temp[j++];
}

// mergeSort：遞迴分割陣列，再呼叫 merge 合併
void mergeSort(std::vector<int>& arr, int lo, int hi)
{
    if (lo >= hi) return; // 基底情況：只有一個元素，不需要排序

    int mid = lo + (hi - lo) / 2; // 中間點

    mergeSort(arr, lo, mid);      // 排序左半段
    mergeSort(arr, mid + 1, hi);  // 排序右半段
    merge(arr, lo, mid, hi);      // 合併兩段
}

// ============================================================
// 快速排序 (Quick Sort)
// 時間複雜度：平均 O(n log n)，最差 O(n²)（已排序時）
// 優點：In-place（不需額外空間），實際執行通常比合併排序快
// ============================================================

// partition：選一個基準值（pivot），
//            把比 pivot 小的放左邊，比 pivot 大的放右邊
//            回傳 pivot 最終的位置
static int partition(std::vector<int>& arr, int lo, int hi)
{
    int pivot = arr[hi]; // 選最後一個元素作為基準值
    int i     = lo - 1;  // i 指向「小於 pivot 區域」的最後一個位置

    for (int j = lo; j < hi; j++)
    {
        if (arr[j] <= pivot)
        {
            i++;
            std::swap(arr[i], arr[j]); // 把小的元素交換到左邊
        }
    }

    // 把 pivot 放到正確位置（i+1）
    std::swap(arr[i + 1], arr[hi]);
    return i + 1; // 回傳 pivot 的最終索引
}

// quickSort：遞迴分割，分治解決
void quickSort(std::vector<int>& arr, int lo, int hi)
{
    if (lo >= hi) return; // 基底情況

    int pivotIdx = partition(arr, lo, hi); // 把陣列分成兩半

    quickSort(arr, lo, pivotIdx - 1);     // 排序左半段（比 pivot 小）
    quickSort(arr, pivotIdx + 1, hi);     // 排序右半段（比 pivot 大）
}

// ---- 印出陣列 ----
static void printArr(const std::vector<int>& arr)
{
    std::cout << "  [ ";
    for (int x : arr) std::cout << x << " ";
    std::cout << "]\n";
}

// ---- 示範入口 ----
void runSorting()
{
    std::cout << "\n============================\n";
    std::cout << "  排序演算法練習\n";
    std::cout << "============================\n";

    srand((unsigned)time(nullptr)); // 設定亂數種子

    // 產生 12 個 1~99 的亂數
    std::vector<int> original;
    for (int i = 0; i < 12; i++)
        original.push_back(rand() % 99 + 1);

    std::cout << "\n原始亂數陣列：\n";
    printArr(original);

    // --- 合併排序 ---
    std::vector<int> arr1 = original; // 複製一份
    mergeSort(arr1, 0, (int)arr1.size() - 1);
    std::cout << "\n[合併排序] 結果：\n";
    printArr(arr1);

    // --- 快速排序 ---
    std::vector<int> arr2 = original; // 再複製一份
    quickSort(arr2, 0, (int)arr2.size() - 1);
    std::cout << "\n[快速排序] 結果：\n";
    printArr(arr2);
}
