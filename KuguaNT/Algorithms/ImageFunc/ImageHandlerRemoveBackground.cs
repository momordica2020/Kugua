using ImageMagick;
using Kugua.Core;

namespace Kugua.Algorithms.ImageFunc

{
    // 该部分是去除图片背景相关算法
    public partial class ImageHandler
    {


        public static MagickImage ImgRemoveBackground2(MagickImage image, double tolerance = 10)
        {

            try
            {
                if (image == null) return null;
                // 确保图像支持Alpha通道
                image.Alpha(AlphaOption.Set);

                // 获取边缘像素的主要颜色
                var backgroundColor = GetDominantEdgeColor(image);

                // 设置容差（模糊匹配）
                var fuzz = new Percentage(tolerance);
                image.ColorFuzz = fuzz;

                // 从图像四个角落开始，将接近背景色的区域设为透明
                MakeEdgesTransparent(image, backgroundColor);


            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return image;
        }


        /// <summary>
        /// 获取图像边缘像素的主要颜色
        /// </summary>
        private static List<IMagickColor<ushort>> GetDominantEdgeColor(MagickImage image, int topN = 3, int lineNum = 3, int colorStep = 16)
        {
            var colorCount = new Dictionary<int, (int Count, MagickColor Color)>();

            using var pixels = image.GetPixels();

            int width = (int)image.Width;
            int height = (int)image.Height;

            void AddColor(int x, int y)
            {
                var color = pixels.GetPixel(x, y).ToColor();

                // 量化颜色（减少噪点影响）
                int r = (color.R / colorStep) * colorStep;
                int g = (color.G / colorStep) * colorStep;
                int b = (color.B / colorStep) * colorStep;

                // 用 int 作为 key
                int key = (r << 20) | (g << 10) | b;

                if (colorCount.TryGetValue(key, out var value))
                {
                    colorCount[key] = (value.Count + 1, value.Color);
                }
                else
                {
                    colorCount[key] = (1, new MagickColor((ushort)r, (ushort)g, (ushort)b));
                }
            }

            // 上下边
            for (int x = 0; x < width; x++)
            {
                for (int i = 0; i < lineNum; i++)
                {
                    AddColor(x, i);
                    AddColor(x, height - 1 - i);
                }
            }

            // 左右边（避免重复四角）
            for (int y = lineNum; y < height - lineNum; y++)
            {
                for (int i = 0; i < lineNum; i++)
                {
                    AddColor(i, y);
                    AddColor(width - 1 - i, y);
                }
            }

            return colorCount
                .OrderByDescending(x => x.Value.Count)
                .Take(topN)
                .Select(x => (IMagickColor<ushort>)x.Value.Color)
                .ToList();
        }

        /// <summary>
        /// 将接近背景色的边缘区域设为透明
        /// </summary>
        private static void MakeEdgesTransparent(MagickImage image, List<IMagickColor<ushort>> backgroundColors)
        {
            foreach (var backgroundColor in backgroundColors)
            {
                // 使用FloodFill从四个角落开始，基于容差将背景色区域设为透明
                using (var clone = image.Clone())
                {
                    clone.FloodFill(MagickColors.Transparent, 0, 0, backgroundColor); // 左上角
                    clone.FloodFill(MagickColors.Transparent, (int)image.Width - 1, 0, backgroundColor); // 右上角
                    clone.FloodFill(MagickColors.Transparent, 0, (int)image.Height - 1, backgroundColor); // 左下角
                    clone.FloodFill(MagickColors.Transparent, (int)image.Width - 1, (int)image.Height - 1, backgroundColor); // 右下角

                    // 将处理结果合并回原图像
                    image.Composite(clone, CompositeOperator.CopyAlpha);
                }
            }

        }


        public static byte[] ImgRemoveBackground(byte[] imageBytes)
        {
            using (HttpClient client = new HttpClient())
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new ByteArrayContent(imageBytes), "file", "f1");

                HttpResponseMessage response = client.PostAsync("http://localhost:7799/api/remove", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    byte[] resultImageBytes = response.Content.ReadAsByteArrayAsync().Result;
                    return resultImageBytes;
                    //var ms = new MemoryStream(resultImageBytes);
                    //MagickImage magickImage = new MagickImage(ms);
                    //return magickImage;

                }
                else
                {
                    Logger.Log("Error removing background: " + response.ReasonPhrase);
                    return null;
                }
            }
        }


