namespace TherapyApp.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TherapyApp.Helpers.Secrets;
using DotnetGeminiSDK;
using DotnetGeminiSDK.Client.Interfaces;
using DotnetGeminiSDK.Client;
using DotnetGeminiSDK.Model.Response;

public class ChatGptService
{
    private readonly HttpClient _httpClient;
    private readonly GptVariables _gptEnvironment;
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly IGeminiClient _geminiClient;

    public ChatGptService(HttpClient httpClient, IOptions<GptVariables> gptEnvironment, IGeminiClient geminiClient)
    {
        _geminiClient = geminiClient;
        _httpClient = httpClient;
        _gptEnvironment = gptEnvironment.Value;
        _apiKey = _gptEnvironment.Secret ?? throw new InvalidOperationException("API Key not configured");
        _apiUrl = _gptEnvironment.Url ?? throw new InvalidOperationException("API Url not configured");
        //_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        //_httpClient.DefaultRequestHeaders.Remove("Authorization");
        //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
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

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
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

    public async Task<string> GetResponseFromOpenAI(string prompt)
    {
        var api = new OpenAI_API.OpenAIAPI(_apiKey);
        var result = await api.Completions.GetCompletion(prompt);
        return result;
    }
}
