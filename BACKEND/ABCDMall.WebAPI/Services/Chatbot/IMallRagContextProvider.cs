namespace ABCDMall.WebAPI.Services.Chatbot;

public interface IMallRagContextProvider
{
    Task<string> GetContextTextAsync(CancellationToken cancellationToken = default);
}
