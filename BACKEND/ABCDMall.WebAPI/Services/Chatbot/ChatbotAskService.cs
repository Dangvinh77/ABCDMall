using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.WebAPI.Contracts.Chatbot;

namespace ABCDMall.WebAPI.Services.Chatbot;

public sealed class ChatbotAskService : IChatbotAskService
{
    private const string SystemInstruction = """
You are the official ABCD Mall digital assistant.

Rules:
- Answer only using the mall reference data provided in the user message (shops, products, vouchers, events) and general mall navigation.
- If the visitor asks about topics unrelated to ABCD Mall, politely refuse and steer them back to mall topics.
- Be concise, friendly, and professional. Use English unless the visitor writes in another language; then reply in that language.
- When recommending a shop, event, product, or voucher, include a machine-readable link token exactly in these forms so the website can render buttons:
  - Shop detail page: [link:shop:SHOP_SLUG] where SHOP_SLUG is the slug from the data (lowercase as given).
  - All shops listing: [link:shops]
  - Events listing: [link:events]
- Do not invent slugs, discounts, or events that are not in the reference data.
- If data is missing, say you do not have that information in the mall directory yet.
""";

    private readonly IMallRagContextProvider _ragContextProvider;
    private readonly IGeminiMallAssistantClient _geminiClient;

    public ChatbotAskService(IMallRagContextProvider ragContextProvider, IGeminiMallAssistantClient geminiClient)
    {
        _ragContextProvider = ragContextProvider;
        _geminiClient = geminiClient;
    }

    public async Task<ApplicationResult<ChatbotAskResponseDto>> AskAsync(
        ChatbotAskRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return ApplicationResult<ChatbotAskResponseDto>.BadRequest("Message is required.");
        }

        var context = await _ragContextProvider.GetContextTextAsync(cancellationToken);
        var userContent =
            $"""
=== Mall reference data (authoritative; may be truncated) ===
{context}
=== End reference data ===

Visitor message:
{request.Message.Trim()}
""";

        var gemini = await _geminiClient.GenerateContentAsync(SystemInstruction, userContent, cancellationToken);
        if (!gemini.Succeeded || gemini.Value is null)
        {
            return ApplicationResult<ChatbotAskResponseDto>.BadRequest(
                gemini.Error ?? "Assistant request failed.");
        }

        return ApplicationResult<ChatbotAskResponseDto>.Ok(new ChatbotAskResponseDto { Answer = gemini.Value });
    }
}
