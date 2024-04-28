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

namespace AntSK.Domain.Common.DependencyInjection
{
    public static class JesseExtensions
    {
        public static IServiceCollection AddJesseConfiguration(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddHttpClient("JCustom");
            services.AddChatDoc(configuration);

            services.Configure<LocalBgeConfigOptions>(configuration.GetSection("LocalBgeConfig"));


            services.AddKeyedScoped("JKMemory", (services, _) =>
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

            return services;
        }
    }
}
