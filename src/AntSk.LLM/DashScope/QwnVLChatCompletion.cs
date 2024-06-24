using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using Sdcb.SparkDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Sdcb.DashScope;

namespace AntSK.LLM.DashScope
{
    public class QwenVLChatCompletion : IChatCompletionService
    {
        private readonly Dictionary<string, object?> _attributes = new();
        private readonly DashScopeClient _client; 
        private readonly DashScopeOptions _options;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public IReadOnlyDictionary<string, object?> Attributes => _attributes;

        public QwnVLChatCompletion(DashScopeOptions options)
        {
            _options = options;
            _client = new(options.ApiKey);
        }

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            

        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            

        }

    }
}
