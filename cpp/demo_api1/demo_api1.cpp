#include <iostream>
#include "feature1_http_rest.h"
#include "feature2_win_api.h"
#include "feature3_http_server.h"
#include <windows.h>

int main()
{
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);
    int choice = -1;
    do {
        std::cout << "\n============================\n";
        std::cout << "    API 練習選單\n";
        std::cout << "============================\n";
        std::cout << "1. HTTP REST API (天氣查詢)\n";
        std::cout << "2. Windows API  (檔案/Registry)\n";
        std::cout << "3. HTTP Server  (啟動本機伺服器)\n";
        std::cout << "0. 離開\n";
        std::cout << "請選擇: ";
        std::cin >> choice;

        switch (choice) {
        case 1: runHttpRestDemo();    break;
        case 2: runWindowsApiDemo();  break;
        case 3: runHttpServerDemo();  break;
        case 0: std::cout << "掰掰!\n"; break;
        default: std::cout << "無效選項\n"; break;
        }
    } while (choice != 0);

    return 0;
}
