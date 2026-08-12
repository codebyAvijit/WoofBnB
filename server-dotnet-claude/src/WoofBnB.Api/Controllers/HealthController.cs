using Microsoft.AspNetCore.Mvc;

namespace WoofBnB.Api.Controllers;

/// <summary>
/// GET /api/health intentionally does NOT use the ApiResponse&lt;T&gt; envelope. Node's
/// implementation (server/src/app.js:33-38) returns a bare object with only two properties
/// and no statusCode/data/timestamp:
///   res.status(200).json({ success: true, message: "WoofBnB API is running" });
/// This is reproduced verbatim rather than assumed to follow the standard contract.
/// </summary>
[ApiController]
public sealed class HealthController : ControllerBase
{
    [HttpGet("/api/health")]
    public IActionResult GetApiHealth()
    {
        return Ok(new ApiHealthStatus());
    }

    private sealed class ApiHealthStatus
    {
        public bool Success { get; } = true;

        public string Message { get; } = "WoofBnB API is running";
    }
}
