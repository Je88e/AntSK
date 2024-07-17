using AntSK.Domain.Common.Pdf;
using BifrostiC.SparkDesk.ChatDoc.Models.v1;
using BifrostiC.SparkDesk.ChatDoc.Services.ChatDoc.v1;
using Microsoft.AspNetCore.Mvc;

namespace AntSK.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChatDocController : ControllerBase
    {
        private readonly ILogger<ChatDocController> _logger;
        private readonly IChatDocService _chatDocService; 
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ChatDocController(ILogger<ChatDocController> logger, IChatDocService chatDocService, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _chatDocService = chatDocService;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> FileUpload(IFormFile file, string? callbackUrl)
        {
            if (file == null)
                return BadRequest("Either file or url is required");

            using var memoryStream = new MemoryStream();
            await using var fileStream = file.OpenReadStream();
            string fileName;
            bool useOCR = false;

            if (FileTypeIsImage(file.ContentType))
            {
                using var pdfStream = PdfHelper.ConvertImageToPdf(fileStream);
                await pdfStream.CopyToAsync(memoryStream);
                useOCR = true;
                fileName = $"{Guid.NewGuid().ToString()}.pdf";
            }
            else
            {
                fileName = file.FileName;
                await fileStream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;
            var result = await _chatDocService.FileUpload(memoryStream, fileName, useOCR);

            if (result.IsSuccessStatusCode)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Failed to upload file: {result.StatusCode} - {result.Message}");
                return StatusCode((int)result.StatusCode, result.Message);
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

            var result = await _chatDocService.FileUpload(url, fileName, false);

            if (result.IsSuccessStatusCode)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Failed to upload file: {result.StatusCode} - {result.Message}");
                return StatusCode((int)result.StatusCode, result.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> StartSummary(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return BadRequest("fileId is required");

            var result = await _chatDocService.StartSummary(fileId);

            if (result.IsSuccessStatusCode)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Failed to start summary: {result.StatusCode} - {result.Message}");
                return StatusCode((int)result.StatusCode, result.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFileSummary(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return BadRequest("fileId is required");

            var result = await _chatDocService.GetSummary(fileId);

            if (result.IsSuccessStatusCode)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError($"Failed to get file summary: {result.StatusCode} - {result.Message}");
                return StatusCode((int)result.StatusCode, result.Message);
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

        private async Task<string> SaveToLocal(IFormFile file)
        {
            var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "files");
            // 如果路径不存在，则创建一个新的目录
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            string extension = Path.GetExtension(file.FileName);
            string fileid = Guid.NewGuid().ToString();
            // 组合目标路径
            var uploads = Path.Combine(uploadsFolderPath, fileid + extension);

            // 保存文件至目标路径
            using var fileStream = System.IO.File.Create(uploads);
            using var uploadStream = file.OpenReadStream();
            await uploadStream.CopyToAsync(fileStream);

            return uploads;
        }

        private bool FileTypeIsImage(string fileType)
        {
            List<string> listImgFileType = ["image/jpeg", "image/png"];

            return listImgFileType.Contains(fileType);
        }
    }
}
