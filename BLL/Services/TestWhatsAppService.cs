using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TestWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly string _phoneNumberId;
        private readonly string _accessToken;

        public TestWhatsAppService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _phoneNumberId = "+972555020829";
            _accessToken = "1199833841241675|71xgxCz-E_AEf41F214PryQy8A4";
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
        }

        public async Task SendWhatsAppMessage(string recipientPhoneNumber, string messageBody)
        {
            var url = $"https://graph.facebook.com/v17.0/{_phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = recipientPhoneNumber,
                type = "text",
                text = new
                {
                    body = messageBody
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Message sent successfully.");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, {responseContent}");
            }
        }
    }
}
