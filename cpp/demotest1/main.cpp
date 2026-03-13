#include <iostream>

// ============================================================
// 練習 1：動態陣列 new[] / delete[]
// ============================================================

void demo_dynamic_array() {
    std::cout << "=== 練習 1：動態陣列 ===" << std::endl;

    int size = 5;
    int* arr = new int[size];  // 在 heap 配置陣列

    for (int i = 0; i < size; i++)
    {
        arr[i] = (i + 1) * 10;  // 10, 20, 30, 40, 50
    }   

    
    std::cout << "陣列內容：";
    for (int i = 0; i < size; i++)
        std::cout << arr[i] << " ";
    std::cout << std::endl;
  
    delete[] arr;  // 必須用 delete[]，不能用 delete
    arr = nullptr; // 避免 dangling pointer

    std::cout << "記憶體已釋放" << std::endl;
}

// ============================================================
// 練習 2：指標算術
// ============================================================
void demo_pointer_arithmetic() {
    std::cout << "\n=== 練習 2：指標算術 ===" << std::endl;

    int arr[] = {100, 200, 300, 400, 500};
    int* p = arr;

    std::cout << "用指標遍歷：";
    for (int i = 0; i < 5; i++) {
        std::cout << *p << " ";
        p++;  // 位址 +4 bytes (sizeof int)
    }
    std::cout << std::endl;

    p = arr;
    std::cout << "arr[2] 位址：" << (arr + 2) << "  值：" << *(arr + 2) << std::endl;
    std::cout << "arr[0] ：" << (arr) << "  值：" << *arr << std::endl;
    std::cout << "arr[0]+2 ：" << (arr ) << "  值：" << (*arr)+2 << std::endl;
    std::cout << "偏移量：" << (arr + 2) - arr << " 個元素" << std::endl;
}

// ============================================================
// 練習 3：傳值 vs 傳指標 vs 傳參考
// ============================================================
void by_value(int x) {
    x = 999;  // 只改 local copy，外部不受影響
}

void by_pointer(int* x) {
    *x = 999;  // 透過指標修改原始值
}

void by_reference(int& x) {
    x = 999;  // 參考語法較乾淨，效果同 pointer
}

void demo_pass_types() {
    std::cout << "\n=== 練習 3：傳址 vs 傳值 ===" << std::endl;

    int a = 1, b = 1, c = 1;

    by_value(a);
    std::cout << "傳值後   a = " << a << "（未改變）" << std::endl;

    by_pointer(&b);
    std::cout << "傳指標後 b = " << b << "（已改變）" << std::endl;

    by_reference(c);
    std::cout << "傳參考後 c = " << c << "（已改變）" << std::endl;
}

// ============================================================
int main() {
 
    demo_dynamic_array();
    demo_pointer_arithmetic();
    demo_pass_types();
    return 0;
}
