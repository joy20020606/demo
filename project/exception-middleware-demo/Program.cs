using ExceptionMiddlewareDemo.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 例外攔截必須在 pipeline 最外層,才能包住後續所有 Middleware 與 Controller 的執行;
// 若註冊在 MapControllers() 之後,請求早已被端點處理,將完全不生效
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
