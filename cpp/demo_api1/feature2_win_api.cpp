#include "feature2_win_api.h"
#include <windows.h>
#include <iostream>
#include <string>

static void demoListDirectory() {
    std::cout << "\n--- 列出目錄檔案 ---\n";
    std::cout << "請輸入目錄路徑 (e.g. C:\\Windows): ";
    std::string path;
    std::cin.ignore();
    std::getline(std::cin, path);

    std::wstring wpath(path.begin(), path.end());
    wpath += L"\\*";

    WIN32_FIND_DATAW ffd;
    HANDLE hFind = FindFirstFileW(wpath.c_str(), &ffd);
    if (hFind == INVALID_HANDLE_VALUE) {
        std::cout << "無法開啟目錄，錯誤碼: " << GetLastError() << "\n";
        return;
    }

    int count = 0;
    do {
        if (count >= 20) { std::cout << "... (只顯示前 20 筆)\n"; break; }
        std::wstring name(ffd.cFileName);
        std::string nameStr(name.begin(), name.end());
        if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
            std::cout << "[DIR]  " << nameStr << "\n";
        else
            std::cout << "[FILE] " << nameStr
            << " (" << ffd.nFileSizeLow << " bytes)\n";
        count++;
    } while (FindNextFileW(hFind, &ffd));

    FindClose(hFind);
}

static void demoRegistry() {
    std::cout << "\n--- Registry 讀寫 ---\n";

    HKEY hKey;
    const wchar_t* subKey = L"SOFTWARE\\DemoApiApp";
    RegCreateKeyExW(HKEY_CURRENT_USER, subKey, 0, nullptr,
        REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nullptr, &hKey, nullptr);

    const wchar_t* valueName = L"LastRun";
    SYSTEMTIME st;
    GetLocalTime(&st);
    wchar_t timeStr[64];
    swprintf_s(timeStr, L"%04d-%02d-%02d %02d:%02d:%02d",
        st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond);

    RegSetValueExW(hKey, valueName, 0, REG_SZ,
        (const BYTE*)timeStr, (DWORD)((wcslen(timeStr) + 1) * sizeof(wchar_t)));
    std::wcout << L"已寫入 Registry: HKCU\\" << subKey << L"\\" << valueName << L" = " << timeStr << L"\n";

    wchar_t readBuf[64] = {};
    DWORD bufSize = sizeof(readBuf);
    if (RegQueryValueExW(hKey, valueName, nullptr, nullptr, (LPBYTE)readBuf, &bufSize) == ERROR_SUCCESS)
        std::wcout << L"讀回值: " << readBuf << L"\n";

    RegCloseKey(hKey);
}

static void demoFileReadWrite() {
    std::cout << "\n--- 檔案讀寫 ---\n";
    const wchar_t* filePath = L"demo_output.txt";

    const char* content = "Hello from Windows API!\r\nThis is written by CreateFile / WriteFile.\r\n";
    HANDLE hFile = CreateFileW(filePath, GENERIC_WRITE, 0, nullptr,
        CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
    if (hFile == INVALID_HANDLE_VALUE) {
        std::cout << "建立檔案失敗，錯誤碼: " << GetLastError() << "\n";
        return;
    }
    DWORD written;
    WriteFile(hFile, content, (DWORD)strlen(content), &written, nullptr);
    CloseHandle(hFile);
    std::cout << "已寫入 demo_output.txt (" << written << " bytes)\n";

    hFile = CreateFileW(filePath, GENERIC_READ, FILE_SHARE_READ, nullptr,
        OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
    char buf[256] = {};
    DWORD read;
    ReadFile(hFile, buf, sizeof(buf) - 1, &read, nullptr);
    CloseHandle(hFile);
    std::cout << "讀回內容:\n" << buf;
}

void runWindowsApiDemo() {
    int choice = 0;
    do {
        std::cout << "\n=== Windows API Demo ===\n";
        std::cout << "1. 列出目錄檔案 (FindFirstFile)\n";
        std::cout << "2. Registry 讀寫\n";
        std::cout << "3. 檔案讀寫 (CreateFile)\n";
        std::cout << "0. 返回\n";
        std::cout << "選擇: ";
        std::cin >> choice;
        switch (choice) {
        case 1: demoListDirectory(); break;
        case 2: demoRegistry();      break;
        case 3: demoFileReadWrite(); break;
        }
    } while (choice != 0);
}
