using AntSK.Domain.Common;
using AntSK.Domain.Repositories;
using Newtonsoft.Json;
using System.ComponentModel;

namespace AntSK.plugins.Functions
{
    public class FunctionTest(IAIModels_Repositories Repository,IHttpClientFactory httpClientFactory)
    {
        [Description("AntSK:获取订单信息")]
        [return: Description("订单信息")]
        public string GetOrder([Description("订单号")]  int id)
        {
            return $"""
                    订单ID: {id}
                    商品名：小米MIX4
                    数量：1个
                    价格：4999元
                    收货地址：上海市黄浦区
                """;
        }

        [Description("AntSK:获取模型")]
        [return: Description("模型列表")]
        public string GetModels()
        {
            var models = Repository.GetList();
            return string.Join(",", models.Select(x => x.ModelName));
        }

        [Description("AntSK:获取某个用户的工时")]
        [return: Description("用户工时列表信息")]
        public string GetWorkingHours([Description("用户名")] string userName)
        {
            using var httpClient = httpClientFactory.CreateClient("JCustom");
            var response = httpClient.GetStringAsync($"http://172.16.1.160:16080/STARLIMS.PHARMA/YunJiRobot.GetWorkingHours？STARLIMSUser=ZHOUZJ&STARLIMSPass=Lims@123&name={userName}").Result;

            return response;
        }
    }
}