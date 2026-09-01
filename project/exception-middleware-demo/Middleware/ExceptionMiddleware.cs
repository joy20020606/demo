using System.Text.Encodings.Web;
using System.Text.Json;

namespace ExceptionMiddlewareDemo.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    // 序列化選項只建一次重複使用:camelCase 命名 + 中文不被 escape 成 \uXXXX
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // 只包住「下游流程丟出的例外」;Controller 已自行處理並正常回傳的錯誤
            // (如 BadRequest) 不會進到 catch,自然滿足 R5,不需要也不可用狀態碼判斷
            await _next(context);
        }
        catch (Exception)
        {
            // 契約為固定值(R2):刻意不使用 ex.Message / StackTrace,避免內部細節外洩
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // 寫入 Body 前明確設定 Content-Type
            context.Response.ContentType = "application/json";

            var body = new
            {
                StatusCode = 500,
                Error = "Internal Server Error",
                Message = "系統異常,請稍後再試"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(body, SerializerOptions));
        }
    }
}
