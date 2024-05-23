﻿using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.AI.OpenAI.GPT3;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Python.Runtime.Py;

namespace AntSK.Domain.Domain.Other.Bge
{
    public static class BgeEmbeddingConfig
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
                    //Runtime.PythonDLL = @"D:\Programs\Python\Python311\python311.dll";
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
                            dynamic modelscope = Import("modelscope");
                            //dynamic model_dir = modelscope.snapshot_download("AI-ModelScope/bge-large-zh-v1.5", revision: "master");
                            dynamic model_dir = modelscope.snapshot_download(modelName, revision: "master");
                            dynamic HuggingFaceBgeEmbeddingstemp = Import("langchain_community.embeddings.huggingface");
                            dynamic HuggingFaceBgeEmbeddings = HuggingFaceBgeEmbeddingstemp.HuggingFaceBgeEmbeddings;
                            string model_name = model_dir;
                            dynamic model_kwargs = new PyDict();
                            model_kwargs["device"] = new PyString("cpu");
                            dynamic hugginmodel = HuggingFaceBgeEmbeddings(
                                  model_name: model_dir,
                                  model_kwargs: model_kwargs
                              );
                            model = hugginmodel;
                            return hugginmodel;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        throw ex;
                    }
                    finally
                    {
                        pyGIL.Dispose(); 
                    }
                }
                else
                    return model; 
            }
        }

        public static Task<float[]> GetEmbedding(string queryStr)
        {
            using (GIL())
            {
                PyObject queryResult = model.embed_query(queryStr);
                var floatList = queryResult.As<float[]>();
                return Task.FromResult(floatList); ;
            }
        }

        public static int TokenCount(string queryStr)
        {
            //using (Py.GIL())
            //{
            //    PyObject queryResult = model.client.tokenize(queryStr);
            //    // 使用Python的内置len()函数获取长度
            //    PyObject lenFunc = Py.Import("builtins").GetAttr("len");
            //    PyObject length = lenFunc.Invoke(queryResult["input_ids"]);
            //    int len = length.As<int>(); // 将PyObject转换为C#中的整数
            //    return len;

            //}
            var tokenCount1 = DefaultGPTTokenizer.StaticCountTokens(queryStr);
            return tokenCount1;
        }

        public static void Dispose()
        {
            Console.WriteLine("python dispose");
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
