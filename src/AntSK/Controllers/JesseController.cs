using AntSK.Domain.Domain.Interface;
using AntSK.Domain.Domain.Model.Constant;
using AntSK.Domain.Domain.Model.Dto;
using AntSK.Domain.Domain.Other.Bge;
using AntSK.Domain.Options;
using AntSK.Domain.Repositories;
using AntSK.plugins.Functions;
using Markdig;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace AntSK.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JesseController : ControllerBase
    {
        private readonly FunctionTest _functionTest;
        private readonly IKMService _kMService;
        private readonly IKmsDetails_Repositories _kmsDetails_Repositories;
        private MemoryServerless _kernelmemory;
        private readonly IOptions<LocalBgeConfigOptions> _bgeOptions;

        private static string KmsId => "a2016a9b-264a-4c43-9b0c-9d584c9e2ffd";
        public JesseController(
        FunctionTest functionTest,
        IKMService kMService,
        IKmsDetails_Repositories kmsDetails_Repositories,
        [FromKeyedServices("JKMemory")] MemoryServerless kernelmemory,
        IOptions<LocalBgeConfigOptions> bgeOptions)
        {
            _functionTest = functionTest;
            _kMService = kMService;
            _kmsDetails_Repositories = kmsDetails_Repositories;
            _kernelmemory = kernelmemory;
            _bgeOptions = bgeOptions;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetWorkingHours()
        {

            var result = _functionTest.GetWorkingHours("ZHOUZJ");

            return Ok(result);
        }

        [HttpGet]
        public async Task<List<RelevantSource>> SimilaritySearch(string searchContent)
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

            return data;
        }

        [HttpPost]
        public async Task<List<RelevantSource>> SimilaritySearchWithMemory([FromForm] string query, [FromForm] string[] searchContent)
        {
            foreach (var item in searchContent)
            {
                await _kernelmemory.ImportTextAsync(item);
            }

            var searchResult = await _kernelmemory.SearchAsync(query);
            var data = new List<RelevantSource>();
            if (!searchResult.NoResult)
            {
                BegRerankConfig.LoadModel(_bgeOptions.Value.PythonDllPath, _bgeOptions.Value.ReRankerModel);
                data.AddRange(
                    searchResult.Results.SelectMany(item => item.Partitions.Select(part => new RelevantSource()
                    {
                        SourceName = item.SourceName,
                        Text = part.Text,
                        Relevance = part.Relevance,
                        RerankScore = BegRerankConfig.Rerank(new List<string>() { query, part.Text })
                    })).ToList()
                );

                foreach (var guid in searchResult.Results.Select(c => c.DocumentId))
                {
                    await _kernelmemory.DeleteDocumentAsync(guid);
                }
            }

            return data.OrderByDescending(p => p.RerankScore).ToList();
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
    }
}
