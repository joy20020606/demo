"""SQLite 初始化與種子資料。

Phase 2:加入 notes 與 secrets(含 flag),讓 SQLi 有更多可挖的東西。
"""
import sqlite3
import os

DB_PATH = os.path.join(os.path.dirname(__file__), "lab.db")

# 打穿任一漏洞後要拿到的旗標
FLAG = "FLAG{web_vuln_lab_pwned}"


def get_conn():
    conn = sqlite3.connect(DB_PATH)
    conn.row_factory = sqlite3.Row
    return conn


def init_db():
    """每次啟動時重建資料庫,確保靶機狀態乾淨可重現。"""
    if os.path.exists(DB_PATH):
        os.remove(DB_PATH)

    conn = get_conn()
    cur = conn.cursor()

    cur.execute(
        """
        CREATE TABLE users (
            id       INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT NOT NULL UNIQUE,
            password TEXT NOT NULL,
            email    TEXT,
            role     TEXT NOT NULL DEFAULT 'user'
        )
        """
    )
    cur.executemany(
        "INSERT INTO users (username, password, email, role) VALUES (?, ?, ?, ?)",
        [
            ("admin", "password123", "admin@lab.local", "admin"),
            ("alice", "alice123", "alice@lab.local", "user"),
            ("bob", "bob123", "bob@lab.local", "user"),
        ],
    )

    # 給 /search 當查詢目標的公開筆記
    cur.execute(
        "CREATE TABLE notes (id INTEGER PRIMARY KEY, title TEXT, body TEXT)"
    )
    cur.executemany(
        "INSERT INTO notes (title, body) VALUES (?, ?)",
        [
            ("welcome", "歡迎來到弱點靶機"),
            ("todo", "記得把 debug 關掉"),
            ("note", "這裡沒有秘密,秘密在別的 table"),
        ],
    )

    # 秘密資料(SQLi 的進階目標)
    cur.execute("CREATE TABLE secrets (id INTEGER PRIMARY KEY, name TEXT, value TEXT)")
    cur.executemany(
        "INSERT INTO secrets (name, value) VALUES (?, ?)",
        [("flag", FLAG), ("api_key", "sk-lab-DO-NOT-USE-1234567890")],
    )

    conn.commit()
    conn.close()


if __name__ == "__main__":
    init_db()
    print(f"DB initialized at {DB_PATH}")
