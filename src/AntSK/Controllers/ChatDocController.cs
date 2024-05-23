using BifrostiC.SparkDesk.ChatDoc.Models.ChatDoc;
using BifrostiC.SparkDesk.ChatDoc.Services.ChatDoc;
using Microsoft.AspNetCore.Mvc;

namespace AntSK.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChatDocController : ControllerBase
    {
        private readonly ILogger<ChatDocController> _logger;
        private readonly IChatDocService _chatDocService;

        public ChatDocController(ILogger<ChatDocController> logger, IChatDocService chatDocService)
        {
            _logger = logger;
            _chatDocService = chatDocService;
        }

        [HttpPost]
        public async Task<IActionResult> FileUpload(IFormFile file, string? callbackUrl)
        {
            if (file == null)
                return BadRequest("Either file or url is required");

            //if (!string.IsNullOrEmpty(callbackUrl))
            //    content.Add(new StringContent(callbackUrl), "callbackUrl");

            using MemoryStream memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var result = await _chatDocService.FileUpload(memoryStream, file.FileName);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Failed to upload file: {result.StatusCode} - {result.ReasonPhrase}");
                return StatusCode(result.StatusCode, result.ReasonPhrase);
            }
        }

        [HttpPost]
        public async Task<IActionResult> FileUploadWithUrl(string url, string fileName, string? callbackUrl)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("Either file or url is required");

            if (string.IsNullOrEmpty(fileName))
                return BadRequest("fileName is required when providing url");

            //if (!string.IsNullOrEmpty(callbackUrl))
            //    content.Add(new StringContent(callbackUrl), "callbackUrl");

            var result = await _chatDocService.FileUpload(url, fileName);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Failed to upload file: {result.StatusCode} - {result.ReasonPhrase}");
                return StatusCode(result.StatusCode, result.ReasonPhrase);
            }
        }

        [HttpGet]
        public async Task<IActionResult> StartSummary(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return BadRequest("fileId is required");

            var result = await _chatDocService.StartSummary(fileId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Failed to start summary: {result.StatusCode} - {result.ReasonPhrase}");
                return StatusCode(result.StatusCode, result.ReasonPhrase);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFileSummary(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return BadRequest("fileId is required");

            var result = await _chatDocService.FileSummary(fileId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Failed to get file summary: {result.StatusCode} - {result.ReasonPhrase}");
                return StatusCode(result.StatusCode, result.ReasonPhrase);
            }
        }

        [HttpPost]
        public async Task<IActionResult?> AskWiki([FromForm] string fileId, [FromForm] string? question)
        {
            if (string.IsNullOrEmpty(fileId))
                return BadRequest("fileId is required");

            if (string.IsNullOrEmpty(question))
                question = """
                    # 文档信息提取
                    请根据要求对文档中的信息进行提取，按指定格式输出
                    
                    **提取信息类型**：
                    - 提取文档中的所有项目名称信息
                    
                    **输出格式**：
                    - 以JSON列表格式输出
                        
                    **输出示例**：
                    - ["testName1","testName2", ... ]

                    **注意**：
                    - 只需要将提取出的信息按要求的格式输出，不要输出其他的内容
                    - 注意输出的JSON格式中需要双引号

                    """;

            var result = await _chatDocService.AskWiki(fileId, new List<WikiMessage>(), question);

            return Ok(result);
        }

    }
}
