using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BL.Serives
{
    public class ChatGPTService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ChatGPTService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
        }

        public async Task<string> AskChatGPT(string prompt)
        {
            var requestUri = "https://api.openai.com/v1/chat/completions";

            var requestBody = new
            {
                model = "gpt-4", // or "gpt-4"
                messages = new[]
                {
                new { role = "system", content = "You are ChatGPT." },
                new { role = "user", content = prompt }
            },
                max_tokens = 150
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            // Add OpenAI API key to the Authorization header
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                return jsonResponse.choices[0].message.content.ToString();
            }
            else
            {
                throw new HttpRequestException($"Error calling OpenAI API: {response.ReasonPhrase}");
            }
        }
    }
}
