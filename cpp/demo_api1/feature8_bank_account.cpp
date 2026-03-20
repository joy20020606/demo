#include "feature8_bank_account.h"
#include <iostream>
#include <string>
#include <vector>

// ============================================================
// 銀行帳戶系統 (Bank Account)
// 重點：封裝 (Encapsulation)
// - private：外部無法直接存取餘額，只能透過公有方法操作
// - public：提供存款、提款、查詢等介面
// ============================================================

class BankAccount
{
private:
    // ---- 私有成員變數 ----
    // 外部程式碼無法直接讀寫這些變數，確保資料安全
    std::string owner;   // 帳戶擁有者
    double      balance; // 餘額（私有，只能透過方法修改）

    // 交易紀錄
    struct Transaction
    {
        std::string type;
        double      amount;
        double      balanceAfter;
    };
    std::vector<Transaction> history;

public:
    // ---- 建構子 ----
    BankAccount(std::string name, double initBalance)
        : owner(name), balance(initBalance)
    {
        if (initBalance < 0)
        {
            std::cout << "初始金額不可為負數，設為 0\n";
            balance = 0;
        }
    }

    // ---- 存款 ----
    void deposit(double amount)
    {
        if (amount <= 0)
        {
            std::cout << "[錯誤] 存款金額必須大於 0\n";
            return;
        }
        balance += amount;
        history.push_back({"存款", amount, balance});
        std::cout << "存款 " << amount << " 成功，目前餘額: " << balance << "\n";
    }

    // ---- 提款 ----
    // 回傳 true = 提款成功，false = 餘額不足
    bool withdraw(double amount)
    {
        if (amount <= 0)
        {
            std::cout << "[錯誤] 提款金額必須大於 0\n";
            return false;
        }
        if (amount > balance)
        {
            // 餘額不足，拒絕提款
            std::cout << "[錯誤] 餘額不足！目前餘額: " << balance
                      << "，嘗試提款: " << amount << "\n";
            return false;
        }
        balance -= amount;
        history.push_back({"提款", amount, balance});
        std::cout << "提款 " << amount << " 成功，目前餘額: " << balance << "\n";
        return true;
    }

    // ---- 查詢餘額 ----
    // 提供唯讀存取，外部只能查不能改
    double getBalance() const { return balance; }

    // ---- 印出交易明細 ----
    void printStatement() const
    {
        std::cout << "\n--- " << owner << " 的帳戶明細 ---\n";
        if (history.empty())
        {
            std::cout << "  （無交易紀錄）\n";
        }
        for (const auto& t : history)
        {
            std::cout << "  [" << t.type << "] " << t.amount
                      << "  ->  餘額: " << t.balanceAfter << "\n";
        }
        std::cout << "  目前餘額: " << balance << "\n";
    }
};

// ---- 示範入口 ----
void runBankAccount()
{
    std::cout << "\n============================\n";
    std::cout << "  銀行帳戶系統練習\n";
    std::cout << "============================\n";

    BankAccount acc("王小明", 1000.0);
    std::cout << "開戶成功，初始餘額: " << acc.getBalance() << "\n\n";

    acc.deposit(500);
    acc.deposit(200);
    acc.withdraw(300);
    acc.withdraw(2000); // 餘額不足，會失敗
    acc.withdraw(-50);  // 非法金額，會失敗

    acc.printStatement();
}
