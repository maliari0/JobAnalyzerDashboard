using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class N8nTestController : ControllerBase
    {
        private readonly ILogger<N8nTestController> _logger;

        public N8nTestController(ILogger<N8nTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "online",
                message = "JobAnalyzerDashboard API is ready for n8n integration",
                timestamp = DateTime.Now,
                version = "1.0.0"
            });
        }

        [HttpPost("echo")]
        public IActionResult Echo([FromBody] JsonElement data)
        {
            _logger.LogInformation("Received data from n8n: {data}", data.ToString());
            
            return Ok(new
            {
                received = true,
                data = data,
                timestamp = DateTime.Now,
                message = "Data successfully received and echoed back"
            });
        }
    }
}
