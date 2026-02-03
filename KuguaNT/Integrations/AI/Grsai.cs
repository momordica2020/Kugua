
using ImageMagick;
using Kugua.Integrations.AI.Base;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using ZhipuApi;

namespace Kugua.Integrations.AI
{
    public class Grsai : LLMBase, ILLMImageGenerate, ILLMImageRecognize
    {
        public string ApiUrl = "https://api.grsai.com/v1/";
        public string ModelName = "gemini-2.5-flash";

        public string ApiUrlNano = "https://grsai.dakka.com.cn/v1/";//draw/
        public string NanoName1 = "nano-banana-fast";
        public string NanoName2 = "nano-banana-pro";
        public ChatClient chatClient;
        public ImageClient imageClient;

        void Init()
        {
            Uri CustomEndpoint = new Uri(ApiUrl);
            var options = new OpenAIClientOptions
            {
                Endpoint = CustomEndpoint
            };
            var openAiClient = new OpenAIClient(new ApiKeyCredential(Config.Instance.App.AI.GeminiApiKey), options);
            chatClient = openAiClient.GetChatClient(ModelName);


            Uri CustomEndpoint2 = new Uri(ApiUrlNano);
            var options2 = new OpenAIClientOptions
            {
                Endpoint = CustomEndpoint2
            };
            var openAiClient2 = new OpenAIClient(new ApiKeyCredential(Config.Instance.App.AI.GeminiApiKey), options2);
            imageClient = openAiClient2.GetImageClient(ModelName);


        }

        public override async Task<string> ChatAsync(ChatContext context)
        {
            if (chatClient == null) Init();

            // 自定义上文：初始化消息列表（系统提示 + 历史对话）
            var messages = context.GptMessageList;

            var completion = chatClient.CompleteChatAsync(messages).Result;
            string answer = completion.Value.Content.First().Text;
            context.AddAssistantText(answer);
            return answer;
        }

        public Task<string> RecognizeImageAsync(string desc, string inputImageBase64, string imgformat)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GenerateImage(string prompt, List<string> inputImagesBase64, string type)
        {
            if(inputImagesBase64 == null)inputImagesBase64 = new List<string>();
            Logger.Log($"生成图片...({prompt})(img:{inputImagesBase64.Count})({type})");


            // 1. 发起绘图请求
            string modelName = NanoName1;
            if(type=="pro")modelName = NanoName2;
            //if (string.IsNullOrWhiteSpace(modelName))modelName = NanoName1;
            var taskId = await CreateDrawingTask(prompt, inputImagesBase64.ToArray(), modelName, "1K", "auto");
            if (string.IsNullOrEmpty(taskId)) return null;

            // 2. 轮询获取结果 URL
            string imageUrl = await PollForResult(taskId);
            if (string.IsNullOrEmpty(imageUrl)) return null;

            Logger.Log("生成图片 URL: " + imageUrl);

            // 3. 将结果 URL 转换为 Base64
            return await DownloadImageAsBase64(imageUrl);
        }











        private static readonly HttpClient client = new HttpClient();


        private async Task<string> CreateDrawingTask(string prompt, string[] base64Image, string modelName, string imageSize="1K", string aspectRatio = "auto")
        {
            var url = $"{ApiUrlNano}draw/nano-banana";

            var payload = new
            {
                model = modelName,
                prompt = prompt,
                urls =  base64Image, // 传入参考图 Base64
                webHook = "-1",              // 填 -1 立即返回 ID 模式
                imageSize = imageSize,
                /*  
                 *  支持模型：
                    nano-banana-pro
                    nano-banana-pro-vt
                    nano-banana-pro-cl
                    nano-banana-pro-vip(只支持1k，2k)
                    nano-banana-pro-4k-vip(只支持4k)
                    -
                    输出图像大小,支持的大小:
                    1K
                    2K
                    4K
                    默认 1K
                    -
                    注意：分辨率越高，生成时间越长
                 */
                aspectRatio = aspectRatio
                /*  auto
                    1:1
                    16:9
                    9:16
                    4:3
                    3:4
                    3:2
                    2:3
                    5:4
                    4:5
                    21:9
                */
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.Instance.App.AI.GeminiApiKey);

            var response = await client.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();
            Logger.Log(json);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.GetProperty("code").GetInt32() == 0)
            {
                return doc.RootElement.GetProperty("data").GetProperty("id").GetString();
            }
            return null;
        }

        private async Task<string> PollForResult(string taskId)
        {
            var url = $"{ApiUrlNano}draw/result";
            var payload = new { id = taskId };

            while (true)
            {
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data))
                {
                    string status = data.GetProperty("status").GetString();
                    if (status == "succeeded")
                    {
                        return data.GetProperty("results")[0].GetProperty("url").GetString();
                    }
                    else if (status == "failed")
                    {
                        Logger.Log($"生成失败: {data.GetProperty("failure_reason").GetString()}({data.GetProperty("error").GetString()})");
                        return null;
                    }
                }

                await Task.Delay(2000); // 等待2秒后重试
            }
        }

        private async Task<string> DownloadImageAsBase64(string imageUrl)
        {
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
            return Convert.ToBase64String(imageBytes);
        }
    }
}
