#include "feature3_http_server.h"
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <iostream>
#include <string>
#include <sstream>
#include <ctime>

#pragma comment(lib, "ws2_32.lib")

static std::string buildJsonResponse(const std::string& body) {
    std::ostringstream resp;
    resp << "HTTP/1.1 200 OK\r\n"
        << "Content-Type: application/json\r\n"
        << "Content-Length: " << body.size() << "\r\n"
        << "Connection: close\r\n"
        << "\r\n"
        << body;
    return resp.str();
}

static std::string build404Response() {
    std::string body = "{\"error\":\"Not Found\"}";
    std::ostringstream resp;
    resp << "HTTP/1.1 404 Not Found\r\n"
        << "Content-Type: application/json\r\n"
        << "Content-Length: " << body.size() << "\r\n"
        << "Connection: close\r\n"
        << "\r\n"
        << body;
    return resp.str();
}

static std::string handleRequest(const std::string& request) {
    std::istringstream ss(request);
    std::string method, path, version;
    ss >> method >> path >> version;

    if (path == "/api/hello") {
        return buildJsonResponse("{\"message\":\"Hello from C++ Server!\",\"status\":\"ok\"}");
    }
    else if (path == "/api/time") {
        time_t now = time(nullptr);
        char buf[64];
        struct tm tmInfo;
        localtime_s(&tmInfo, &now);
        strftime(buf, sizeof(buf), "%Y-%m-%d %H:%M:%S", &tmInfo);
        std::string body = "{\"time\":\"" + std::string(buf) + "\"}";
        return buildJsonResponse(body);
    }
    else if (path == "/api/info") {
        std::string body =
            "{\"server\":\"demo_api1\","
            "\"version\":\"1.0\","
            "\"routes\":[\"/api/hello\",\"/api/time\",\"/api/info\"]}";
        return buildJsonResponse(body);
    }
    return build404Response();
}

void runHttpServerDemo() {
    const int PORT = 8080;
    std::cout << "\n=== HTTP Server Demo ===\n";

    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        std::cout << "WSAStartup 失敗\n"; return;
    }

    SOCKET listenSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (listenSock == INVALID_SOCKET) {
        std::cout << "socket 建立失敗\n"; WSACleanup(); return;
    }

    int opt = 1;
    setsockopt(listenSock, SOL_SOCKET, SO_REUSEADDR, (const char*)&opt, sizeof(opt));

    sockaddr_in addr{};
    addr.sin_family = AF_INET;
    addr.sin_addr.s_addr = INADDR_ANY;
    addr.sin_port = htons(PORT);

    if (bind(listenSock, (sockaddr*)&addr, sizeof(addr)) == SOCKET_ERROR) {
        std::cout << "bind 失敗，錯誤碼: " << WSAGetLastError() << "\n";
        closesocket(listenSock); WSACleanup(); return;
    }

    listen(listenSock, SOMAXCONN);
    std::cout << "伺服器已啟動: http://localhost:" << PORT << "\n";
    std::cout << "可用路由:\n";
    std::cout << "  GET http://localhost:" << PORT << "/api/hello\n";
    std::cout << "  GET http://localhost:" << PORT << "/api/time\n";
    std::cout << "  GET http://localhost:" << PORT << "/api/info\n";
    std::cout << "按 Ctrl+C 停止伺服器\n\n";

    while (true) {
        sockaddr_in clientAddr{};
        int clientAddrLen = sizeof(clientAddr);
        SOCKET clientSock = accept(listenSock, (sockaddr*)&clientAddr, &clientAddrLen);
        if (clientSock == INVALID_SOCKET) continue;

        char ipBuf[INET_ADDRSTRLEN];
        inet_ntop(AF_INET, &clientAddr.sin_addr, ipBuf, sizeof(ipBuf));

        char buf[4096] = {};
        int recvLen = recv(clientSock, buf, sizeof(buf) - 1, 0);
        if (recvLen > 0) {
            std::string request(buf, recvLen);
            std::string firstLine = request.substr(0, request.find("\r\n"));
            std::cout << "[" << ipBuf << "] " << firstLine << "\n";

            std::string response = handleRequest(request);
            send(clientSock, response.c_str(), (int)response.size(), 0);
        }
        closesocket(clientSock);
    }

    closesocket(listenSock);
    WSACleanup();
}
