#include "feature7_simple_vector.h"
#include <iostream>
#include <stdexcept>

// ============================================================
// 動態陣列 SimpleVector
// 模擬 std::vector 的核心行為：自動擴充、深拷貝
// ============================================================

class SimpleVector
{
private:
    int* data;      // 指向堆積（Heap）上配置的整數陣列
    int  size;      // 目前存了幾個元素
    int  capacity;  // 陣列總共可以放幾個元素

public:
    // ---- 建構子 ----
    // 初始配置容量為 4 的空陣列
    SimpleVector()
        : size(0), capacity(4)
    {
        data = new int[capacity]; // 在 Heap 上配置記憶體
    }

    // ---- 複製建構子（深拷貝）----
    // 當你寫 SimpleVector b = a; 時會呼叫這個
    // 深拷貝：另外配置一塊新記憶體，把內容複製過去
    // 若不自己實作，預設只會做「淺拷貝」（只複製指標位址），
    // 導致兩個物件的 data 指向同一塊記憶體，解構時會 double-free
    SimpleVector(const SimpleVector& other)
        : size(other.size), capacity(other.capacity)
    {
        data = new int[capacity]; // 重新配置一塊新記憶體
        for (int i = 0; i < size; i++)
        {
            data[i] = other.data[i]; // 逐一複製每個元素
        }
    }

    // ---- 解構子 ----
    // 當物件離開作用域時自動呼叫，釋放 Heap 上的記憶體
    // 若不手動 delete，會造成記憶體洩漏（Memory Leak）
    ~SimpleVector()
    {
        delete[] data; // 釋放陣列記憶體（注意是 delete[]，不是 delete）
    }

    // ---- 新增元素到尾端 ----
    void push_back(int val)
    {
        // 如果空間滿了，就擴充為原來的兩倍
        if (size == capacity)
        {
            capacity *= 2;
            int* newData = new int[capacity]; // 配置更大的新陣列

            // 把舊陣列的資料搬到新陣列
            for (int i = 0; i < size; i++)
            {
                newData[i] = data[i];
            }

            delete[] data;  // 釋放舊陣列
            data = newData; // data 指向新陣列
        }

        data[size] = val; // 把新值放到最後
        size++;
    }

    // ---- 取得指定位置的元素 ----
    int get(int index) const
    {
        if (index < 0 || index >= size)
            throw std::out_of_range("索引超出範圍");
        return data[index];
    }

    int getSize()     const { return size; }
    int getCapacity() const { return capacity; }

    // ---- 印出所有元素 ----
    void print() const
    {
        std::cout << "  [";
        for (int i = 0; i < size; i++)
        {
            std::cout << data[i];
            if (i < size - 1) std::cout << ", ";
        }
        std::cout << "]  size=" << size << "  capacity=" << capacity << "\n";
    }
};

// ---- 示範入口 ----
void runSimpleVector()
{
    std::cout << "\n============================\n";
    std::cout << "  動態陣列練習\n";
    std::cout << "============================\n";

    SimpleVector v;

    std::cout << "\n依序加入 1~8：\n";
    for (int i = 1; i <= 8; i++)
    {
        v.push_back(i);
        std::cout << "  push_back(" << i << ") -> ";
        v.print();
    }

    std::cout << "\n--- 深拷貝測試 ---\n";
    SimpleVector v2 = v; // 呼叫複製建構子（深拷貝）
    std::cout << "原始 v:  "; v.print();
    std::cout << "拷貝 v2: "; v2.print();

    std::cout << "\n修改 v2（加入 999），確認 v 不受影響：\n";
    v2.push_back(999);
    std::cout << "原始 v:  "; v.print();
    std::cout << "拷貝 v2: "; v2.print();
}
