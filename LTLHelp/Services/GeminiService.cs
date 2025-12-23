using System.Text.Json;

namespace LTLHelp.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GeminiService(IConfiguration config)
        {
            _http = new HttpClient();
            _apiKey = config["Gemini:ApiKey"];
        }

        public async Task<string> AskGemini(string prompt)
        {
            var model = "gemini-2.5-flash"; // ✅ thử cái này trước
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
            };

            var response = await _http.PostAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 429)
                {
                    return "Hiện hệ thống đang quá tải. Bạn vui lòng thử lại sau ít phút nhé.";
                }

                return "Hệ thống AI đang tạm thời không phản hồi. Vui lòng thử lại sau.";
            }


            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (json.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var parts = candidates[0].GetProperty("content").GetProperty("parts");
                if (parts.GetArrayLength() > 0 && parts[0].TryGetProperty("text", out var text))
                    return text.GetString() ?? "Gemini trả lời rỗng.";
            }

            return "Gemini không trả nội dung.";
        }

    }
}
