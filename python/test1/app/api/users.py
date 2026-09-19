# app/api/users.py
from fastapi import APIRouter,Depends
from app.schemas import UserCreate, UserOut
from app.deps import get_db, get_current_user


router = APIRouter(prefix="/users", tags=["users"])   # ≈ [Route("users")] + Swagger 分組

@router.get("")
def list_users(db = Depends(get_db)):
    print("② 查詢中")
    return db["users"]

@router.get("/me")
def me(db = Depends(get_db), user = Depends(get_current_user)):
    print("② 處理中")
    return user

@router.get("/{user_id}")          # 注意是 @router 不是 @app
def get_user(user_id: int):
    return {"id": user_id}



@router.post("", response_model=UserOut, status_code=201)   # ≈ [HttpPost] + [ProducesResponseType(201)]
def create_user(body: UserCreate):                           # ≈ ([FromBody] UserCreateDto body)
    return {"id": 1, "name": body.name, "age": body.age, "email": body.email}

