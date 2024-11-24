using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Functions
{
    public class ImgFunctions
    {
        public async Task<string?> HandleImage(string? MediaUrl0)
        {
            var imagePath = await DownloadImageAsync(MediaUrl0);
            var extractedText = ExtractTextFromImage(imagePath);

            return extractedText;
        }



        private async Task<string> DownloadImageAsync(string? mediaUrl)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(mediaUrl);
            response.EnsureSuccessStatusCode();

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");

            await File.WriteAllBytesAsync(filePath, imageBytes);

            return filePath; // Return the path to the downloaded image
        }
        private string ExtractTextFromImage(string imagePath)
        {
            try
            {
                using var engine = new Tesseract.TesseractEngine(@"C:\Program Files\Tesseract-OCR\tessdata", "heb+eng", Tesseract.EngineMode.Default);

         //       using var engine = new Tesseract.TesseractEngine(@"./tessdata", "eng", Tesseract.EngineMode.Default);
                using var img = Tesseract.Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);
                var text = page.GetText();

                return text;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during OCR processing: {e.Message}");
                throw;
            }
        }


    }
}
