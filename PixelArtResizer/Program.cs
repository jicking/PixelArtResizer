using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelArtResizer
{
    class Program
    {
        static  int resizeRatio = 2;
        private static bool willMoveFiles;
        private static bool willDoCleanup;
        private static bool willDoResize;

        static void Main(string[] args)
        {
            int.TryParse(ConfigurationManager.AppSettings.Get("resizeRatio"), out resizeRatio);
            bool.TryParse(ConfigurationManager.AppSettings.Get("willMoveFiles"), out willMoveFiles);
            bool.TryParse(ConfigurationManager.AppSettings.Get("willDoCleanup"), out willDoCleanup);

            var root = Directory.GetCurrentDirectory();
            var files = Directory.GetFiles(root);
            int counter = 0;

            files = files.Where(c => c.Contains(".png")).ToArray();

            if (files.Count() == 0)
            {
                willDoResize = false;
                willMoveFiles = false;
                willDoCleanup = false;

                Console.WriteLine($"I have no [png] files to resize.");
            }

            //RESIZE
            if (willDoResize)
            {
                foreach (var path in files)
                {
                    var resizedImage = ResizeBitmap(path);
                    File.Delete(path);
                    resizedImage.Save(path);
                    counter++;

                    Console.WriteLine($"RESIZED {path}");
                }
                Console.WriteLine($"DONE RESIZING {counter} FILES");
                Console.WriteLine();
            }

            //MOVE FILES
            if (willMoveFiles)
            {
                counter = 0;

                foreach (var path in files)
                {
                    try
                    {
                        var filename = Path.GetFileName(path);
                        var rootParent = Directory.GetParent(root).FullName;
                        var exportFolder = filename.Substring(0, filename.IndexOf("-"));
                        var newFileName = filename.Substring(filename.IndexOf("-") + 1);
                        var exportPath = Path.Combine(Path.Combine(rootParent, exportFolder), newFileName);

                        if (!Directory.Exists(Path.GetDirectoryName(exportPath)))
                            continue;

                        if (File.Exists(exportPath))
                            File.Delete(exportPath);

                        File.Copy(path, exportPath);
                        File.Delete(path);

                        Console.WriteLine($"MOVED FILE TO {exportPath}");
                        counter++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                Console.WriteLine($"DONE MOVING {counter} FILES");
                Console.WriteLine();
            }

            //CLEANUP/REMOVE KRITA EXPORT ARTIFACTS
            if (willDoCleanup)
            {
                files = Directory.GetFiles(root);
                counter = 0;

                foreach (var file in files)
                {
                    if(!Path.GetFileName(file).Contains("~"))
                        continue;

                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"DELETED FILE {file}");
                        counter++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                Console.WriteLine($"DONE DELETING {counter} FILES");
                Console.WriteLine();
            }

            Console.WriteLine($"===================================");
            Console.WriteLine($"PixelArtResizer Done. Kill me now.");
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
                newWidth = img.Size.Width * resizeRatio;
                newHeight = img.Size.Height * resizeRatio;
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
