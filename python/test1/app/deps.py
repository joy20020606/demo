from fastapi import Header, HTTPException

def get_db():
    print("① 開連線")
    try:
        yield {"users": ["joy", "amy"]}
    finally:
        print("③ 關連線")

def get_current_user(x_token: str = Header()):
    print("   驗 token")
    if x_token != "secret123":
        raise HTTPException(status_code=401, detail="token 錯誤")
    return {"name": "joy"}