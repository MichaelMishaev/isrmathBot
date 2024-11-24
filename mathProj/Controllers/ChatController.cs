using BL.Serives;
using Microsoft.AspNetCore.Mvc;

namespace mathProj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {

        private readonly ChatGPTService _chatGPTService;

        public ChatController(ChatGPTService chatGPTService)
        {
            _chatGPTService = chatGPTService;
        }

        // POST: api/chat/ask
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string prompt)
        {
            //"אנא תן לי 10 תרגילי כפל בטור, כמו הדוגמה:\\n112\\n*\\n4\\nוהצג את התרגילים בפורמט JSON שבו הם יוצגו בטור אחרי שליפה. \\nאל תכתוב שום דבר אחר חוץ מהתרגילים, שום מילה מיותרת.\\nדוגמה: {\"exercise\": \"12\\\\n*\\\\n6\"}"
            try
            {
                var response = await _chatGPTService.AskChatGPT(prompt);
                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