        public static MagickImage setPixelChange1_2(MagickImage image, double scale = 2)
        {

            try
            {
                if (image == null) return null;
                // 确保图像支持Alpha通道
                image.Alpha(AlphaOption.Set);

                var newWidth = image.Width * scale;
                var newHeight = image.Height * scale;
                var newImage = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), (uint)newWidth, (uint)newHeight);
                var p = image.GetPixels();
                var np = newImage.GetPixels();
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        np.SetPixel((int)(x * scale), (int)(y * scale), p[x, y].ToArray());
                    }
                }
                return newImage;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return image;
        }

        public static MagickImageCollection setPixelChange1(MagickImageCollection images)
        {
            try
            {
                int num = images.Count;
                images.Coalesce();


                for (int i = 0; i < num; i++)
                {

                    images[i] = setPixelChange1_2((MagickImage)images[i]);
                    images[i].GifDisposeMethod = GifDisposeMethod.Background;
                    images[i].AnimationDelay = images[i].AnimationDelay;

                    if (num == 1) images[i].Format = MagickFormat.Png;
                    else images[i].Format = MagickFormat.Gif;

                }

                images.OptimizeTransparency();

                var settings = new QuantizeSettings();
                settings.Colors = 256;

                images.Quantize(settings);
                return images;

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }
        public static MagickImage setPixelChange2_2(MagickImage image)
        {

            try
            {
                if (image == null) return null;
                // 确保图像支持Alpha通道
                image.Alpha(AlphaOption.Set);
                var p = image.GetPixels();
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        if (x % 2 + y % 2 == 1)
                        {
                            p.SetPixel(x, y, [0, 0, 0, 0]);
                        }
                    }
                }
                return image;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return image;
        }
        public static MagickImageCollection setPixelChange2(MagickImageCollection images)
        {
            try
            {
                int num = images.Count;
                images.Coalesce();


                for (int i = 0; i < num; i++)
                {

                    images[i] = setPixelChange2_2((MagickImage)images[i]);
                    images[i].GifDisposeMethod = GifDisposeMethod.Background;
                    images[i].AnimationDelay = images[i].AnimationDelay;

                    if (num == 1) images[i].Format = MagickFormat.Png;
                    else images[i].Format = MagickFormat.Gif;

                }

                images.OptimizeTransparency();

                var settings = new QuantizeSettings();
                settings.Colors = 256;

                images.Quantize(settings);
                return images;

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }


        public static MagickImageCollection ImgRemoveBackgrounds(MagickImageCollection images)
        {
            try
            {
                //MagickImageCollection m2 = new MagickImageCollection();
                //while (m2.Count > 0) m2.RemoveAt(0);
                int num = images.Count;
                images.Coalesce();


                for (int i = 0; i < num; i++)
                {
                    // 2025.6.14 使用新写法
                    //var resb = ImgRemoveBackground(images[i].ToByteArray());
                    //var img = new MagickImage(resb);

                    ImgRemoveBackground2((MagickImage)images[i]);
                    images[i].GifDisposeMethod = GifDisposeMethod.Background;
                    images[i].AnimationDelay = images[i].AnimationDelay;

                    if (num == 1) images[i].Format = MagickFormat.Png;
                    else images[i].Format = MagickFormat.Gif;
                    // img.Transparent(new MagickColor(0, 0, 0,0));
                    //if(i!=0) img.Alpha(AlphaOption.Copy);





                    //images.Add(img);
                }
                //for (int i = 0; i < num; i++)
                //{
                //    images.RemoveAt(0);
                //}
                images.OptimizeTransparency();

                var settings = new QuantizeSettings();
                settings.Colors = 256;

                images.Quantize(settings);
                //images.OptimizeTransparency();
                return images;

                //using (MemoryStream ms = new MemoryStream())
                //{

                //    images.Write(ms,MagickFormat.Gif);
                //    byte[] imageBytes = ms.ToArray();
                //    string base64String = Convert.ToBase64String(imageBytes);
                //    return base64String;
                //}
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }
    }
}
