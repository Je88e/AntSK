using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BifrostiC.SparkDesk.ChatDoc.Common.Dependency;
using Microsoft.KernelMemory;
using AntSK.Domain.Domain.Model.Constant;
using AntSK.Domain.Repositories;
using Microsoft.SemanticKernel.Memory;
using AntSK.Domain.Common.Embedding;
using Microsoft.KernelMemory.Postgres;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.KernelMemory.Handlers;
using OpenCvSharp.ML;
using LLamaSharp.KernelMemory;
using BifrostiC.SparkDesk.ChatDoc.Models.Options;
using AntSK.Domain.Options;
using SqlSugar;
using Microsoft.Extensions.Options;
using AntSK.Domain.Domain.Other;
using System.Data;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace AntSK.Domain.Common.DependencyInjection
{
    public static class JesseExtensions
    {
        public static IServiceCollection AddJesseConfiguration(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddHttpClient("JCustom");
            services.AddChatDoc(configuration);

            services.Configure<LocalBgeConfigOptions>(configuration.GetSection("LocalBgeConfig"));

            services.AddKeyedSingleton("JKMemory", (services, _) =>
            {
                var searchClientConfig = new SearchClientConfig
                {
                    MaxAskPromptSize = 1024,
                    MaxMatchesCount = 10,
                    AnswerTokens = 1024,
                    EmptyAnswer = KmsConstantcs.KmsSearchNull
                };

                string pyDll = services.GetRequiredService<IOptions<LocalBgeConfigOptions>>().Value.PythonDllPath;
                string bgeEmbeddingModelName = services.GetRequiredService<IOptions<LocalBgeConfigOptions>>().Value.EmbeddingModel;

                var memoryBuild = new KernelMemoryBuilder()
                                        .WithDashScopeTextGeneration(new Cnblogs.KernelMemory.AI.DashScope.DashScopeConfig
                                        {
                                            ApiKey = "N/A",
                                        })
                                        //.WithoutDefaultHandlers()
                                        .WithSearchClientConfig(searchClientConfig)
                                        .WithBgeTextEmbeddingGeneration(new HuggingfaceTextEmbeddingGenerator(pyDll, bgeEmbeddingModelName))
                                        .WithSimpleVectorDb(new SimpleVectorDbConfig()
                                        {
                                            StorageType = FileSystemTypes.Volatile
                                        });

                var _memory = memoryBuild.Build<MemoryServerless>();

                return _memory;
            });

            services.AddKeyedSingleton("SirioMemory", (services, _) =>
            {
                var searchClientConfig = new SearchClientConfig
                {
                    MaxAskPromptSize = 1024,
                    MaxMatchesCount = 10,
                    AnswerTokens = 1024,
                    EmptyAnswer = KmsConstantcs.KmsSearchNull
                };

                string pyDll = services.GetRequiredService<IOptions<LocalBgeConfigOptions>>().Value.PythonDllPath;
                string bgeEmbeddingModelName = services.GetRequiredService<IOptions<LocalBgeConfigOptions>>().Value.EmbeddingModel;
                var hostingEnvironment = services.GetRequiredService<IWebHostEnvironment>();

                var memoryBuild = new KernelMemoryBuilder()
                                        .WithDashScopeTextGeneration(new Cnblogs.KernelMemory.AI.DashScope.DashScopeConfig
                                        {
                                            ApiKey = "N/A",
                                        })
                                        //.WithoutDefaultHandlers()
                                        .WithSearchClientConfig(searchClientConfig)
                                        .WithBgeTextEmbeddingGeneration(new HuggingfaceTextEmbeddingGenerator(pyDll, bgeEmbeddingModelName))
                                        .WithSimpleVectorDb(new SimpleVectorDbConfig()
                                        {
                                            StorageType = FileSystemTypes.Volatile
                                        });

                var _memory = memoryBuild.Build<MemoryServerless>();

                _memory.Orchestrator.AddHandler<TextExtractionHandler>("extract_text");
                _memory.Orchestrator.AddHandler<KMExcelHandler>("antsk_excel_split");
                _memory.Orchestrator.AddHandler<GenerateEmbeddingsHandler>("generate_embeddings");
                _memory.Orchestrator.AddHandler<SaveRecordsHandler>("save_memory_records");

                var fileName = @"202405221320-TESTS.xlsx";
                var filePath = Path.Combine(hostingEnvironment.WebRootPath, "files", fileName);
                using FileStream fileStream = System.IO.File.OpenRead(filePath);

                var dataTable = ExcelHelper.ExcelToDataTable(fileStream, true);

                StringBuilder text = new StringBuilder();
                foreach (DataRow item in dataTable.Rows)
                {
                    text.AppendLine(@$"{item["TESTNO"].ToString()}{KmsConstantcs.KMExcelSplit}");
                }
                var importText = text.ToString();
                _memory.ImportTextAsync(importText,
                      steps: new[]
                      {
                                        "extract_text",
                                        "antsk_excel_split",
                                        "generate_embeddings",
                                        "save_memory_records"
                      }
                ).Wait();

                return _memory;
            });

            services.AddKeyedSingleton("ocrMemory", (services, _) =>
            {
                var searchClientConfig = new SearchClientConfig
                {
                    MaxAskPromptSize = 1024,
                    MaxMatchesCount = 10,
                    AnswerTokens = 1024,
                    EmptyAnswer = KmsConstantcs.KmsSearchNull
                };

                string pyDll = services.GetRequiredService<IOptions<LocalBgeConfigOptions>>().Value.PythonDllPath;
                string bgeEmbeddingModelName = services.GetRequiredService<IOptions<LocalBgeConfigOptions>>().Value.EmbeddingModel;
                var hostingEnvironment = services.GetRequiredService<IWebHostEnvironment>();

                var memoryBuild = new KernelMemoryBuilder()
                                        .WithDashScopeTextGeneration(new Cnblogs.KernelMemory.AI.DashScope.DashScopeConfig
                                        {
                                            ApiKey = "N/A",
                                        })
                                        //.WithoutDefaultHandlers()
                                        .WithSearchClientConfig(searchClientConfig)
                                        .WithBgeTextEmbeddingGeneration(new HuggingfaceTextEmbeddingGenerator(pyDll, bgeEmbeddingModelName))
                                        .WithSimpleVectorDb(new SimpleVectorDbConfig()
                                        {
                                            StorageType = FileSystemTypes.Volatile
                                        });

                var _memory = memoryBuild.Build<MemoryServerless>();

                _memory.Orchestrator.AddHandler<TextExtractionHandler>("extract_text");
                _memory.Orchestrator.AddHandler<KMExcelHandler>("antsk_excel_split");
                _memory.Orchestrator.AddHandler<GenerateEmbeddingsHandler>("generate_embeddings");
                _memory.Orchestrator.AddHandler<SaveRecordsHandler>("save_memory_records");

                var fileName = @"202405221320-TESTS.xlsx";
                var filePath = Path.Combine(hostingEnvironment.WebRootPath, "files", fileName);
                using FileStream fileStream = System.IO.File.OpenRead(filePath);

                var dataTable = ExcelHelper.ExcelToDataTable(fileStream, true);

                StringBuilder text = new StringBuilder();
                foreach (DataRow item in dataTable.Rows)
                {
                    text.AppendLine(@$"{item["TESTNO"].ToString()}{KmsConstantcs.KMExcelSplit}");
                }
                var importText = text.ToString();
                _memory.ImportTextAsync(importText,
                      steps: new[]
                      {
                                        "extract_text",
                                        "antsk_excel_split",
                                        "generate_embeddings",
                                        "save_memory_records"
                      }
                ).Wait();

                return _memory;
            });

            return services;
        }
    }
}
