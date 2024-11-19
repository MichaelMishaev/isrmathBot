using BL.Serives;
using BLL.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Twilio.TwiML;
using System.Text.Json;

namespace mathProj.Controllers
{
    [Route("api/whatsapp")]
    [ApiController]
    public class WhatsAppController : ControllerBase
    {

        private readonly WhatsAppService _whatsAppService;
        private readonly IndexService _indexService;
        private readonly TestWhatsAppService _testWhatsAppService;

        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;
        private readonly IHttpClientFactory _httpClientFactory;
        public WhatsAppController(WhatsAppService whatsAppService, IndexService indexService, TestWhatsAppService testWhatsAppService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _whatsAppService = whatsAppService;
            _indexService = indexService;
            _testWhatsAppService = testWhatsAppService;
            _httpClientFactory = httpClientFactory;
        }

  


        [HttpPost("messageTEST")]
        public async Task<IActionResult> ReceiveMessageTEST([FromForm] string From, [FromForm] string Body)
        {
            Console.WriteLine($"Received message from {From}: {Body}");

            // Send a response back to the user
            
            // await SendWhatsAppMessage(From, responseMessage);

            var responseMessage = "Hello! Thanks for your message. This is an automated response from ASP.NET Core.";

            await SendResponseToSender(From, responseMessage);

            return Ok();
        }


        [HttpPost("message")]
        public async Task<IActionResult> ReceiveMessage(
                                                        [FromForm] string From,
                                                        [FromForm] string? Body)
        {
            // Log or process the incoming message
            Console.WriteLine($"Received message from {From}: {Body}");

            var mediaUrls = new List<string>();
            var mediaContentTypes = new List<string>();

            if (From == "status@broadcast" || From.Contains("@g.us"))
            {
                return Ok("Broadcast or group message ignored");
            }
            var result = await _indexService.UserBalancer(From, Body, mediaUrls, mediaContentTypes);

            //await SendResponseToSender(From, result);

            return Ok(result); //Content(response.ToString(), "application/xml");
        }

        private async Task SendResponseToSender(string number, string message)
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
