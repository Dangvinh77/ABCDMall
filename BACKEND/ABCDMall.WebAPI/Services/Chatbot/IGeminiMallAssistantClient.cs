using ABCDMall.Modules.Users.Application.Common;

namespace ABCDMall.WebAPI.Services.Chatbot;

public interface IGeminiMallAssistantClient
{
    Task<ApplicationResult<string>> GenerateContentAsync(
        string systemInstruction,
        string userContent,
        CancellationToken cancellationToken = default);
}
