# 📈VolatilityMonitor
A real-time volatility monitoring tool for cryptocurrency markets, designed to help traders identify sudden price movements and take timely actions.

## 📌 Features

---
- Real-time volatility scanning of crypto pairs from Binance
- Customizable time intervals for volatility measurement (e.g., 1min, 5min)
- Alerts on threshold breach to help catch pump/dump activity
- Simple and modular structure for easy extension



## ⚙️ Configuration

---
- Modify config.ini
```ini
TelegramBotToken=
TelegramChannelId=
CandleInterval=5m
AlertCooldown=20 
```
- **TelegramBotToken**: Telegram bot token (by BotFather)
- **TelegramChannelId**: Telegram channel ID
- **CandleInterval**: Chart Timeframe (1m,5m,15m,1h,1d ...) 
- **AlertCooldown**: prevent duplicate notifications for the same pair (minute) 

## 🛠️ Roadmap / TODO

---
- [ ] Add support for other exchanges (e.g., Bybit, Okx)
- [ ] Other volatility detection (VWAP, Bollinger, TrendLine)
