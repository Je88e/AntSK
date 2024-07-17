using Newtonsoft.Json;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Python.Runtime.Py;

namespace AntSK.Domain.Domain.Other.Bge
{
    public static class BegRerankConfig
    {
        public static dynamic model { get; set; }

        static object lockobj = new object(); 

        /// <summary>
        /// 模型写死
        /// </summary>
        public static dynamic LoadModel(string pythondllPath, string modelName)
        {
            lock (lockobj)
            {
                if (model == null)
                {
                    if (string.IsNullOrEmpty(Runtime.PythonDLL))
                    {
                        HandleEnvirometVariablesPath(ref pythondllPath);
                        Runtime.PythonDLL = pythondllPath;
                    }
                    PythonEngine.Initialize();
                    PythonEngine.BeginAllowThreads();
                    using var pyGIL = GIL();
                    try
                    {
                       
                            dynamic modelscope = Py.Import("modelscope");
                            dynamic flagEmbedding = Py.Import("FlagEmbedding");

                            dynamic model_dir = modelscope.snapshot_download(modelName, revision: "master");
                            dynamic flagReranker = flagEmbedding.FlagReranker(model_dir, use_fp16: true);
                            model = flagReranker;
                            return model;
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
                else
                {
                    return model;
                }
            }
        }


        public static double Rerank(List<string> list)
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

        private static void HandleEnvirometVariablesPath(ref string path)
        {
            string[] segments = path.Split('\\');
            StringBuilder expandedPath = new StringBuilder();

            foreach (string segment in segments)
            {
                if (segment.StartsWith('%') && segment.EndsWith('%'))
                {
                    string variableName = segment.Substring(1, segment.Length - 2);
                    string variableValue = Environment.ExpandEnvironmentVariables("%" + variableName + "%");
                    expandedPath.Append(variableValue);
                }
                else
                {
                    expandedPath.Append(segment);
                }
                expandedPath.Append('\\');
            }

            expandedPath.Remove(expandedPath.Length - 1, 1); // Remove the trailing backslash

            path = expandedPath.ToString();
        }
    }
}
