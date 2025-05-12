namespace VolatilityMonitor;

public class Candle
{
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public DateTime CloseTime { get; set; }

    public override string ToString() => $"Close: {Close}, Volume: {Volume}, Time: {CloseTime:yyyy-MM-dd HH:mm:ss}";
}
