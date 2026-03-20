#include "feature11_linked_list.h"
#include <iostream>

// ============================================================
// 雙向鏈結串列 (Doubly Linked List)
// 每個節點有 prev（前一個）和 next（後一個）兩個指標
//
//  NULL <- [A] <-> [B] <-> [C] -> NULL
//           ^                ^
//          head             tail
// ============================================================

// ---- 節點結構 ----
struct Node
{
    int   data; // 存放的資料
    Node* prev; // 指向前一個節點
    Node* next; // 指向後一個節點

    // 建構子：初始化資料，前後指標設為 nullptr
    Node(int val) : data(val), prev(nullptr), next(nullptr) {}
};

// ---- 雙向鏈結串列類別 ----
class DoublyLinkedList
{
private:
    Node* head; // 指向第一個節點
    Node* tail; // 指向最後一個節點

public:
    DoublyLinkedList() : head(nullptr), tail(nullptr) {}

    // ---- 解構子：釋放所有節點 ----
    ~DoublyLinkedList()
    {
        Node* curr = head;
        while (curr != nullptr)
        {
            Node* next = curr->next;
            delete curr;
            curr = next;
        }
    }

    // ---- 從頭部插入 ----
    void insertFront(int val)
    {
        Node* newNode = new Node(val);

        if (head == nullptr)
        {
            // 串列是空的，head 和 tail 都指向新節點
            head = tail = newNode;
        }
        else
        {
            newNode->next = head; // 新節點的 next 指向原本的 head
            head->prev = newNode; // 原本 head 的 prev 指向新節點
            head = newNode;       // 更新 head 為新節點
        }
    }

    // ---- 從尾部插入 ----
    void insertBack(int val)
    {
        Node* newNode = new Node(val);

        if (tail == nullptr)
        {
            head = tail = newNode;
        }
        else
        {
            tail->next = newNode; // 原本 tail 的 next 指向新節點
            newNode->prev = tail; // 新節點的 prev 指向原本的 tail
            tail = newNode;       // 更新 tail 為新節點
        }
    }

    // ---- 刪除第一個值等於 val 的節點 ----
    void deleteNode(int val)
    {
        Node* curr = head;

        // 搜尋目標節點
        while (curr != nullptr && curr->data != val)
        {
            curr = curr->next;
        }

        if (curr == nullptr)
        {
            std::cout << "  找不到值 " << val << "\n";
            return;
        }

        // 把前後節點「繞過」curr 相連
        if (curr->prev != nullptr)
            curr->prev->next = curr->next; // 前一個的 next 跳過 curr
        else
            head = curr->next; // curr 是 head，更新 head

        if (curr->next != nullptr)
            curr->next->prev = curr->prev; // 後一個的 prev 跳過 curr
        else
            tail = curr->prev; // curr 是 tail，更新 tail

        delete curr; // 釋放記憶體
        std::cout << "  已刪除節點 " << val << "\n";
    }

    // ---- 反轉整個串列 ----
    // 把每個節點的 prev 和 next 交換，最後再交換 head 和 tail
    void reverse()
    {
        Node* curr = head;

        while (curr != nullptr)
        {
            // 交換 prev 和 next
            Node* temp = curr->prev;
            curr->prev = curr->next;
            curr->next = temp;

            // 往「舊的 next」移動（現在是 curr->prev）
            curr = curr->prev;
        }

        // 交換 head 和 tail
        Node* temp = head;
        head = tail;
        tail = temp;
    }

    // ---- 從頭到尾印出 ----
    void print() const
    {
        std::cout << "  HEAD <-> ";
        Node* curr = head;
        while (curr != nullptr)
        {
            std::cout << curr->data;
            if (curr->next != nullptr) std::cout << " <-> ";
            curr = curr->next;
        }
        std::cout << " <-> NULL\n";
    }
};

// ---- 示範入口 ----
void runLinkedList()
{
    std::cout << "\n============================\n";
    std::cout << "  雙向鏈結串列練習\n";
    std::cout << "============================\n";

    DoublyLinkedList list;

    std::cout << "\n從尾部插入 1, 2, 3, 4, 5：\n";
    for (int i = 1; i <= 5; i++) list.insertBack(i);
    list.print();

    std::cout << "\n從頭部插入 0：\n";
    list.insertFront(0);
    list.print();

    std::cout << "\n刪除節點 3：\n";
    list.deleteNode(3);
    list.print();

    std::cout << "\n刪除不存在的節點 99：\n";
    list.deleteNode(99);

    std::cout << "\n反轉串列：\n";
    list.reverse();
    list.print();
}
