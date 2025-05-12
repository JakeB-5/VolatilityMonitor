namespace VolatilityMonitor;

public interface IAlertCondition
{
    bool ShouldAlert(string symbol, List<Candle> candles, out string reason);
}
