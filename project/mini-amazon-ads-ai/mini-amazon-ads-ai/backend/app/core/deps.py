from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from app.core.security import decode_token

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="/api/auth/login")


async def get_current_user(token: str = Depends(oauth2_scheme)) -> dict:
    """從 JWT 解出當前使用者(含 tenant_id)— 所有需要登入的 API 都用這個"""
    try:
        payload = decode_token(token)
        return {
            "user_id": payload["sub"],
            "tenant_id": payload["tenant_id"],
            "email": payload["email"],
        }
    except ValueError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token 無效或過期",
        )
