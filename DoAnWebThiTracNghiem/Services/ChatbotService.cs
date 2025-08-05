using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace DoAnWebThiTracNghiem.Services
{
    public class ChatbotService
    {
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";

        public ChatbotService(IConfiguration configuration)
        {
            _apiKey = configuration["ChatbotSettings:ApiKey"];
        }

        public async Task<string> GetResponseAsync(string prompt)
        {
            using var client = new HttpClient();
            var requestBody = new
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

            var requestUrl = $"{_apiUrl}{_apiKey}";
            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            Console.WriteLine($"Request URL: {requestUrl}");
            Console.WriteLine($"Request Body: {jsonBody}");

            try
            {
                HttpResponseMessage response = await client.PostAsync(requestUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Body: {responseBody}");

                // Kiểm tra trạng thái sau khi đã lưu responseBody
                response.EnsureSuccessStatusCode();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                // Không sử dụng e.Response vì nó không tồn tại
                Console.WriteLine($"Request error: {e.Message}");
                throw new Exception("Không thể lấy phản hồi từ API AI: " + e.Message);
            }
        }
    }
}
