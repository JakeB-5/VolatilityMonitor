namespace VolatilityMonitor.Conditions;

public class PriceChangeCondition : IAlertCondition
{
    private readonly decimal _threshold;
    
    public PriceChangeCondition(decimal threshold)
    {
        _threshold = threshold;
    }
    
    public bool ShouldAlert(string symbol, List<Candle> candles, out string reason)
    {
        reason = "";
        if (candles.Count < 5) return false;
        
        var recent5 = candles.TakeLast(5).ToList();
        var avg = recent5.Average(c => c.Close);
        var latest = candles.Last().Close;
        var change = (latest - avg) / avg * 100;

        if (Math.Abs(change) > _threshold)
        {
            reason = $"Price change: {change:F2}%";
            return true;
        }

        return false;
    }
}
