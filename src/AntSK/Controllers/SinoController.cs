using AntSK.Domain.Common.Embedding;
using AntSK.Domain.Domain.Interface;
using AntSK.Domain.Domain.Model.Constant;
using AntSK.Domain.Domain.Model.Dto;
using AntSK.Domain.Domain.Other.Bge;
using AntSK.Domain.Options;
using AntSK.Domain.Repositories;
using AntSK.plugins.Functions;
using Markdig;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Handlers;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Text;

namespace AntSK.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SinoController : ControllerBase
    {
        private readonly ILogger<SinoController> _logger;
        private readonly FunctionTest _functionTest;
        private readonly IKMService _kMService;
        private readonly IKmsDetails_Repositories _kmsDetails_Repositories;
        private MemoryServerless _kernelMemory;
        private MemoryServerless _sirioMemory; 
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly GlobalBgeReranker _bgeReranker;

        private const string KmsId = "a2016a9b-264a-4c43-9b0c-9d584c9e2ffd";

        public SinoController(
        ILogger<SinoController> logger,
        FunctionTest functionTest,
        IKMService kMService,
        IKmsDetails_Repositories kmsDetails_Repositories,
        [FromKeyedServices("JKMemory")] MemoryServerless kernelMemory,
        [FromKeyedServices("SirioMemory")] MemoryServerless sirioMemory,
        IOptions<LocalBgeConfigOptions> bgeOptions,
        GlobalBgeReranker bgeReranker,
        IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _functionTest = functionTest;
            _kMService = kMService;
            _kmsDetails_Repositories = kmsDetails_Repositories;
            _kernelMemory = kernelMemory;
            _sirioMemory = sirioMemory;
            _bgeReranker = bgeReranker;

            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetWorkingHours()
        {

            var result = _functionTest.GetWorkingHours("ZHOUZJ");

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<RelevantSource>>> SimilaritySearch(string searchContent)
        {
            MemoryServerless _memory = _kMService.GetMemoryByKMS(KmsId);
            var filters = new MemoryFilter().ByTag(KmsConstantcs.KmsIdTag, KmsId);
            var searchResult = await _memory.SearchAsync(searchContent, index: KmsConstantcs.KmsIndex, filters: [filters]);

            var data = new List<RelevantSource>();
            data.AddRange(
                searchResult.Results.SelectMany(item => item.Partitions.Select(part => new RelevantSource()
                {
                    SourceName = GetFileName(item.SourceName),
                    Text = part.Text,
                    Relevance = part.Relevance
                })).Take(10).ToList()
            );

            return new JsonResult(data);
        }

        [HttpPost]
        public async Task<ActionResult<List<RelevantSource>>> SimilaritySearchWithMemory([FromForm] string query, [FromForm] string[] searchContent)
        {
            foreach (var item in searchContent)
            {
                await _kernelMemory.ImportTextAsync(item);
            }

            var searchResult = await _kernelMemory.SearchAsync(query);
            var data = new List<RelevantSource>();
            if (!searchResult.NoResult)
            { 
                data.AddRange(
                    searchResult.Results.SelectMany(item => item.Partitions.Select(part => new RelevantSource()
                    {
                        SourceName = item.SourceName,
                        Text = part.Text,
                        Relevance = part.Relevance,
                        RerankScore = _bgeReranker.Rerank(new List<string>() { query, part.Text })
                    })).ToList()
                );

                foreach (var guid in searchResult.Results.Select(c => c.DocumentId))
                {
                    await _kernelMemory.DeleteDocumentAsync(guid);
                }
            }

            return new JsonResult(data.OrderByDescending(p => p.RerankScore).ToList());
        }

        private string GetFileName(string fileGuidName)
        {
            var fileDetail = _kmsDetails_Repositories.GetFirst(p => p.FileGuidName == fileGuidName);
            if (fileDetail == null)
            {
                return fileGuidName;
            }
            var fileName = fileDetail.FileName;
            return fileName;
        }

        [HttpGet]
        public async Task<ActionResult<List<RelevantSource>>> SimilaritySearchSirio(string searchContent)
        { 
            var _memory = this._sirioMemory;

            var searchResult = await _memory.SearchAsync(searchContent);
            var data = new List<RelevantSource>();
            if (!searchResult.NoResult)
            { 
                data.AddRange(
                    searchResult.Results.SelectMany(item => item.Partitions.Select(part => new RelevantSource()
                    {
                        SourceName = item.SourceName,
                        Text = part.Text,
                        Relevance = part.Relevance,
                        RerankScore = _bgeReranker.Rerank(new List<string>() { searchContent, part.Text })
                    })).ToList()
                );
            }

            return new JsonResult(data.OrderByDescending(p => p.RerankScore).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<List<List<RelevantSource>>>> SimilaritySearchListSirio([FromForm] string strings)
        {
            List<string> listItem = JsonConvert.DeserializeObject<List<string>>(strings);

            var _memory = _sirioMemory;

            var listResults = new List<List<RelevantSource>>(); 
            foreach (string query in listItem)
            {
                var searchResult = await _memory.SearchAsync(query);
                var data = new List<RelevantSource>();
                if (!searchResult.NoResult)
                {
                    data.AddRange(
                        searchResult.Results.SelectMany(item => item.Partitions.Select(part => new RelevantSource()
                        {
                            SourceName = item.SourceName,
                            Text = part.Text,
                            Relevance = part.Relevance,
                            //RerankScore = _bgeReranker.Rerank(new List<string>() { query, part.Text })
                        })).ToList()
                    );
                }
                listResults.Add(data.OrderByDescending(p => p.RerankScore).ToList());
            }

            return new JsonResult(listResults);
        }

        
    }
}
