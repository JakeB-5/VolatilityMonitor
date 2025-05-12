namespace VolatilityMonitor.Conditions;

public class VolumeSpikeCondition : IAlertCondition
{
    public bool ShouldAlert(string symbol, List<Candle> candles, out string reason)
    {
        reason = "";
        if (candles.Count < 20) return false;
        
        var avgVolume = candles.Take(20).Average(c => c.Volume);
        var latestVolume = candles.Last().Volume;

        if (latestVolume > avgVolume * 1.5m)
        {
            reason = $"Volume spike : {latestVolume} > {avgVolume} (average)";
            return true;
        }

        return false;
    }
}
