#include "feature1_http_rest.h"
#include <windows.h>
#include <winhttp.h>
#include <iostream>
#include <string>

#pragma comment(lib, "winhttp.lib")

static std::string wstrToStr(const std::wstring& wstr) {
    if (wstr.empty()) return {};
    int size = WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), -1, nullptr, 0, nullptr, nullptr);
    std::string result(size - 1, 0);
    WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), -1, &result[0], size, nullptr, nullptr);
    return result;
}

static std::string httpGet(const std::wstring& host, const std::wstring& path) {
    std::string response;

    HINTERNET hSession = WinHttpOpen(L"WinHTTP Demo/1.0",
        WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
        WINHTTP_NO_PROXY_NAME,
        WINHTTP_NO_PROXY_BYPASS, 0);
    if (!hSession) return "Error: WinHttpOpen failed, code=" + std::to_string(GetLastError());

    DWORD decompression = WINHTTP_DECOMPRESSION_FLAG_GZIP | WINHTTP_DECOMPRESSION_FLAG_DEFLATE;
    WinHttpSetOption(hSession, WINHTTP_OPTION_DECOMPRESSION, &decompression, sizeof(decompression));

    HINTERNET hConnect = WinHttpConnect(hSession, host.c_str(), INTERNET_DEFAULT_HTTPS_PORT, 0);
    if (!hConnect) { WinHttpCloseHandle(hSession); return "Error: WinHttpConnect failed, code=" + std::to_string(GetLastError()); }

    HINTERNET hRequest = WinHttpOpenRequest(hConnect, L"GET", path.c_str(),
        nullptr, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, WINHTTP_FLAG_SECURE);
    if (!hRequest) { WinHttpCloseHandle(hConnect); WinHttpCloseHandle(hSession); return "Error: WinHttpOpenRequest failed, code=" + std::to_string(GetLastError()); }

    const wchar_t* headers = L"Accept: application/json\r\nUser-Agent: demo_api1/1.0\r\n";

    if (WinHttpSendRequest(hRequest, headers, -1,
        WINHTTP_NO_REQUEST_DATA, 0, 0, 0) &&
        WinHttpReceiveResponse(hRequest, nullptr)) {
        DWORD statusCode = 0;
        DWORD statusCodeSize = sizeof(statusCode);
        if (!WinHttpQueryHeaders(hRequest,
            WINHTTP_QUERY_STATUS_CODE | WINHTTP_QUERY_FLAG_NUMBER,
            WINHTTP_HEADER_NAME_BY_INDEX,
            &statusCode,
            &statusCodeSize,
            WINHTTP_NO_HEADER_INDEX)) {
            statusCode = 0;
        }

        DWORD dwSize = 0;
        do {
            dwSize = 0;
            WinHttpQueryDataAvailable(hRequest, &dwSize);
            if (dwSize == 0) break;
            std::string buf(dwSize, 0);
            DWORD dwDownloaded = 0;
            WinHttpReadData(hRequest, &buf[0], dwSize, &dwDownloaded);
            response.append(buf, 0, dwDownloaded);
        } while (dwSize > 0);

        if (response.empty()) {
            response = "Error: empty response, HTTP status=" + std::to_string(statusCode);
        }
    }
    else {
        response = "Error: request failed, code=" + std::to_string(GetLastError());
    }

    WinHttpCloseHandle(hRequest);
    WinHttpCloseHandle(hConnect);
    WinHttpCloseHandle(hSession);
    return response;
}

void runHttpRestDemo() {
    std::cout << "\n=== HTTP REST API Demo ===\n";
    std::cout << "請輸入城市名稱 (e.g. Taipei, Tokyo): ";
    std::string city;
    std::cin >> city;

    std::wstring path = L"/" + std::wstring(city.begin(), city.end()) + L"?format=j1";
    std::cout << "正在查詢 wttr.in 天氣資料...\n";

    std::string result = httpGet(L"wttr.in", path);

    if (result.find("temp_C") != std::string::npos) {
        auto extractField = [&](const std::string& key) -> std::string {
            size_t pos = result.find("\"" + key + "\":");
            if (pos == std::string::npos) return "N/A";
            pos = result.find("\"", pos + key.size() + 3);
            if (pos == std::string::npos) return "N/A";
            size_t end = result.find("\"", pos + 1);
            return result.substr(pos + 1, end - pos - 1);
        };

        std::cout << "\n--- " << city << " 天氣 ---\n";
        std::cout << "溫度 (C): " << extractField("temp_C") << "\n";
        std::cout << "體感 (C): " << extractField("FeelsLikeC") << "\n";
        std::cout << "天氣描述: " << extractField("weatherDesc") << "\n";
        std::cout << "風速 (kmph): " << extractField("windspeedKmph") << "\n";
        std::cout << "濕度 (%): " << extractField("humidity") << "\n";
    }
    else {
        std::cout << "回應:\n" << result.substr(0, 500) << "\n";
    }
}
