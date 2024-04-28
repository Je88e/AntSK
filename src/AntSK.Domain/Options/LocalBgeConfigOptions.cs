using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntSK.Domain.Options
{
    public class LocalBgeConfigOptions
    {
        public const string LocalBgeConfig = "LocalBgeConfig";

        public string PythonDllPath { get; set; }
        public string EmbeddingModel { get; set; }
        public string ReRankerModel { get; set; }
    }
}
