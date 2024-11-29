namespace TherapyApp.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TherapyApp.Helpers.Secrets;

public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIVariables _openaiEnvironment;
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public OpenAIService(HttpClient httpClient, IOptions<OpenAIVariables> openAiEnvironment)
    {
        _httpClient = httpClient;
        _openaiEnvironment = openAiEnvironment.Value;
        _apiKey = _openaiEnvironment.Secret 
            ?? throw new InvalidOperationException("API Key not configured");
        _apiUrl = _openaiEnvironment.Url 
            ?? throw new InvalidOperationException("API Url not configured");
    }
    public async Task<string> GetConciseAnswer(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8,
                                                                    "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        try
        {
            var response = await _httpClient.PostAsync(_apiUrl, content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(responseString);
            var messageContent = json?.choices[0]?.message?.content?.ToString();

            return messageContent ?? "No response";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
