using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.WebAPI.Contracts.Chatbot;
using ABCDMall.WebAPI.Services.Chatbot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotAskService _chatbotAskService;

    public ChatbotController(IChatbotAskService chatbotAskService)
    {
        _chatbotAskService = chatbotAskService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatbotAskRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _chatbotAskService.AskAsync(dto, cancellationToken);
        return result.Status switch
        {
            ApplicationResultStatus.Ok => Ok(new { answer = result.Value!.Answer }),
            ApplicationResultStatus.BadRequest => BadRequest(result.Error),
            ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
            ApplicationResultStatus.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
