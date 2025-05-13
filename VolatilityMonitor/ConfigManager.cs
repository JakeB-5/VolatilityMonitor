namespace VolatilityMonitor;

public class ConfigManager
{
    private readonly static string DataPath = @".\";
    private static readonly Lazy<ConfigManager> Lazy = new(() => new ConfigManager());
    public static ConfigManager Instance => Lazy.Value;
    private readonly Dictionary<string, string?> _settings = new();

    protected ConfigManager()
    {
        LoadConfigData();
    }

    private void LoadConfigData()
    {
        using StreamReader sr = new StreamReader(Path.Combine(DataPath, "config.ini"));
        string? line;
        String delimStr = "=";
        char[] delimiter = delimStr.ToCharArray();
        while ((line = sr.ReadLine()) != null)
        {
            string?[] strData = line.Split(delimiter);
            _settings.Add(strData[0], strData[1]);
        }
    }
    
    private string? GetSetting(ConfigKey key)
    {
        return _settings[Enum.GetName(typeof(ConfigKey), key)];
    }

    public static string? Get(ConfigKey key, string defaultValue = "")
    {
        return Instance.GetSetting(key) ?? defaultValue;
    }
}


public enum ConfigKey
{
    TelegramBotToken,
    TelegramChannelId,
    CandleInterval,
    AlertCooldown,
}

