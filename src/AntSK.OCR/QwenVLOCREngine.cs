using BifrostiC.DashScope.OSSUpload.Services.OSSUpload;
using Microsoft.KernelMemory.DataFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntSK.OCR
{
    public class QwenVLOCREngine : IOcrEngine
    {
        private readonly IOSSUploadService _uploadService;
        public QwenVLOCREngine(IOSSUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        public async Task<string> ExtractTextFromImageAsync(Stream imageContent, CancellationToken cancellationToken = default)
        {
            //获取上传凭证
            var policyResponse = await _uploadService.GetUploadPolicyAsync("qwen-vl-plus");
            var uploadResponse = await _uploadService.FileUpload(policyResponse, imageContent, $"{Guid.NewGuid().ToString()}.jpg", "image/jpeg");

            if(!uploadResponse.Success)
            {
                return "";
            }

            var ossEndPoint = uploadResponse.OssEndpoint;



        }
    }
}
