using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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

            _httpClient.Timeout = TimeSpan.FromSeconds(200);
        }

        public async Task<string> AskChatGPT(string prompt)
        {
            var requestUri = "https://api.openai.com/v1/chat/completions";

            var requestBody = new
            {
                model = "gpt-4-turbo",
                messages = new[]
                {
            new { role = "user", content = prompt }
        },
                max_tokens = 2500 // Adjust as needed
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            // Add OpenAI API key to the Authorization header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            // Use CancellationTokenSource for per-request timeout
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(200)))
            {
                var response = await _httpClient.PostAsync(requestUri, content, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Parse the response to extract the assistant's message content
                    var jsonResponse = JObject.Parse(responseString);
                    var contentResponse = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString();

                    if (string.IsNullOrWhiteSpace(contentResponse))
                    {
                        throw new Exception("The assistant's response is empty or null.");
                    }

                    return contentResponse;
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error calling OpenAI API: {response.ReasonPhrase} - {errorResponse}");
                }
            }
        }


        #region oldAskGPT
        public async Task<string> AskChatGPT1(string prompt)
        {

            var requestUri = "https://api.openai.com/v1/chat/completions";  // Ensure this is correct!

            var requestBody = new
            {
                model = "gpt-4-turbo",  // Double-check this model name
                messages = new[]
                {
                new { role = "system", content = "You are ChatGPT." },
                new { role = "user", content = prompt }
            },
                max_tokens = 4096
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            // Add OpenAI API key to the Authorization header
            _httpClient.DefaultRequestHeaders.Clear();
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
                // Log the error response details for debugging
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error calling OpenAI API: {response.ReasonPhrase} - {errorResponse}");
            }
        }

        #endregion

        public async Task<string> AskChatGPTReduceTokens(string prompt)
        {

            var requestUri = "https://api.openai.com/v1/chat/completions";  // Ensure this is correct!

            var requestBody = new
            {
                model = "gpt-4-turbo",  // Double-check this model name
                messages = new[]
                {
                new { role = "system", content = "You are ChatGPT." },
                new { role = "user", content = prompt }
            },
                max_tokens = 400
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            // Add OpenAI API key to the Authorization header
            _httpClient.DefaultRequestHeaders.Clear();
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
                // Log the error response details for debugging
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error calling OpenAI API: {response.ReasonPhrase} - {errorResponse}");
            }
        }
    }

}
