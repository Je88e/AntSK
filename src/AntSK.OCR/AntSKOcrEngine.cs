using Microsoft.KernelMemory.DataFormats;
using OpenCvSharp;
using Sdcb.OpenVINO;
using Sdcb.OpenVINO.PaddleOCR;
using Sdcb.OpenVINO.PaddleOCR.Models;
using Sdcb.OpenVINO.PaddleOCR.Models.Online;
using Sdcb.PaddleOCR.Models.Local;

namespace AntSK.OCR
{
    /// <summary>
    /// OCR
    /// </summary>
    public class AntSKOcrEngine : IOcrEngine
    {
        FullOcrModel model;
        TableRecognitionModel tableModel;
        public Task<string> ExtractTextFromImageAsync(Stream imageContent, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null)
                {
                    model = OnlineFullModels.ChineseV4.DownloadAsync().Result;
                }
                if (tableModel == null)
                {
                    tableModel = OnlineTableRecognitionModel.ChineseMobileV2_SLANET.DownloadAsync().Result;
                }
                Sdcb.PaddleOCR.PaddleOcrTableRecognizer tableRe1 = new(LocalTableRecognitionModel.ChineseMobileV2_SLANET);
                //var mt = new Sdcb.PaddleOCR.Models.FullOcrModel(LocalFullModels.ChineseV4.DetectionModel, LocalFullModels.ChineseV4.ClassificationModel, LocalFullModels.ChineseV4.RecognizationModel);
                using (PaddleOcrAll all = new(model)
                {
                    AllowRotateDetection = true,
                    Enable180Classification = true,
                })
                {
                    var core = new OVCore();
                    var rec_model = tableModel.CreateOVModel(core);
                    rec_model.ReshapePrimaryInput
                    foreach (var input_layer in rec_model.Inputs)
                    {
                        var input_shape = input_layer.PartialShape;
                        input_shape[3] = -1;
                        rec_model.ReshapePrimaryInput(input_shape);
                    }   

                    var rec_complied_model = core.CompileModel(rec_model);

                    //PaddleOcrTableRecognizer tableRec = new(TableRecognitionModel.FromDirectory(@"D:\JC\GitProjects\PaddleOCRSharp-dev\PaddleOCRSharp-dev\PaddleOCRDemo\PaddleOCRSharpDemo\bin\Debug\inference\ch_ppstructure_mobile_v2.0_SLANet_infer", @"D:\JC\GitProjects\PaddleOCRSharp-dev\PaddleOCRSharp-dev\PaddleOCRDemo\PaddleOCRSharpDemo\bin\Debug\inference\table_structure_dict_ch.txt"));
                    //Sdcb.PaddleOCR.PaddleOcrTableRecognizer tableRec = new (LocalTableRecognitionModel.ChineseMobileV2_SLANET);
                    PaddleOcrTableRecognizer tableRec = new(tableModel);
                    //using Mat src = Cv2.ImRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "100000005&葡萄籽提取物原花青素.jpg"));



                    Mat src = Cv2.ImDecode(StreamToByte(imageContent), ImreadModes.Color);

                    // Table detection
                    TableDetectionResult tableResult = tableRec.Run(src);

                    all.Detector.UnclipRatio = 1.2f;
                    PaddleOcrResult ocrResult = all.Run(src); 

                    string html = tableResult.RebuildTable(ocrResult);

                    Console.WriteLine(html);
                    return Task.FromResult(html);
                    //return Task.FromResult(result.Text);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult("");
            }
        }

        private byte[] StreamToByte(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[1024]; //自定义大小，例如 1024
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }
                byte[] bytes = memoryStream.ToArray();
                return bytes;
            }
        }
    }
}
