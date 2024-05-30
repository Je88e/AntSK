using iTextSharp.text;
using iTextSharp.text.pdf;

namespace AntSK.Domain.Common.Pdf
{
    public static class PdfHelper
    {
        public static MemoryStream ConvertImageToPdf(string imgPath)
        {
            using MemoryStream outputStream = new MemoryStream();
            using Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, outputStream);
            document.Open();
            Image image = Image.GetInstance(imgPath);
            image.SetAbsolutePosition(0, 0);
            image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
            document.Add(image);
            document.NewPage();
            document.Close();
            return outputStream;
        }

        public static MemoryStream ConvertImageToPdf(byte[] imgBytes)
        {
            using MemoryStream outputStream = new MemoryStream();
            using Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, outputStream);
            document.Open();
            Image image = Image.GetInstance(imgBytes);
            image.SetAbsolutePosition(0, 0);
            image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
            document.Add(image);
            document.NewPage();
            document.Close();
            return outputStream;
        }
    }
}
