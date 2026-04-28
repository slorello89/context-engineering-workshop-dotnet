using Microsoft.AspNetCore.Mvc;
using BackendDotnetLayer.Services;

namespace BackendDotnetLayer.Controllers;

[ApiController]
public sealed class ChatController : ControllerBase
{
    private readonly OpenAiChatService _chatService;

    public ChatController(OpenAiChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("/ai/chat/string")]
    [Produces("text/plain")]
    public async Task<ActionResult<string>> Chat([FromQuery] string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("The `query` parameter is required.");
        }

        try
        {
            var response = await _chatService.ChatAsync(query, cancellationToken);
            return Content(response, "text/plain");
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, exception.Message);
        }
    }
}
