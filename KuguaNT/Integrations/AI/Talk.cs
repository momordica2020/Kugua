using Kugua.Core;
using Kugua.Integrations.NTBot;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Kugua.Integrations.AI
{
    public partial class LLM
    {
        #region 说话模块


        /// <summary>
        /// 预处理加语气、停顿等prompt
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string[] TalkPre(string input)
        {
            string filterdInput = Util.RemoveEmojis(input);
            if (string.IsNullOrWhiteSpace(filterdInput)) return null;
            var sentences = filterdInput.Split(['\r', '\n', '！', '!', '?', '？', '。', '…'], StringSplitOptions.RemoveEmptyEntries);
            List<string> resList = new List<string>();
            string res = "";
            foreach (var s in sentences)
            {
                var ss = s;
                //foreach(var signal in new string[] { "，", ",", "、", "(", "'", "\"", ")", ":", "：", "——", "“", "'", "”" })
                //{
                //    ss=ss.Replace(signal, " ");
                //    //ss = ss.Replace(signal, "[uv_break]");
                //}
                // ss= ss.Replace("\n", "[lbreak]");
                res += ss + "[uv_break]";
                //Logger.Log(res);
                if (res.Length > 100)
                {
                    // 太长了截断了
                    var c = res + "";
                    if (c.EndsWith("[uv_break]")) c = c.Remove(c.Length - "[uv_break]".Length);
                    resList.Add(c);

                    res = "";
                    //break;
                }

            }


            if (!string.IsNullOrWhiteSpace(res))
            {
                if (res.EndsWith("[uv_break]")) res = res.Remove(res.Length - "[uv_break]".Length);
                resList.Add(res);
            }



            //res = res.Replace("哈哈", "哈哈[laugh]");
            //res += "。";
            //res = "[laugh]" + res;// + "[laugh]";

            return resList.ToArray();
        }


        public async Task TalkSingle(MessageContext context, string input, bool reverse)
        {
            try
            {
                Logger.Log($"+))){input}");
                string json = "";
                json += $"text={input}";
                json += $"&prompt={(input.Length < 10 ? "[oral_0][laugh_0][break_0]" : "[oral_3][laugh_5][break_5]")}";
                json += $"&voice=seed_1694_restored_emb-covert.pt";

                json += $"&speed={(input.Length < 10 ? 1 : 1)}";
                json += $"&temperature=0.01";
                json += $"&top_p=0.07";
                json += $"&top_k=15";
                json += $"&refine_max_new_token=384";
                json += $"&infer_max_new_token=2048";
                json += $"&text_seed={MyRandom.Next(40, 50)}";
                //json += $"&text_seed=43";
                json += $"&skip_refine=1";
                json += $"&is_stream=0";
                json += $"&custom_voice=0";
                //json.Add("text", "你好啊");
                //json.Add("prompt", "[break_6]");
                //json.Add("voice", "1031.pt");
                //json.Add("speed", 5);
                //json.Add("temperature", 0.1);
                //json.Add("top_p", 0.701);
                //json.Add("top_k", 20);
                //json.Add("refine_max_new_token", 384);
                //json.Add("infer_max_new_token", 2048);
                //json.Add("text_seed", 42);
                //json.Add("skip_refine", 1);
                //json.Add("is_stream", 0);
                //json.Add("custom_voice", 0);
                string url = Config.Instance.App.Net.TTSUri;
                //{code:0,
                //msg:'ok',
                //audio_files:[{
                //  filename: D:/Projects/chattts/win-ChatTTS-ui-v1.0/static/wavs/1101-234740_4.7s-seed1031.pt-temp0.1-top_p0.701-top_k20-len18-91128-merge.wav,
                //  url: http://127.0.0.1:9966/static/wavs/1101-234740_4.7s-seed1031.pt-temp0.1-top_p0.701-top_k20-len18-91128-merge.wav
                //  }]}
                var responseBody = await Network.PostAsync(url, json);
                JObject jsonResponse = JObject.Parse(responseBody);
                //Logger.Log(responseBody,LogType.Mirai);
                string res = jsonResponse["audio_files"].First()["filename"].ToString();

                if(reverse)
                {
                    res = AudioUtil.WavReverse(res);
                }


                //string amrf = StaticUtil.Wav2Amr(res, 1);
                //var amrb64 = StaticUtil.ConvertFileToBase64(amrf);
                //if (string.IsNullOrWhiteSpace(amrb64)) return;

                //Logger.Log($"=> {amrFile}");
                //var resInc = StaticUtil.WavInc(res);

                context.SendBack([new Record($"file://{res}")]);
                Thread.Sleep(3000);
                //System.IO.File.Delete(amrf);
                //System.IO.File.Delete(resInc);
                // 临时：用于存下来素材用
                File.Copy(res, $"D:/Musics/kuguaaudio/{input}({DateTime.Now.ToString("yyyyMMdd hhmmss")}).wav", true);
                System.IO.File.Delete(res);

                //System.IO.File.Delete(amrFile);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }


        public static string SherpaOnnxTTS(string outputFile, string text, int soundid = 0)
        {
            //inputFile = Path.GetFullPath(inputFile);
            //string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}_Inc.wav";
            string sopath = "D:\\Downloads\\sherpa-onnx-v1.12.14-win-x64-cuda";
            //string model = "vits-icefall-zh-aishell3";
            string model = "vits-zh-hf-eula";

            string cmd = $"{sopath}\\bin\\sherpa-onnx-offline-tts.exe";
            string param = "";
            param = $" --vits-model={sopath}\\{model}\\model.onnx   " +
                $"--vits-lexicon={sopath}\\{model}\\lexicon.txt   " +
                $"--vits-tokens={sopath}\\{model}\\tokens.txt   " +
                $"--vits-dict-dir={sopath}\\{model}\\dict   " +
                $"--sid={soundid}  " +
                $"--output-filename={outputFile}   \"{text}\"";

            //param = $" --vits-model={sopath}\\vits-zh-hf-eula\\eula.onnx   " +
            //    $"--vits-lexicon={sopath}\\vits-zh-hf-eula\\lexicon.txt   " +
            //    $"--vits-tokens={sopath}\\vits-zh-hf-eula\\tokens.txt   " +
            //    $"--vits-dict-dir={sopath}\\vits-zh-hf-eula\\dict   " +
            //    $"--sid={soundid}  " +
            //    $"--output-filename={outputFile}   \"{text}\"";


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

        public async Task TalkSingle2(MessageContext context, string input, bool reverse)
        {
            try
            {
                Logger.Log($"+))){input}");

                string res = SherpaOnnxTTS($"D:\\Musics\\kuguaaudio\\{input}({DateTime.Now.ToString("yyyyMMDD-HHmmss")}).wav",input,0);

                if (reverse)
                {
                    res = AudioUtil.WavReverse(res);
                }


                context.SendBack([new Record($"file://{res}")]);
                Thread.Sleep(3000);
                //System.IO.File.Delete(amrf);
                //System.IO.File.Delete(resInc);
                // 临时：用于存下来素材用
                //File.Copy(res, $"D:/Musics/kuguaaudio/{input}({DateTime.Now.ToString("yyyyMMdd hhmmss")}).wav", true);
                System.IO.File.Delete(res);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
        }




        public async void Talk(MessageContext context, string sentense, bool reverse = false)
        {
            if (string.IsNullOrWhiteSpace(Config.Instance.App.Net.TTSUri)) return;
            var inputs = TalkPre(sentense);
            if (inputs != null)
            {
                foreach (var ipt in inputs)
                {
                    await TalkSingle2(context, ipt, reverse);
                }
            }
        }



        #endregion


    }
}
