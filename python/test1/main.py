from fastapi import FastAPI
from app.api import users,hello

app = FastAPI()       
app.include_router(users.router)           # ≈ var app = builder.Build();
app.include_router(hello.router)  
