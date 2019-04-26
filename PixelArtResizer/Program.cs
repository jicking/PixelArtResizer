using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelArtResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootDirPath = Directory.GetCurrentDirectory();
            var filePaths = Directory.GetFiles(rootDirPath);
            int counter = 0;

            filePaths = filePaths.Where(c => c.Contains(".png")).ToArray();

            foreach (var path in filePaths)
            {
                var resizedImage = ResizeBitmap(path);
                File.Delete(path);
                resizedImage.Save(path);
                counter++;

                Console.WriteLine($"RESIZED {path}");
            }
            Console.WriteLine($"DONE RESIZING {counter} FILES");
            Console.ReadLine();
        }

        static Bitmap ResizeBitmap(string filePath)
        {
            int newWidth = 0;
            int newHeight = 0;
            Bitmap img ;

            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
            {
                var bmp = new Bitmap(fs);
                img = (Bitmap)bmp.Clone();
                newWidth = img.Size.Width * 2;
                newHeight = img.Size.Height * 2;
            }
            
            return ResizeBitmap(img,newWidth,newHeight);
        }

        static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(sourceBMP, 0, 0, width, height);
            }
            return result;
        }

    }
}
