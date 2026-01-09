using ImageMagick;

namespace Kugua.Integrations.AI.Base
{
    public interface ILLMImageGenerate
    {
        /// <summary>
        /// 图片生成接口
        /// </summary>
        /// <param name="input">生成描述，必填</param>
        /// <param name="inputImagesBase64">输入的样图，可为null</param>
        /// <param name="type">类型标志，用于选择清晰度、尺寸或内部模型类别等</param>
        /// <returns></returns>
        Task<string> GenerateImage(string input, List<string> inputImagesBase64, string type);
        //string ChatWithImage(string text, MagickImageCollection img, MessageContext context);
    }
}
