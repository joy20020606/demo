# app/schemas.py
from pydantic import BaseModel, Field

class UserCreate(BaseModel):                       # ≈ public class UserCreateDto
    name: str = Field(min_length=1, max_length=50) # ≈ [Required][StringLength(50, MinimumLength=1)]
    age: int = Field(ge=0, le=150)                 # ≈ [Range(0,150)]   ge = greater or equal
    email: str | None = None                       # ≈ string? Email { get; set; }  (可不傳)

class UserOut(BaseModel):                          # ≈ 回傳用的 DTO
    id: int
    name: str
    age: int