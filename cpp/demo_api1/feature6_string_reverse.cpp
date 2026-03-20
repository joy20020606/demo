#include "feature6_string_reverse.h"
#include <iostream>
#include <string>
#include <algorithm>

// ============================================================
// 字串反轉 (String Reverse)
// 練習：指標操作 與 STL 迭代器
// ============================================================

// ---- 方法一：指標版 In-place ----
// 使用兩個指標，一個從頭、一個從尾，交換後往中間靠攏
// 不需要額外空間，直接在原字元陣列上操作
// 參數：char* str 是字元陣列的起始位址（指標）
void reverseInPlace(char* str)
{
    if (str == nullptr) return;

    char* left  = str;           // 左指標：指向第一個字元
    char* right = str;

    // 先讓 right 移到最後一個字元
    while (*right != '\0') right++;
    right--; // 退回到最後一個字元（不是 '\0'）

    // 兩指標往中間靠攏，交換字元
    while (left < right)
    {
        // 用 XOR swap 或暫存變數交換
        char temp = *left;
        *left  = *right;
        *right = temp;

        left++;   // 左指標往右移
        right--;  // 右指標往左移
    }
}

// ---- 方法二：STL 迭代器版 ----
// std::string 提供 begin() 和 end() 迭代器
// std::reverse 是 STL 的標準演算法，內部也是雙指標交換
void reverseSTL(std::string& s)
{
    // begin() 指向第一個字元，end() 指向最後一個字元的後一位
    std::reverse(s.begin(), s.end());
}

// ---- 示範入口 ----
void runStringReverse()
{
    std::cout << "\n============================\n";
    std::cout << "  字串反轉練習\n";
    std::cout << "============================\n";

    std::string input;
    std::cout << "請輸入一個字串: ";
    std::cin >> input;

    // --- 方法一：指標版 ---
    // 把 std::string 轉成 char 陣列來示範指標操作
    std::string s1 = input;
    reverseInPlace(&s1[0]); // &s1[0] 取得底層 char 陣列的指標
    std::cout << "\n[指標版] 反轉結果: " << s1 << "\n";

    // --- 方法二：STL 版 ---
    std::string s2 = input;
    reverseSTL(s2);
    std::cout << "[STL版]  反轉結果: " << s2 << "\n";
}
