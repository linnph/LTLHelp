using LTLHelp.Models;
using LTLHelp.Services;
using Microsoft.AspNetCore.Mvc;

namespace LTLHelp.Controllers
{
    [ApiController]
    [Route("Chat")]
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest();

            var reply = await _chatService.ProcessMessage(request.Message);
            return Json(new { reply });
        }
    }
    
}
