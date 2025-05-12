namespace VolatilityMonitor.Services;

public class TelegramNotifier
{
    private readonly string _token;
    private readonly string _chatId;
    
    public TelegramNotifier(string token, string chatId)
    {
        _token = token;
        _chatId = chatId;
    }

    public async Task SendAsync(string message)
    {
        using var client = new HttpClient();
        var url = $"https://api.telegram.org/bot{_token}/sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(message)}";
        await client.GetAsync(url);
    }
    
}
