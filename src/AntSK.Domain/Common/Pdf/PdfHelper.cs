using PdfSharp.Drawing;
using PdfSharp.Pdf;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AntSK.Domain.Common.Pdf
{
    public static class PdfHelper
    {
        public static MemoryStream ConvertImageToPdf(Stream imgStream)
        {
            MemoryStream outputStream = new MemoryStream();
            // 创建一个新的PDF文档
            PdfSharp.Pdf.PdfDocument document = new();

            // 添加一个新的页面到PDF文档
            PdfPage page = document.AddPage();

            // 创建XGraphics对象以在页面上绘制
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // 加载图像
            XImage image = XImage.FromStream(imgStream);

            // 根据图像尺寸调整页面大小
            page.Width = image.PixelWidth;
            page.Height = image.PixelHeight;

            // 将图像绘制到页面
            gfx.DrawImage(image, 0, 0, image.PixelWidth, image.PixelHeight);

            // 保存PDF文档
            document.Save(outputStream);

            return outputStream;
        }

        public static List<MemoryStream> ExtractImageFromPdf(Stream pdfStream)
        {
            List<MemoryStream> outputImgStreams = new List<MemoryStream>();
            // 打开PDF文件
            using (var pdf = UglyToad.PdfPig.PdfDocument.Open(pdfStream))
            {
                // 遍历每一页
                foreach (var page in pdf.GetPages())
                {
                    // 提取图像
                    foreach (var image in page.GetImages())
                    {
                        var imageBytes = image.RawBytes.ToArray();
                        var imgStream = new MemoryStream(imageBytes);
                        outputImgStreams.Add(imgStream);
                    }
                }
            }

            return outputImgStreams;
        }
    }
}
