#include <iostream>
#include "feature1_http_rest.h"
#include "feature2_win_api.h"
#include "feature3_http_server.h"
#include "feature4_fibonacci.h"
#include "feature5_primes.h"
#include "feature6_string_reverse.h"
#include "feature7_simple_vector.h"
#include "feature8_bank_account.h"
#include "feature9_shapes.h"
#include "feature10_parentheses.h"
#include "feature11_linked_list.h"
#include "feature12_binary_search.h"
#include "feature13_sorting.h"
#include <windows.h>

int main()
{
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);
    int choice = -1;
    do {
        std::cout << "\n============================\n";
        std::cout << "    C++ 練習選單\n";
        std::cout << "============================\n";
        std::cout << "--- API / 系統 ---\n";
        std::cout << "1. HTTP REST API (天氣查詢)\n";
        std::cout << "2. Windows API  (檔案/Registry)\n";
        std::cout << "3. HTTP Server  (啟動本機伺服器)\n";
        std::cout << "--- 演算法 ---\n";
        std::cout << "4.  費波那契數列 (遞迴/迭代/DP)\n";
        std::cout << "5.  質數篩法 (Sieve of Eratosthenes)\n";
        std::cout << "6.  字串反轉 (指標 / STL)\n";
        std::cout << "--- 記憶體與物件 ---\n";
        std::cout << "7.  動態陣列 (new/delete/深拷貝)\n";
        std::cout << "8.  銀行帳戶 (封裝)\n";
        std::cout << "9.  形狀多型 (繼承/虛擬函式)\n";
        std::cout << "--- 資料結構 ---\n";
        std::cout << "10. 括號匹配 (Stack)\n";
        std::cout << "11. 雙向鏈結串列\n";
        std::cout << "12. 二分搜尋法\n";
        std::cout << "13. 合併排序 + 快速排序\n";
        std::cout << "---\n";
        std::cout << "0. 離開\n";
        std::cout << "請選擇: ";
        std::cin >> choice;

        switch (choice) {
        case 1:  runHttpRestDemo();    break;
        case 2:  runWindowsApiDemo();  break;
        case 3:  runHttpServerDemo();  break;
        case 4:  runFibonacci();       break;
        case 5:  runPrimes();          break;
        case 6:  runStringReverse();   break;
        case 7:  runSimpleVector();    break;
        case 8:  runBankAccount();     break;
        case 9:  runShapes();          break;
        case 10: runParentheses();     break;
        case 11: runLinkedList();      break;
        case 12: runBinarySearch();    break;
        case 13: runSorting();         break;
        case 0:  std::cout << "掰掰!\n"; break;
        default: std::cout << "無效選項\n"; break;
        }
    } while (choice != 0);

    return 0;
}
