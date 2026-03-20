#include "feature9_shapes.h"
#include <iostream>
#include <vector>
#include <cmath>

// ============================================================
// 形狀多型 (Shape Polymorphism)
// 重點：繼承 (Inheritance) 與 多型 (Polymorphism)
//
// 繼承：子類別（Circle、Rectangle）繼承基底類別（Shape）的介面
// 多型：透過基底類別的指標 Shape*，呼叫各自不同的 calculateArea()
//       執行時才決定要呼叫哪個版本（動態分派）
// ============================================================

// ---- 基底類別（抽象類別）----
// 含有純虛擬函式（pure virtual function），無法直接建立實體
class Shape
{
public:
    // 純虛擬函式：= 0 表示「子類別必須自己實作這個函式」
    // 這讓 Shape 成為「抽象類別」，強制子類別都要提供面積計算
    virtual double calculateArea() const = 0;

    // 虛擬函式：子類別可以選擇是否覆寫
    // virtual 讓程式在執行時透過虛擬函式表（vtable）找到正確的版本
    virtual void print() const
    {
        std::cout << "  形狀，面積 = " << calculateArea() << "\n";
    }

    // 虛擬解構子：當透過基底指標刪除子物件時，
    // 確保子類別的解構子也被呼叫，避免記憶體洩漏
    virtual ~Shape() {}
};

// ---- 子類別：圓形 ----
class Circle : public Shape // public 繼承：Shape 的 public 成員保持 public
{
private:
    double radius; // 半徑

public:
    Circle(double r) : radius(r) {}

    // override 關鍵字：明確告訴編譯器這是覆寫父類別的虛擬函式
    // 若拼錯函式名稱，編譯器會報錯（保護機制）
    double calculateArea() const override
    {
        return 3.14159265358979 * radius * radius; // π * r²
    }

    void print() const override
    {
        std::cout << "  圓形（半徑=" << radius << ") ，面積 = " << calculateArea() << "\n";
    }
};

// ---- 子類別：矩形 ----
class Rectangle : public Shape
{
private:
    double width;  // 寬
    double height; // 高

public:
    Rectangle(double w, double h) : width(w), height(h) {}

    double calculateArea() const override
    {
        return width * height; // 寬 * 高
    }

    void print() const override
    {
        std::cout << "  矩形（" << width << " x " << height
                  << "），面積 = " << calculateArea() << "\n";
    }
};

// ---- 示範入口 ----
void runShapes()
{
    std::cout << "\n============================\n";
    std::cout << "  形狀多型練習\n";
    std::cout << "============================\n";

    // 用基底類別指標（Shape*）存放不同子類別物件
    // 這就是多型的精髓：統一介面，不同行為
    std::vector<Shape*> shapes;
    shapes.push_back(new Circle(5.0));
    shapes.push_back(new Circle(3.0));
    shapes.push_back(new Rectangle(4.0, 6.0));
    shapes.push_back(new Rectangle(2.5, 8.0));

    std::cout << "\n透過 Shape* 呼叫各自的 calculateArea()：\n";
    double total = 0;
    for (Shape* s : shapes)
    {
        s->print();         // 執行時動態決定呼叫哪個 print()
        total += s->calculateArea();
    }
    std::cout << "\n  所有形狀面積總和 = " << total << "\n";

    // 釋放 Heap 上配置的物件
    for (Shape* s : shapes)
    {
        delete s; // 因為解構子是 virtual，會正確呼叫子類別的解構子
    }
}
