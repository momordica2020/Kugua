using ImageMagick;
using Kugua.Core.Algorithms;
using Kugua.Integrations.NTBot;
using System.Diagnostics;

namespace Kugua.Core.Images
{
    public class Esrgan
    {
        public static string TempDir = "Temp";
        public static string EsrganPath = $"D:\\Projects\\Real-ESRGAN\\inference_realesrgan.py";

        public static MagickImage SingleImageEsrgan(MagickImage inputImage)
        {
            (string fileInputName, string fileOutputName) = GetRandomFilePath(inputImage);
            Logger.Log($"{fileInputName} -> {fileOutputName}");
            inputImage.Write(fileInputName);

            if (RunPythonEsrgan(fileInputName, Path.GetDirectoryName(fileInputName)))
            {
                var upscaledFrame = new MagickImage(fileOutputName);

                // 保持动图属性
                upscaledFrame.AnimationDelay = inputImage.AnimationDelay;
                upscaledFrame.AnimationIterations = inputImage.AnimationIterations;

                // 可选：如果不需要保留磁盘上的临时输入文件，可以删除
                if (File.Exists(fileInputName)) File.Delete(fileInputName);
                if (File.Exists(fileOutputName)) File.Delete(fileOutputName);

                return upscaledFrame;
            }
            else
            {
                return inputImage;
            }

                //oriImg.Dispose();
                //if (processedCollection.Count > 0) imgs.Add(processedCollection);
            
        }


        /// <summary>
        /// 调用python命令行执行esrgan处理图像
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="modelName"></param>
        /// <param name="extParam"></param>
        /// <returns></returns>
        static bool RunPythonEsrgan(string input, string output, string modelName = "RealESRGAN_x4plus",string extParam = "--face_enhance")
        {

            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $" \"{EsrganPath}\" -n {modelName} -i \"{input}\" -o \"{output}\" {extParam}",
                    //UseShellExecute = false,
                    CreateNoWindow = true,
                    //RedirectStandardError = true
                };
                Logger.Log($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }

        static (string inputFile, string outputFile) GetRandomFilePath(MagickImage image)
        {
            string ext = "jpg";
            try
            {
                ext = image.Format.ToString().ToLower();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                ext = "jpg";
            }
            string path = Config.Instance.FullPath(TempDir);
            string inputFile = $"{path}\\{MyRandom.NextString(10)}.{ext}";
            string outputFile = $"{path}\\{Path.GetFileNameWithoutExtension(inputFile)}_out.{ext}";
            return (inputFile, outputFile);
        }

        //static (string inputFile, string outputFile) GetFrameFilePath(string filename,int index)
        //{
        //    string inputFile = $"{Path.GetFileNameWithoutExtension(filename)}_{index:D5}.png";
        //    string outputFile = $"{Path.GetFileNameWithoutExtension(filename)}_{index:D5}_out.png";
        //    return (inputFile, outputFile);
        //}
    }
}
