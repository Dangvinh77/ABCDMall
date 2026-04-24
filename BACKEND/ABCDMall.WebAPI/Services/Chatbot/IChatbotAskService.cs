using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.WebAPI.Contracts.Chatbot;

namespace ABCDMall.WebAPI.Services.Chatbot;

public interface IChatbotAskService
{
    Task<ApplicationResult<ChatbotAskResponseDto>> AskAsync(
        ChatbotAskRequestDto request,
        CancellationToken cancellationToken = default);
}
