using Microsoft.AspNetCore.Mvc;
using MMDK;

namespace MMDK.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {



        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "Get")]
        public IEnumerable<string> Get(string input)
        {
            List<string> res=new List<string>();
            MMDKBot.Instance.HandleLocalAPI(input);
            return res;
        }
    }
}
