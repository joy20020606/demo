# app/api/users.py
from fastapi import APIRouter

router = APIRouter(prefix="/hello", tags=["hello"])   # ≈ [Route("users")] + Swagger 分組

@router.get("/{user_id}")          # 注意是 @router 不是 @app
def get_hello(user_id: str):
    return {"hello": user_id}