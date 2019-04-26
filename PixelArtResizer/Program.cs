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
        static  string fileExtension = ".png";
        private static bool willDoResize = true;
        private static bool willMoveFiles;
        private static bool willDoCleanup;
        
        static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var files = Directory.GetFiles(root);
            int counter = 0;

            Console.Title = "PixelArtResizer V1.0 @jickingx";

            int.TryParse(ConfigurationManager.AppSettings.Get("resizeRatio"), out resizeRatio);
            bool.TryParse(ConfigurationManager.AppSettings.Get("willMoveFiles"), out willMoveFiles);
            bool.TryParse(ConfigurationManager.AppSettings.Get("willDoCleanup"), out willDoCleanup);
            fileExtension = ConfigurationManager.AppSettings.Get("fileExtension");

            if (!fileExtension.Contains("."))
                fileExtension = "." + fileExtension;

            files = files.Where(c => c.Contains(fileExtension)).ToArray();

            if (files.Count() == 0)
            {
                willDoResize = false;
                willMoveFiles = false;
                willDoCleanup = false;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Ooops! Looks like I have no [${fileExtension}] files to resize here.");
                Console.ResetColor();
            }

            //RESIZE
            if (willDoResize)
            {
                foreach (var path in files)
                {
                    try
                    {
                        var resizedImage = ResizeBitmap(path);
                        File.Delete(path);
                        resizedImage.Save(path);
                        counter++;

                        Console.WriteLine($"RESIZED {path}");
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        Console.ResetColor();
                    }

                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"DONE RESIZING {counter} FILES");
                Console.ResetColor();
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
                        var exportPath = Path.Combine(Path.Combine(rootParent, exportFolder), newFileName.ToLower());

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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        Console.ResetColor();
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"DONE MOVING {counter} FILES");
                Console.ResetColor();
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        Console.ResetColor();
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"DONE DELETING {counter} FILES");
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.WriteLine($"===================================");
            Console.WriteLine($"PixelArtResizer was here. Kill me now.");
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
