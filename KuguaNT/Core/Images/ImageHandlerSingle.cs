using ImageMagick;
using Kugua.Core.Algorithms;

namespace Kugua.Core.Images
{
    // 该部分是对单张图片的处理
    public partial class ImageHandler
    {

        // Clamp values to be within 0-65535
        private static int Clamp(int x)
        {
            return x < 0 ? 0 : x > 65535 ? 65535 : x;
        }

        // Convert RGB to YUV
        private static int[] Rgb2Yuv(int r, int g, int b)
        {
            int y = (int)(r * 0.299 + g * 0.587 + b * 0.114);
            int u = (int)(r * -0.168736 + g * -0.331264 + b * 0.500 + 32768);
            int v = (int)(r * 0.500 + g * -0.418688 + b * -0.081312 + 32768);
            return new int[] { Clamp(y), Clamp(u), Clamp(v) };
        }

        // Convert YUV back to RGB
        private static int[] Yuv2Rgb(int y, int u, int v)
        {
            int r = (int)(y + 1.4075 * (v - 32768));
            int g = (int)(y - 0.3455 * (u - 32768) - 0.7169 * (v - 32768));
            int b = (int)(y + 1.7790 * (u - 32768));
            return new int[] { Clamp(r), Clamp(g), Clamp(b) };
        }



        /// <summary>
        /// int -> #FFFFFF
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string IntToHexColor(int color)
        {
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)color;
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// ushort->#ffffff 有损
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string UshortToHexColor(ushort color)
        {
            // 假设 color 是 RGB565 格式
            byte r = (byte)(color >> 11 & 0x1F); // 提取红色分量（5 位）
            byte g = (byte)(color >> 5 & 0x3F);  // 提取绿色分量（6 位）
            byte b = (byte)(color & 0x1F);         // 提取蓝色分量（5 位）

            // 将 5/6 位分量扩展到 8 位
            r = (byte)(r << 3 | r >> 2); // 5 位 -> 8 位
            g = (byte)(g << 2 | g >> 4); // 6 位 -> 8 位
            b = (byte)(b << 3 | b >> 2); // 5 位 -> 8 位

            // 将 R、G、B 转换为 2 位十六进制字符串并拼接
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        ///// <summary>
        ///// 颜色对象转成#FFFFFF这样的代码喵
        ///// </summary>
        ///// <param name="color"></param>
        ///// <returns></returns>
        //public static string GetColorHex(ColorRGB color)
        //{
        //    return $"#{color.R>> 8:X2}{color.G >> 8:X2}{color.B>> 8:X2}";
        //}

        ///// <summary>
        ///// 颜色对象转成(255,252,255)这样的代码喵
        ///// </summary>
        ///// <param name="color"></param>
        ///// <returns></returns>
        //public static string GetColorNum(ColorRGB color)
        //{
        //    return $"({color.R >> 8},{color.G >> 8},{color.B >> 8})";
        //}

        /// <summary>
        /// 画一个纯色正方块
        /// </summary>
        /// <param name="colorCode"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static MagickImage GetColorSample(string colorCode, int size = 80)
        {
            MagickColor color = new MagickColor(colorCode);
            var image = new MagickImage(color, (uint)size, (uint)size);
            image.Format = MagickFormat.Png;
            return image;
        }
        /// <summary>
        /// 获取每个颜色一个小方块的颜色版
        /// </summary>
        /// <param name="colorCodes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static MagickImage GetColorSamples(List<string> colorCodes, int size = 80)
        {
            MagickImageCollection images = new MagickImageCollection();

            foreach (var colorCode in colorCodes)
            {
                images.Add(GetColorSample(colorCode, size));
            }
            return Combine(images, 1);
            //var result = images.Montage(new MontageSettings
            //{
            //    Geometry = new MagickGeometry("+0+0"), // 图像之间的间距
            //    BackgroundColor = MagickColors.Transparent, // 背景颜色
            //    TileGeometry = new MagickGeometry($"{images.Count}x1") // 横向拼接
            //});
            //return (MagickImage)result;

        }



        /// <summary>
        /// 生成特定尺寸随机像素点图片
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static MagickImageCollection ImgGenerateRandomPixel(int size = 100)
        {
            var images = new MagickImageCollection();
            var frame = new MagickImage(
                    MagickColor.FromRgba(255, 255, 255, 255),
                    (uint)size,
                    (uint)size);
            frame.Format = MagickFormat.Gif;
            foreach (var pixel in frame.GetPixelsUnsafe())
            {
                // Generate random x, y components in range [-1, 1]
                double x = (MyRandom.NextDouble * 2.0 - 1.0);
                double y = (MyRandom.NextDouble * 2.0 - 1.0);
                double z = (MyRandom.NextDouble * 2.0 - 1.0);

                // Convert to 0-255 range for image storage
                pixel.SetChannel(0, (ushort)(65535 * (x * 0.5 + 0.5)));
                pixel.SetChannel(1, (ushort)(65535 * (y * 0.5 + 0.5)));
                pixel.SetChannel(2, (ushort)(65535 * (z * 0.5 + 0.5)));
            }
            images.Add(frame);
            return images;

        }

    }
}
