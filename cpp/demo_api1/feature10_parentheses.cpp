#include "feature10_parentheses.h"
#include <iostream>
#include <string>
#include <stack>
#include <vector>

// ============================================================
// 括號匹配 (Valid Parentheses)
// 使用堆疊 (Stack) 來解決
//
// 演算法：
//   1. 遇到左括號（'(', '[', '{'）→ 推入 Stack
//   2. 遇到右括號（')', ']', '}'）→ 從 Stack 取出頂端，
//      檢查是否與右括號對應
//   3. 最後 Stack 必須是空的，才算完全匹配
// ============================================================

bool isValidParentheses(const std::string& s)
{
    std::stack<char> st; // 用來存放「待匹配的左括號」

    for (char c : s) // 逐字元掃描
    {
        // 遇到左括號，推入 Stack
        if (c == '(' || c == '[' || c == '{')
        {
            st.push(c);
        }
        // 遇到右括號
        else if (c == ')' || c == ']' || c == '}')
        {
            // Stack 是空的，代表沒有對應的左括號 → 不匹配
            if (st.empty()) return false;

            char top = st.top(); // 查看 Stack 頂端的左括號
            st.pop();            // 取出頂端

            // 檢查左右括號是否配對
            if (c == ')' && top != '(') return false;
            if (c == ']' && top != '[') return false;
            if (c == '}' && top != '{') return false;
        }
        // 其他字元（數字、字母）忽略
    }

    // Stack 為空 = 所有左括號都有配對 → 匹配成功
    // Stack 非空 = 還有左括號未被匹配 → 失敗
    return st.empty();
}

// ---- 示範入口 ----
void runParentheses()
{
    std::cout << "\n============================\n";
    std::cout << "  括號匹配練習\n";
    std::cout << "============================\n";

    // 測試案例
    std::vector<std::string> tests = {
        "()",
        "[]{}()",
        "{[()]}",
        "([)]",   // 交叉括號，不合法
        "{{}",    // 少了一個右括號
        "",       // 空字串，合法
        "(((",    // 全是左括號
        "a+b*(c-d)" // 含其他字元
    };

    for (const auto& t : tests)
    {
        bool result = isValidParentheses(t);
        std::cout << "  \"" << t << "\"\t-> "
                  << (result ? "合法" : "不合法") << "\n";
    }

    std::cout << "\n請輸入要測試的字串（q 離開）：\n";
    std::string input;
    while (std::cin >> input && input != "q")
    {
        std::cout << "  -> " << (isValidParentheses(input) ? "合法" : "不合法") << "\n";
    }
}
