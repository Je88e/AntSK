using AntSK.Domain.Common.Embedding;
using AntSK.Domain.Domain.Model.Constant;
using AntSK.Domain.Domain.Other;
using AntSK.Domain.Options;
using BifrostiC.DashScope.QwenVL.Common.Dependency;
using BifrostiC.SparkDesk.ChatDoc.Common.Dependency;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.Handlers;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using SqlSugar;
using System.Data;
using System.Text;

namespace AntSK.Domain.Common.DependencyInjection
{
    public static class SinoExtensions
    {
        public static IServiceCollection AddSinoConfiguration(this IServiceCollection services, IConfiguration configuration)
        { 
            services.AddChatDoc(configuration);
            services.AddDashScopeQwenVLChat(configuration);

            services.Configure<LocalBgeConfigOptions>(configuration.GetSection("LocalBgeConfig"));

            services.AddSingleton<GlobalBgeReranker>();

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

                var dataRows = ExcelHelper.ExcelToDataTable(fileStream, true).Select().Select(r => r["TESTNO"] as string).Distinct().ToList();

                StringBuilder text = new StringBuilder();
                foreach (var item in dataRows)
                {
                    text.AppendLine(@$"{item}{KmsConstantcs.KMExcelSplit}");
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
