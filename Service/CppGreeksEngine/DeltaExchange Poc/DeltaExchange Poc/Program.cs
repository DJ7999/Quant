using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeltaExchange_Poc
{
    internal class Program
    {
        private static readonly string BaseUrl = "https://cdn.india.deltaex.org";

        static async Task Main(string[] args)
        {
            // The confirmed database symbol from the diagnostic scan
            string symbol = "C-BTC-77800-170526";
            string resolution = "1h"; // Using 1-hour intervals to catch trade periods

            // Look back 5 days to ensure we capture the active trading sessions
            long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long startTime = DateTimeOffset.UtcNow.AddDays(-5).ToUnixTimeSeconds();

            string requestUrl = $"{BaseUrl}/v2/history/candles?symbol={symbol}&resolution={resolution}&start={startTime}&end={endTime}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Adding user-agent to comfortably bypass Cloudflare WAF
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    Console.WriteLine($"Fetching historical OHLC for {symbol} ({resolution} bars)...");
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var apiData = JsonSerializer.Deserialize<DeltaApiResponse>(jsonResponse);

                        if (apiData?.Result != null && apiData.Result.Count > 0)
                        {
                            Console.WriteLine($"\nSuccess! Retrieved {apiData.Result.Count} historical data points:");
                            Console.WriteLine("--------------------------------------------------------------------------------");
                            Console.WriteLine($"{"Time (UTC)",-20} | {"Open",-10} | {"High",-10} | {"Low",-10} | {"Close",-10} | {"Volume",-10}");
                            Console.WriteLine("--------------------------------------------------------------------------------");

                            foreach (var candle in apiData.Result)
                            {
                                DateTime candleTime = DateTimeOffset.FromUnixTimeSeconds(candle.Time).UtcDateTime;
                                Console.WriteLine($"{candleTime:yyyy-MM-dd HH:mm:ss,-20} | {candle.Open,-10:F2} | {candle.High,-10:F2} | {candle.Low,-10:F2} | {candle.Close,-10:F2} | {candle.Volume,-10:F2}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No candle data points found within this time range.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"API Request failed. Status Code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
            }
        }
    }

    public class DeltaApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("result")]
        public System.Collections.Generic.List<CandleData> Result { get; set; }
    }

    public class CandleData
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("open")]
        public double Open { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }

        [JsonPropertyName("close")]
        public double Close { get; set; }

        [JsonPropertyName("volume")]
        public double Volume { get; set; }
    }
}