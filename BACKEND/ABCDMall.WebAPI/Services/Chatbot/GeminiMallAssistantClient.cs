using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ABCDMall.Modules.Users.Application.Common;

namespace ABCDMall.WebAPI.Services.Chatbot;

public sealed class GeminiMallAssistantClient : IGeminiMallAssistantClient
{
    private const string GeminiModelPath = "v1beta/models/gemini-2.5-flash:generateContent";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiMallAssistantClient> _logger;

    public GeminiMallAssistantClient(IHttpClientFactory httpClientFactory, ILogger<GeminiMallAssistantClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ApplicationResult<string>> GenerateContentAsync(
        string systemInstruction,
        string userContent,
        CancellationToken cancellationToken = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ApplicationResult<string>.BadRequest("Gemini API key is not configured.");
        }

        var url =
            $"https://generativelanguage.googleapis.com/{GeminiModelPath}?key={Uri.EscapeDataString(apiKey.Trim())}";

        var body = new GeminiGenerateContentRequest
        {
            SystemInstruction = new GeminiContent
            {
                Parts = [new GeminiPart { Text = systemInstruction }]
            },
            Contents =
            [
                new GeminiContent
                {
                    Role = "user",
                    Parts = [new GeminiPart { Text = userContent }]
                }
            ]
        };

        var client = _httpClientFactory.CreateClient("Gemini");
        using var response = await client.PostAsJsonAsync(url, body, JsonOptions, cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini API error {Status}: {Body}", (int)response.StatusCode, raw);
            var errMsg = TryParseErrorMessage(raw) ?? "The assistant could not reach Gemini right now.";
            return ApplicationResult<string>.BadRequest(errMsg);
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return ApplicationResult<string>.BadRequest("Gemini returned no candidates for this request.");
            }

            var first = candidates[0];
            if (first.TryGetProperty("finishReason", out var fr) && fr.GetString() == "SAFETY")
            {
                return ApplicationResult<string>.BadRequest("Response was blocked by safety filters.");
            }

            if (!first.TryGetProperty("content", out var content)
                || !content.TryGetProperty("parts", out var parts)
                || parts.GetArrayLength() == 0)
            {
                return ApplicationResult<string>.BadRequest("Gemini returned an empty response.");
            }

            var text = parts[0].TryGetProperty("text", out var t) ? t.GetString() : null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return ApplicationResult<string>.BadRequest("Gemini returned no text.");
            }

            return ApplicationResult<string>.Ok(text.Trim());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response.");
            return ApplicationResult<string>.BadRequest("Failed to parse Gemini response.");
        }
    }

    private static string? TryParseErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var err)
                && err.TryGetProperty("message", out var msg))
            {
                return msg.GetString();
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private sealed class GeminiGenerateContentRequest
    {
        public GeminiContent? SystemInstruction { get; set; }
        public IList<GeminiContent> Contents { get; set; } = [];
    }

    private sealed class GeminiContent
    {
        public string? Role { get; set; }
        public IList<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiPart
    {
        public string Text { get; set; } = string.Empty;
    }
}
