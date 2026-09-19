from pydantic import BaseModel, field_validator, model_validator, ValidationError

class UserCreate(BaseModel):
    name: str
    password: str
    password_confirm: str

    @field_validator("name")
    @classmethod
    def no_admin(cls, v):
        print(f"  [field] 檢查 name,拿到 v = {v!r}")
        if "admin" in v.lower():
            raise ValueError("name 不能含 admin")
        return v.strip()

    @model_validator(mode="after")
    def passwords_match(self):
        print(f"  [model] 檢查整個物件,name = {self.name!r}")
        if self.password != self.password_confirm:
            raise ValueError("兩次密碼不一致")
        return self

for data in [
    {"name": "  Joy ", "password": "a", "password_confirm": "a"},
    {"name": "Joy", "password": "a", "password_confirm": "b"},
    {"name": "SuperAdmin", "password": "a", "password_confirm": "b"},
]:
    print(f"\n輸入: {data}")
    try:
        u = UserCreate(**data)          # ** = 把 dict 展開成具名參數 ≈ 物件初始設定式
        print(f"  成功: {u}")
    except ValidationError as e:
        print(f"  失敗: {[err['msg'] for err in e.errors()]}")