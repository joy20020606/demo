from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordRequestForm
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from pydantic import BaseModel, EmailStr

from app.core.database import get_db
from app.core.security import verify_password, hash_password, create_token
from app.core.deps import get_current_user
from app.models import User, Tenant

router = APIRouter(prefix="/api/auth", tags=["auth"])


class RegisterRequest(BaseModel):
    email: EmailStr
    password: str
    tenant_name: str


class TokenResponse(BaseModel):
    access_token: str
    token_type: str = "bearer"


@router.post("/register", response_model=TokenResponse)
async def register(req: RegisterRequest, db: AsyncSession = Depends(get_db)):
    """註冊:建立 tenant + user,回傳 JWT"""
    # 檢查 email 是否已存在
    result = await db.execute(select(User).where(User.email == req.email))
    if result.scalar_one_or_none():
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Email 已註冊")

    # 建立 tenant
    tenant = Tenant(name=req.tenant_name)
    db.add(tenant)
    await db.flush()  # 拿到 tenant.id

    # 建立 user
    user = User(
        tenant_id=tenant.id,
        email=req.email,
        password_hash=hash_password(req.password),
    )
    db.add(user)
    await db.commit()

    token = create_token(str(user.id), str(tenant.id), user.email)
    return TokenResponse(access_token=token)


@router.post("/login", response_model=TokenResponse)
async def login(form: OAuth2PasswordRequestForm = Depends(), db: AsyncSession = Depends(get_db)):
    """登入:OAuth2 form 格式(username = email)"""
    result = await db.execute(select(User).where(User.email == form.username))
    user = result.scalar_one_or_none()
    if not user or not verify_password(form.password, user.password_hash):
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "帳號或密碼錯誤")

    token = create_token(str(user.id), str(user.tenant_id), user.email)
    return TokenResponse(access_token=token)


@router.get("/me")
async def me(current=Depends(get_current_user)):
    """取得當前登入者"""
    return current
