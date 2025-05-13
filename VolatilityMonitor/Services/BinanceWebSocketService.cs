using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace VolatilityMonitor.Services;

public class BinanceWebSocketService
{
    private readonly List<IAlertCondition> _conditions;
    private readonly TelegramNotifier _notifier;
    private readonly Dictionary<string, List<Candle>> _history = new();
    private readonly Dictionary<string, DateTime> _lastAlertTime = new();
    private readonly TimeSpan _cooldown = TimeSpan.FromMinutes(20);
    private readonly string _interval = "5m";
    
    public BinanceWebSocketService(List<IAlertCondition> conditions, TelegramNotifier notifier)
    {
        _conditions = conditions;
        _notifier = notifier;
        _interval = ConfigManager.Get(ConfigKey.CandleInterval);
        _cooldown = TimeSpan.FromMinutes(int.Parse(ConfigManager.Get(ConfigKey.AlertCooldown)));
    }

    public async Task StartAsync(List<string> symbols)
    {
        var streams = symbols.Select(s => $"{s.ToLower()}@kline_{_interval}");
        string endpoint = $"wss://stream.binance.com:9443/stream?streams={string.Join("/", streams)}";

        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri(endpoint), CancellationToken.None);
        var buffer = new byte[8192];

        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            PorcessMessage(json);
        }
    }

    private void PorcessMessage(string json)
    {
        var data = JObject.Parse(json)["data"]?["k"];
        if (data == null) return;

        string symbol = data["s"]!.ToString();
        decimal close = decimal.Parse(data["c"]!.ToString());
        decimal volume = decimal.Parse(data["v"]!.ToString());
        long openTime = long.Parse(data["t"]!.ToString());
        
        var candle = new Candle
        {
            Close = close,
            Volume = volume,
            CloseTime = DateTimeOffset.FromUnixTimeMilliseconds(openTime).UtcDateTime
        };
        
        if (!_history.ContainsKey(symbol))
            _history[symbol] = new List<Candle>();
        var list = _history[symbol];

        var lastCandle = list.Last();
        if (list.Count == 0 || candle.CloseTime > lastCandle.CloseTime)
        {
            list.Add(candle);

            if (list.Count > 20)
                list.RemoveAt(0);
        } else if (lastCandle.CloseTime == candle.CloseTime)
        {
            lastCandle.Close = candle.Close;
            lastCandle.Volume = candle.Volume;
        }

        var now = DateTime.UtcNow;
        if (_lastAlertTime.TryGetValue(symbol, out var last) && now - last < _cooldown) return;
        foreach (var cond in _conditions)
        {
            if (cond.ShouldAlert(symbol, list, out string reason))
            {
                _lastAlertTime[symbol] = now;
                // Console.WriteLine($"[{symbol}] Alert\nReason: {reason}");
                // list.ForEach(a => Console.WriteLine(a.ToString()));
                // Console.WriteLine($"{list.Take(20).Average(c=>c.Volume)} AVG");

                _ = _notifier.SendAsync($"[{symbol}] Alert\nReason: {reason}");
            }
        }

    }
    public async Task<Dictionary<string, List<Candle>>> LoadCandlesForSymbolsAsync(List<string> symbols, int limit = 20)
    {
        var result = new Dictionary<string, List<Candle>>();
        var semaphore = new SemaphoreSlim(10); // 동시 요청 제한 (Binance API 보호 목적)

        var tasks = symbols.Select(async symbol =>
        {
            await semaphore.WaitAsync();
            try
            {
                var candles = await LoadRecentCandlesAsync(symbol, limit);
                lock (result)
                {
                    result[symbol] = candles;
                    // Console.WriteLine($"[{symbol}] Loaded");
                    // candles.ForEach(a => Console.WriteLine(a.ToString()));
                    // Console.WriteLine($"{candles.TakeLast(20).Average(c=>c.Volume)} AVG");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{symbol}] Failure Load OHLCV: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return result;
    }
    public void InitializeHistory(Dictionary<string, List<Candle>> initialData)
    {
        foreach (var pair in initialData)
        {
            _history[pair.Key] = pair.Value;
        }
    }
    public async Task<List<Candle>> LoadRecentCandlesAsync(string symbol, int limit = 20)
    {
        using var client = new HttpClient();
        string url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={_interval}&limit={limit+1}";
        var response = await client.GetStringAsync(url);
        var data = JArray.Parse(response);

        return data.Select(item => new Candle
        {
            Close = decimal.Parse(item[4].ToString()),
            Volume = decimal.Parse(item[5].ToString()),
            CloseTime = DateTimeOffset.FromUnixTimeMilliseconds((long)item[6]).UtcDateTime
        }).Take(20).ToList();
    }
    
}
