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
        private readonly string _apiUrl = "http://195.179.229.119/gpt/api.php";

        public ChatbotService(IConfiguration configuration)
        {
            _apiKey = configuration["ChatbotSettings:ApiKey"];
            _apiUrl = configuration["ChatbotSettings:ApiUrl"];
        }


        public async Task<string> GetResponseAsync(string prompt)
        {
            // Define the default model
            string model = "gpt-3.5-turbo";

            // Build the URL with query parameters
            string apiUrlWithParams = $"{_apiUrl}?prompt={HttpUtility.UrlEncode(prompt)}&api_key={HttpUtility.UrlEncode(_apiKey)}&model={HttpUtility.UrlEncode(model)}";

            using var client = new HttpClient();
            try
            {
                // Send the GET request
                HttpResponseMessage response = await client.GetAsync(apiUrlWithParams);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response as a string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Giải mã chuỗi Unicode sang tiếng Việt
                string decodedResponse = System.Text.RegularExpressions.Regex.Unescape(responseBody);

                // Return the decoded response
                return decodedResponse;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw new Exception("Failed to fetch response from AI API.");
            }
        }

    }
}
