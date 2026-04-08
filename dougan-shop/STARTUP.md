# 豆干專賣店 - 啟動教學

## 環境需求

- Node.js 18+
- npm

## 第一次啟動

```bash
# 1. 進入專案目錄
cd dougan-shop

# 2. 安裝依賴（第一次才需要）
npm install

# 3. 建立資料庫（第一次才需要）
npx prisma migrate dev

# 4. 建立測試資料（第一次才需要）
npm run seed

# 5. 啟動開發伺服器
npm run dev
```

## 日常啟動

```bash
cd dougan-shop
npm run dev
```

開啟瀏覽器前往 http://localhost:3000

---

## 頁面一覽

| 頁面 | URL |
|------|-----|
| 前台首頁 | http://localhost:3000 |
| 商品列表 | http://localhost:3000/products |
| 購物車 | http://localhost:3000/cart |
| 結帳 | http://localhost:3000/checkout |
| 會員帳號 | http://localhost:3000/account |
| 登入 | http://localhost:3000/login |
| 註冊 | http://localhost:3000/register |
| 後台 | http://localhost:3000/admin |

---

## 測試帳號

| 角色 | Email | 密碼 |
|------|-------|------|
| 管理員 | admin@dougan.com | admin123 |
| 測試會員 | user@dougan.com | user123 |

---

## 測試完整購物流程

1. 前往 http://localhost:3000/login 用會員帳號登入
2. 點選任意商品「加入購物車」
3. 前往購物車確認品項
4. 填寫結帳資料送出訂單
5. 在「我的帳號」查看訂單狀態

## 後台管理流程

1. 前往 http://localhost:3000/login 用管理員帳號登入
2. 進入 http://localhost:3000/admin
3. 可管理商品（新增/編輯/下架）、訂單狀態、會員列表

---

## 資料庫管理（可選）

```bash
# 開啟 Prisma Studio 圖形介面查看資料
npx prisma studio
```

開啟後前往 http://localhost:5555

---

## 常見問題

**Q: 重新啟動後資料不見？**
資料存在 `dev.db` 檔案，只要沒有刪除就會保留。

**Q: 想重置資料庫？**
```bash
npx prisma migrate reset
npm run seed
```

**Q: 如何新增管理員帳號？**
在 `prisma/seed.ts` 修改後重新執行 `npm run seed`，或直接透過 Prisma Studio 修改 role 欄位。
