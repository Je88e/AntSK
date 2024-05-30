using AntSK.Domain.Domain.Other.Bge;
using AntSK.Domain.Options;
using Microsoft.Extensions.Options;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Python.Runtime.Py;

namespace AntSK.Domain.Common.Embedding
{
    public class GlobalBgeReranker
    {
        private readonly IOptions<LocalBgeConfigOptions> _bgeOptions;
        public dynamic model { get; set; }

        public GlobalBgeReranker(IOptions<LocalBgeConfigOptions> bgeOptions)
        {
            this._bgeOptions = bgeOptions;
            model = BegRerankConfig.LoadModel(_bgeOptions.Value.PythonDllPath, _bgeOptions.Value.ReRankerModel);
        }

        public double Rerank(List<string> list)
        {
            using var pyGIL = GIL();
            try
            {
                PyList pyList = new PyList();
                foreach (string item in list)
                {
                    pyList.Append(item.ToPython()); // 将C# string转换为Python对象并添加到PyList中
                }
                PyObject result = model.compute_score(pyList, normalize: true);
                return result.As<double>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                pyGIL.Dispose();
            }
        }
    }
}
