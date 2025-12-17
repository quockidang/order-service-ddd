using Microsoft.AspNetCore.Mvc;
using Ordering.API;

namespace Ordering.API.Controllers;
[ApiController]
[Route("api/kafka")]
public class KafkaController : ControllerBase
{
    private readonly IKafkaProducerService _producer;

    public KafkaController(IKafkaProducerService producer)
    {
        _producer = producer;
    }

    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromBody] string message)
    {
        await _producer.ProduceAsync(message);
        return Ok(new { status = "sent", message });
    }
}
