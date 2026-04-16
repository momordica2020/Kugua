using ImageMagick;

namespace Kugua.Core.Images
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
        private static List<IMagickColor<ushort>> GetDominantEdgeColor(MagickImage image, int topN = 3, int line_num = 3)
        {
            using (var pixels = image.GetPixels())
            {
                var edgePixels = new List<IMagickColor<ushort>>();

                // 收集顶部和底部边缘像素
                for (int x = 0; x < image.Width; x++)
                {
                    for (int i = 0; i < line_num; i++)
                    {
                        edgePixels.Add(pixels.GetPixel(x, i).ToColor());
                        edgePixels.Add(pixels.GetPixel(x, (int)image.Height - 1 - i).ToColor());
                    }

                    //edgePixels.Add(pixels.GetPixel(x, (int)image.Height - 1).ToColor());
                }

                // 收集左右边缘像素
                for (int y = 0; y < image.Height; y++)
                {
                    for (int i = 0; i < line_num; i++)
                    {
                        edgePixels.Add(pixels.GetPixel(i, y).ToColor());
                        edgePixels.Add(pixels.GetPixel((int)image.Height - 1 - i, y).ToColor());
                    }
                    //edgePixels.Add(pixels.GetPixel(i, y).ToColor());
                    //edgePixels.Add(pixels.GetPixel((int)image.Width - 1, y).ToColor());
                }

                // 统计前 N 个出现次数最多的颜色
                var dominantColors = edgePixels
                    .GroupBy(c => $"{c.R},{c.G},{c.B}") // 使用 RGB 值比较
                    .OrderByDescending(g => g.Count())
                    .Take(topN)
                    .Select(g => edgePixels.First(c => $"{c.R},{c.G},{c.B}" == g.Key))
                    .ToList();

                return dominantColors;

            }
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
