using Microsoft.AspNetCore.Components.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Kugua.Core
{

    /// <summary>
    /// 音频功能
    /// </summary>
    public static class AudioUtil
    {
        #region silkv3 -> wav

        /// <summary>
        /// 执行本地指令，返回值是成功与否
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool DealProcess(string cmd, string param = "")
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = $"\"{cmd}\"",
                    Arguments = param,
                    CreateNoWindow = true
                };
                process.StartInfo = startInfo;

                process.Start();
                process.WaitForExit();

                int exitCode = process.ExitCode;
                if (exitCode != 0)
                {
                    Logger.Log($"执行程序失败。指令是 {cmd} {param}");
                    return false;
                }
                else
                {
                    Logger.Log($"执行程序成功。指令是 {cmd} {param}");
                    return true;
                }

            }
            catch(Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }


        public static string SilkV32Wav(string inputFile, bool deleteAfterFinish=false)
        {
            if (string.IsNullOrWhiteSpace(inputFile)) return "";
            // 命令行指令
            inputFile = Path.GetFullPath(inputFile);
            string pcmFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.pcm";
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.wav";
            //string cmd1 = "D:\\Downloads\\_software\\silk-v3-decoder-master\\windows\\silk_v3_decoder.exe";
            //string cmd2 = "D:\\ffmpeg\\bin\\ffmpeg.exe";
            //string param1 = $" \"{inputFile}\" \"{pcmFile}\"";
            //string param2 = $" -i \"{inputFile}\" -acodec libmp3lame -y \"{outputFile}\"";
            if(!DealProcess("D:\\Downloads\\_software\\silk-v3-decoder-master\\windows\\silk_v3_decoder.exe", 
                $" \"{inputFile}\" \"{pcmFile}\""))
            {
                //fail
                return "";
            }
            if (!DealProcess("D:\\ffmpeg\\bin\\ffmpeg.exe", 
                $" -f s16le -ar 24000 -ac 1 -i \"{pcmFile}\"  \"{outputFile}\" -y"))
            {
                //fail
                return "";
            }
            
            //File.Delete(pcmFile);
            if (deleteAfterFinish)
            {
                File.Delete(inputFile);
            }

            return outputFile;
        }




        public static string SilkV32Mp3(string inputFile, bool deleteAfterFinish = false)
        {
            if (string.IsNullOrWhiteSpace(inputFile)) return "";
            // 命令行指令
            inputFile = Path.GetFullPath(inputFile);
            string pcmFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.pcm";
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.mp3";
            //string cmd1 = "D:\\Downloads\\_software\\silk-v3-decoder-master\\windows\\silk_v3_decoder.exe";
            //string cmd2 = "D:\\ffmpeg\\bin\\ffmpeg.exe";
            //string param1 = $" \"{inputFile}\" \"{pcmFile}\"";
            //string param2 = $" -i \"{inputFile}\" -acodec libmp3lame -y \"{outputFile}\"";
            if (!DealProcess("D:\\Downloads\\_software\\silk-v3-decoder-master\\windows\\silk_v3_decoder.exe",
                $" \"{inputFile}\" \"{pcmFile}\""))
            {
                //fail
                return "";
            }
            if (!DealProcess("D:\\ffmpeg\\bin\\ffmpeg.exe",
                $" -f s16le -ar 24000 -ac 1 -i \"{pcmFile}\" -codec:a libmp3lame -q:a 2 \"{outputFile}\" -y"))
            {
                //fail
                return "";
            }

            //File.Delete(pcmFile);
            if (deleteAfterFinish)
            {
                File.Delete(inputFile);
            }

            return outputFile;
        }
        #endregion
        #region FFMPEG

        public static string Mp32AmrBase64(string inputFile)
        {
            var f1 = MP32Wav(inputFile);
            if (!string.IsNullOrWhiteSpace(f1))
            {
                var f2 = Wav2Amr(f1, 0);
                if (!string.IsNullOrWhiteSpace(f2))
                {
                    var b64 = ConvertFileToBase64(f2);
                    if(!string.IsNullOrWhiteSpace(b64))
                    {
                        Thread.Sleep(500);
                        File.Delete(f1);
                        File.Delete(f2);

                        return b64;
                    }
                }
            }

            return null;
        }
       
        public static string MP32Wav(string inputFile)
        {
            // 命令行指令
            inputFile = Path.GetFullPath(inputFile);
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.wav";
            string cmd = "D:\\ffmpeg\\bin\\ffmpeg.exe";
            string param = $" -i \"{inputFile}\" -acodec libmp3lame -y \"{outputFile}\"";
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = cmd,
                Arguments = param,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                Logger.Log($"语音合成失败。指令：{cmd} {param}");
                //throw new Exception($"FFmpeg exited with code {exitCode}");
            }
            else
            {
                //var res = ConvertAmrToBase64(outputFile);
                //System.IO.File.Delete(outputFile);
                //return res;
            }

            return outputFile;
        }

        /// <summary>
        /// wav反转
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        public static string WavReverse(string inputFile)
        {
            //inputFile = Path.GetFullPath(inputFile);
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}_Inc.wav";
            string cmd = "ffmpeg";
            string param = "";

            param = $" -i \"{inputFile}\" -af \"areverse\" -y \"{outputFile}\"";


            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = cmd,
                Arguments = param,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                Logger.Log($"语音合成失败。指令：{cmd} {param}");
                //throw new Exception($"FFmpeg exited with code {exitCode}");
            }

            return outputFile;
        }


        /// <summary>
        /// wav增强
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        public static string WavInc(string inputFile)
        {
            inputFile = Path.GetFullPath(inputFile);
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}_Inc.wav";
            string cmd = "ffmpeg";
            string param = "";

            param = $" -i \"{inputFile}\" -af \"volume=3.0,highpass=f=200,lowpass=f=3000,afftdn=nr=50\" -y \"{outputFile}\"";


            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = cmd,
                Arguments = param,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                Logger.Log($"语音合成失败。指令：{cmd} {param}");
                //throw new Exception($"FFmpeg exited with code {exitCode}");
            }

            return outputFile;
        }


        public static string Wav2Amr(string inputFile, int addDB)
        {
            // 命令行指令
            inputFile = Path.GetFullPath(inputFile);
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.amr";
            string cmd = "D:\\ffmpeg\\bin\\ffmpeg.exe";
            string param = "";
            if (addDB != 0)
            {
                //param = $" -i {inputFile} -acodec amr_wb -ar 16000 -ac 1 -filter:a \"loudnorm=i=-14:tp=0.0\" -y {outputFile}";
                //param = $" -i {inputFile} -acodec amr_wb -ar 16000 -ac 1 -filter:a \"volume={addDB}dB\" -y {outputFile}";
                param = $" -i \"{inputFile}\" -c:a amr_nb -b:a 12.20k -ar 8000 -filter:a \"loudnorm=i=-14:tp=0.0\" -y \"{outputFile}\"";
                // param = $" -i {inputFile} -c:a amr_nb -b:a 12.20k -ar 8000 -filter:a \"volume={addDB}dB\" -y {outputFile}";
            }
            else
            {
                    param = $" -i \"{inputFile}\" -acodec amr_wb -ar 16000 -ac 1 -y \"{outputFile}\"";
                //param = $" -i {inputFile} -acodec amr_nb -ab 12.2k -ar 8000 -ac 1 -y {outputFile}";
            }
            
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = cmd,
                Arguments = param,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                Logger.Log($"语音合成失败。指令：{cmd} {param}");
                //throw new Exception($"FFmpeg exited with code {exitCode}");
            }
            else
            {
                //var res = ConvertFileToBase64(outputFile);
                //System.IO.File.Delete(outputFile);
                //return res;
            }

            return outputFile;
        }

        #endregion

        /// <summary>
        /// 从文件读取 .wav 文件并转换为 Base64 字符串
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ConvertFileToBase64(string filePath)
        {
            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                return null;
                //throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            // 读取文件的字节内容
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // 将字节数组转换为 Base64 字符串
            string base64String = Convert.ToBase64String(fileBytes);

            return base64String;
        }





        //public static string Wav2Pcm(string inputFile)
        //{
        //    // 命令行指令
        //    inputFile = Path.GetFullPath(inputFile);
        //    string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.pcm";
        //    string cmd = "ffmpeg";
        //    string param = $" -i {inputFile} -y {outputFile}";
        //    Process process = new Process();
        //    ProcessStartInfo startInfo = new ProcessStartInfo()
        //    {
        //        FileName = cmd,
        //        Arguments = param,
        //        CreateNoWindow = true
        //    };
        //    process.StartInfo = startInfo;

        //    process.Start();
        //    process.WaitForExit();

        //    int exitCode = process.ExitCode;
        //    if (exitCode != 0)
        //    {
        //        Logger.Log($"语音合成失败。指令：{cmd} {param}");
        //        //throw new Exception($"FFmpeg exited with code {exitCode}");
        //    }
        //    return outputFile;

        //}




    }
}
