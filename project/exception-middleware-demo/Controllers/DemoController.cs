using Microsoft.AspNetCore.Mvc;

namespace ExceptionMiddlewareDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    // 情境一:未處理例外 → 一路往外拋,由 ExceptionMiddleware 收斂成統一 500 契約
    [HttpGet("unhandled")]
    public IActionResult ThrowUnhandled()
    {
        throw new InvalidOperationException("模擬未處理的例外");
    }

    // 情境二:已處理錯誤 → Controller 自行決定回應,屬於正常回傳而非例外,
    // 因此不會觸發 Middleware 的 catch,格式維持自訂樣貌(R5)
    [HttpGet("handled")]
    public IActionResult HandledError()
    {
        return BadRequest(new
        {
            code = "DEMO_VALIDATION_FAILED",
            detail = "這是 Controller 自行處理的錯誤,格式不受 Middleware 影響"
        });
    }
}
