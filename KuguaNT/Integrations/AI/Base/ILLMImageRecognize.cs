using ImageMagick;

namespace Kugua.Integrations.AI.Base
{
    public interface ILLMImageRecognize
    {
        /// <summary>
        /// 对图片进行识别并回答问题，返回的是llm的输出结果。
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="inputImageBase64"></param>
        /// <param name="imgformat"></param>
        /// <returns></returns>
        Task<string> RecognizeImageAsync(string desc, string inputImageBase64, string imgformat);
        //string ChatWithImage(string text, MagickImageCollection img, MessageContext context);

    }
}
