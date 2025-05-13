// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json.Linq;
using VolatilityMonitor;
using VolatilityMonitor.Conditions;
using VolatilityMonitor.Services;

var symbols = await GetAllUSDTTradingPairsAsync();

var conditions = new List<IAlertCondition>
{
    new PriceChangeCondition(1.0m),
    new VolumeSpikeCondition()
};
// symbols.Clear();
// symbols.Add("BTCUSDT");
var notifier = new TelegramNotifier(ConfigManager.Get(ConfigKey.TelegramBotToken), ConfigManager.Get(ConfigKey.TelegramChannelId));
var wsService = new BinanceWebSocketService(conditions, null);

var history = await wsService.LoadCandlesForSymbolsAsync(symbols);
wsService.InitializeHistory(history);

await wsService.StartAsync(symbols.Take(100).ToList());


static async Task<List<string>> GetAllUSDTTradingPairsAsync()
{
    using var client = new HttpClient();
    var response = await client.GetStringAsync("https://api.binance.com/api/v3/exchangeInfo");
    var json = JObject.Parse(response);
    return json["symbols"]
        .Where(s => s["quoteAsset"]!.ToString() == "USDT" && s["status"]!.ToString() == "TRADING")
        .Select(x => x["symbol"]!.ToString())
        .ToList();
}
