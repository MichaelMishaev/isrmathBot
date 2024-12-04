using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Functions
{
    public class CommonFunctions
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CommonFunctions(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task SendResponseToSender(string number, string message)
        {
            // Prepare the data to send to Node.js server
            var client = _httpClientFactory.CreateClient();
            var requestUri = "http://localhost:3000/send-message"; // Assuming Node.js server runs locally on port 3000

            var payload = new
            {
                number = number.Replace("@c.us", ""), // Strip the "@c.us" part if needed
                message = message
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Send the HTTP request to the Node.js server
            var response = await client.PostAsync(requestUri, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Message successfully sent to {number}");
            }
            else
            {
                Console.WriteLine($"Failed to send message to {number}: {response.StatusCode}");
            }
        }
    }
}
