using ImageMagick;
using ImageMagick.Colors;
using ImageMagick.Drawing;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using ZhipuApi.Modules;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kugua.Core
{
    /// <summary>
    /// 图像处理相关
    /// </summary>
    public class ImageUtil
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



        public static MagickImage GetGifFrames(MagickImageCollection gif, int totalCols = 1)
        {
            if (totalCols <= 0) totalCols = 1;
            int totalRows = (int)Math.Ceiling((double)gif.Count / totalCols);
            gif.Coalesce();
            // 加载所有图片
            var images = new List<MagickImage>();
            foreach (var path in gif)
            {
                images.Add(new MagickImage(path));
            }

            // 计算每个图像的宽度和高度
            var imageWidth = images[0].Width;
            var imageHeight = images[0].Height;

            // 计算拼接图像的总宽度和高度
            var totalWidth = imageWidth * totalCols;
            var totalHeight = imageHeight * totalRows;

            // 创建一个新的空白图片用于存放拼接后的图像
            var result = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), (uint)totalWidth, (uint)totalHeight);

            // 拼接图像
            for (int col = 0; col < totalCols; col++)
            {
                for (int row = 0; row < totalRows; row++)
                {
                    int x = (int)(col * imageWidth);  // 水平偏移
                    int y = (int)(row * imageHeight); // 垂直偏移

                    // 将当前图片放置到拼接图像中
                    var findex = col * totalRows + row;
                    if (findex < images.Count) result.Composite(images[findex], x, y, CompositeOperator.Over);
                }
            }

            // 保存拼接后的图像
            result.Format = MagickFormat.Png;
            return result;

        }


        /// <summary>
        /// 图片旋转
        /// </summary>
        /// <param name="images"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        public static MagickImageCollection ImgRotate(MagickImageCollection images, double rotate)
        {
            if (images == null) return null;

            foreach (var image in images)
            {
                image.Rotate(rotate);
            }
            return images;

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    images.Write(ms);
            //    byte[] imageBytes = ms.ToArray();
            //    string base64String = Convert.ToBase64String(imageBytes);
            //    return base64String;
            //}
        }





        /// <summary>
        /// 图像旋转，传入下刀方位
        /// </summary>
        /// <param name="images"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static MagickImageCollection ImgMirror(MagickImageCollection images, double degree)
        {
            if (images == null) return null;
            var outputGif = new MagickImageCollection();
            images.Coalesce();
            foreach (var image in images)
            {
                // 获取图片宽高
                uint width = image.Width;
                uint height = image.Height;
                int pos = (int)degree;

                // 裁剪右半部分
                MagickGeometry rightHalfGeometry = new MagickGeometry((int)(width / 2), 0, width / 2, height);
                using (var cropped = image.Clone())
                {
                    var outputImage = (MagickImage)image.Clone();
                    switch (pos)
                    {
                        case 1:
                            cropped.Crop(new MagickGeometry(0, 0, width / 2, height)); // 裁切左半部分
                            cropped.Flop(); // 水平翻转

                            outputImage.Composite(cropped, (int)width / 2, 0, CompositeOperator.Src); // 覆盖到右侧
                            break;

                        case 2:
                            cropped.Crop(new MagickGeometry((int)width / 2, 0, width / 2, height)); // 裁切右半部分
                            cropped.Flop();

                            outputImage.Composite(cropped, 0, 0, CompositeOperator.Src); // 覆盖到左侧
                            break;

                        case 3:
                            cropped.Crop(new MagickGeometry(0, 0, width, height / 2)); // 裁切上半部分
                            cropped.Flip(); // 垂直翻转

                            outputImage.Composite(cropped, 0, (int)height / 2, CompositeOperator.Src); // 覆盖到下半部分
                            break;

                        case 4:
                            cropped.Crop(new MagickGeometry(0, (int)height / 2, width, height / 2)); // 裁切下半部分
                            cropped.Flip(); // 垂直翻转

                            outputImage.Composite(cropped, 0, 0, CompositeOperator.Src); // 覆盖到上半部分
                            break;
                        default: break;
                    }
                    outputImage.ColorFuzz = new Percentage(5);
                    outputGif.Add(outputImage);
                }
            }
            var settings = new QuantizeSettings();
            settings.Colors = 256;

            outputGif.Quantize(settings);
            outputGif.OptimizeTransparency();

            return outputGif;
            //using (MemoryStream ms = new MemoryStream())
            //{


            //    //outputGif.Write(ms);
            //    //byte[] imageBytes = ms.ToArray();
            //    //string base64String = Convert.ToBase64String(imageBytes);
            //    //return base64String;
            //}
        }



        public static MagickImageCollection ImgCut(MagickImageCollection images, int top, int bottom, int left, int right)
        {
            if (images == null) return null;
            var oriWidth = images.First().Width;
            var oriHeight = images.First().Height;
            uint newWidth = oriWidth - (uint)left - (uint)right;
            uint newHeight = oriHeight - (uint)top - (uint)bottom;
            if (images.Count == 1)
            {
                // single img
                images.First().Crop(new MagickGeometry(left, top, (uint)newWidth, (uint)newHeight));
                return images;
            }

            MagickImageCollection newImages = new MagickImageCollection();
            images.Coalesce();
            foreach (var frame in images)
            {
                MagickImage newFrame = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), newWidth, newHeight);
                newFrame.Format = MagickFormat.Gif;
                newFrame.AnimationDelay = frame.AnimationDelay;
                newFrame.Composite(frame, left, top, CompositeOperator.Over);
                newFrame.GifDisposeMethod = GifDisposeMethod.Background;
                newImages.Add(newFrame);
            }
            newImages.Optimize();

            return newImages;
        }



        /// <summary>
        /// gif图片调速
        /// </summary>
        /// <param name="images"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static MagickImageCollection ImgSetGifSpeed(MagickImageCollection images, double speed)
        {
            if (images == null) return null;
            images.Coalesce();
            if (images.Count <= 1)
            {
                //skip;
            }
            else if (Math.Abs(speed) <= 0.001)
            {
                // use first img
                //var imgfirst = images.First();
                while (images.Count > 1) images.RemoveAt(1);
            }
            else
            {
                if (speed < 0)
                {
                    images.Reverse();
                }
                // 初步的动画长度（以延迟总和表示）
                uint originalDelaySum = 0;
                foreach (var image in images)
                {
                    originalDelaySum += image.AnimationDelay;
                }

                // 计算加速后 GIF 的目标总时长
                uint targetDelaySum = (uint)(originalDelaySum / Math.Abs(speed));

                // 判断每帧的 delay 是否有小于 2 的情况
                bool needsFrameRemoval = false;
                foreach (var image in images)
                {
                    if (image.AnimationDelay / Math.Abs(speed) < 2)
                    {
                        needsFrameRemoval = true;
                        break;
                    }
                }

                if (needsFrameRemoval)
                {
                    int targetFrameCount = (int)(images.Count / Math.Abs(speed));
                    // 计算实际应该保留多少帧
                    targetFrameCount = Math.Max(2, targetFrameCount); // 至少保留 2 帧
                    // 抽帧处理：按加速比率删除帧
                    List<MagickImage> newImages = new List<MagickImage>();
                    int step = (int)Math.Ceiling((double)images.Count / targetFrameCount);

                    for (int i = 0; i < images.Count; i += step)
                    {
                        newImages.Add(new MagickImage(images[i]));
                    }

                    // 更新 images 为新的帧列表
                    while (images.Count > 0) images.RemoveAt(0);
                    images.AddRange(newImages);


                    // 计算新的 delay，使得总的动画长度符合加速后的比例
                    uint newDelaySum = targetDelaySum;
                    uint totalNewDelay = 0;
                    foreach (var image in images)
                    {
                        // 计算每帧的新的 delay，使总动画时长符合目标
                        uint newDelay = (uint)(newDelaySum / images.Count);

                        image.AnimationDelay = newDelay;
                        if (image.AnimationDelay < 2)
                        {
                            image.AnimationDelay = 2;
                        }
                        totalNewDelay += newDelay;
                    }
                }
                else
                {
                    // 加速比率较低，直接调整每帧的 AnimationDelay
                    foreach (var image in images)
                    {
                        // 按照 speed 调整动画延迟
                        image.AnimationDelay = (uint)(image.AnimationDelay / Math.Abs(speed));

                        // 保证最小延迟为 2
                        if (image.AnimationDelay < 2)
                        {
                            image.AnimationDelay = 2;
                        }

                        image.ColorFuzz = new Percentage(5);  // 保持颜色模糊度
                    }
                }
            }
            var settings = new QuantizeSettings();
            settings.Colors = 256;

            images.Quantize(settings);
            images.OptimizeTransparency();
            return images;
            //using (MemoryStream ms = new MemoryStream())
            //{

            //    images.Write(ms);
            //    byte[] imageBytes = ms.ToArray();
            //    string base64String = Convert.ToBase64String(imageBytes);
            //    return base64String;
            //}
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


        /// <summary>
        /// 图像逐帧拼合成一张大图
        /// </summary>
        /// <param name="images"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static MagickImage Combine(MagickImageCollection images, int row = 1)
        {
            List<MagickImage> imsg = new List<MagickImage>();
            foreach (var img in images) imsg.Add((MagickImage)img);
            return Combine(imsg, row);
        }

        public static MagickImage Combine(List<MagickImage> images, int Rows = 1)
        {
            if (images == null || images.Count <= 0) return null;
            int Cols = (int)Math.Ceiling((double)images.Count / Rows);

            var singleWidth = images[0].Width;
            var singleHeight = images[0].Height;

            var totalWidth = singleWidth * Cols;
            var totalHeight = singleHeight * Rows;

            var result = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), (uint)totalWidth, (uint)totalHeight);

            // 拼接图像
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    int x = (int)(col * singleWidth);  // 水平偏移
                    int y = (int)(row * singleHeight); // 垂直偏移

                    // 将当前图片放置到拼接图像中
                    var findex = row * Cols + col;
                    if (findex < images.Count) result.Composite(images[findex], x, y, CompositeOperator.Over);
                }
            }
            result.Format = MagickFormat.Png;
            return result;
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



        //static int[] histograms = new int[16777216];
        //// 提取图片颜色，但是靠像素众数排序
        //public static int[] ImageColorExtract2(MagickImageCollection images, int colorCount = 3)
        //{
        //    // 初始化直方图数组（24 位精度）

        //    Array.Clear(histograms, 0, histograms.Length);
        //    using (var image = images.First())
        //    {
        //        // 获取像素数据
        //        using (var pixels = image.GetPixelsUnsafe())
        //        {
        //            var areaPointer = pixels.GetAreaPointer(0, 0, image.Width, image.Height);

        //            // 并行处理每个通道的直方图
        //            Parallel.For(0, image.Height, y =>
        //            {
        //                for (int x = 0; x < image.Width; x++)
        //                {
        //                    var hexColor = pixels.GetPixel(x, (int)y).ToColor().ToByteArray();

        //                    // 组合为 ushort 值
        //                    uint colorushort = (uint)((hexColor[0] << 16) | (hexColor[1] << 8) | hexColor[2]);
        //                    //ushort colorushort = (ushort)(((color.R >> 8) << 11) + ((color.G >> 8) << 5) + (color.B >> 8));
        //                    histograms[colorushort] = (int)Math.Min((uint)(histograms[colorushort] + 1), int.MaxValue);
        //                }
        //            });
        //        }
        //    }
        //    return histograms
        //     .Select((value, index) => new { Value = value, Index = index })
        //     .OrderByDescending(x => x.Value)
        //     .Take(colorCount)
        //     .Select(x => (int)x.Index)
        //     .ToArray();
        //}





        /// <summary>
        /// 将图片抽象为几种颜色
        /// </summary>
        /// <param name="image"></param>
        /// <param name="colorCount"></param>
        /// <returns></returns>
        public static List<MagickColor> ImageColorExtract(MagickImageCollection images, int colorCount = 3)
        {
            List<MagickColor> mainColors = new List<MagickColor>();

            // 设置量化参数，提取指定数量的颜色
            var settings = new QuantizeSettings
            {
                Colors = (uint)colorCount, // 提取的颜色数量
                DitherMethod = DitherMethod.No // 不使用抖动
            };
            images.Quantize(settings);
            for (int i = 0; i < colorCount; i++)
            {
                var color = images.FirstOrDefault().GetColormapColor(i);
                mainColors.Add(new MagickColor(color.R, color.G, color.B));
            }

            return mainColors;
        }




        /// <summary>
        /// 处理图片做旧，返回base64编码的图像数据
        /// </summary>
        /// <param name="images"></param>
        /// <param name="iterations"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static MagickImageCollection ImgGreen(MagickImageCollection images, int iterations = 16, double quality = 0.75)
        {
            Logger.Log($"{iterations},{quality}");
            if (images == null) return null;

            images.Coalesce();
            //int no  = 0;
            foreach (var image in images)
            {
                //Logger.Log("IMG " + image.Height + "," + image.Width);
                uint oriW = image.Width;
                uint oriH = image.Height;
                if (quality < 0.5)
                {
                    uint smallW = (uint)(oriW * Math.Min(1, quality * 1.5));
                    uint smallH = (uint)((double)oriH / image.Width * smallW);
                    //Logger.Log($"{oriW},{oriH} => {smallW},{smallH}");
                    image.Resize(smallW, smallH);
                    image.Resize(oriW, oriH);
                }

                //if (no++ < 5) Logger.Log("! " + image.ColorFuzz);
                image.ColorFuzz = new Percentage(5);

                //Bitmap resultImage = new Bitmap((int)(image.Width), (int)(image.Height));
                var pixels = image.GetPixels();
                foreach (var pixel in pixels)
                {
                    var pixelColor = pixel.ToColor();
                    for (int iter = 0; iter < iterations; iter++)
                    {

                        var yuv = Rgb2Yuv(pixelColor.R, pixelColor.G, pixelColor.B);
                        // Apply green shift (UV shift)
                        yuv[1] = yuv[1] - 255; // Slight shift to simulate the green effect
                                               // Convert back to RGB and set the pixel
                        int[] rgb = Yuv2Rgb(yuv[0], yuv[1], yuv[2]);
                        pixelColor.R = (ushort)rgb[0];
                        pixelColor.G = (ushort)rgb[1];
                        pixelColor.B = (ushort)rgb[2];
                    }
                    pixel.SetChannel(0, pixelColor.R);//.SetPixel(x, y, Color.FromArgb(rgb[0], rgb[1], rgb[2]));
                    pixel.SetChannel(1, pixelColor.G);
                    pixel.SetChannel(2, pixelColor.B);
                    // pixels.SetPixel(pixel);

                }

            }
            //MemoryStream ms = new MemoryStream();

            //images.Optimize();
            //images.OptimizePlus();

            var settings = new QuantizeSettings();
            settings.Colors = 256;

            images.Quantize(settings);
            images.OptimizeTransparency();
            for (int i = 0; i < iterations; i++)
            {

                using (MemoryStream mss = new MemoryStream())
                {
                    foreach (var image in images)
                    {
                        //image.Format = MagickFormat.Jpeg;
                        image.Quality = (uint)(quality * 90 * MyRandom.NextDouble);
                        //image.Write(ms);
                    }
                    images.Write(mss);
                    byte[] imageBytess = mss.ToArray();
                    images = new MagickImageCollection(imageBytess);
                }
            }
            return images;
            //images.Write(ms);
            //byte[] imageBytes = ms.ToArray();
            ////Logger.Log("bytes => " + imageBytes.Length);
            //string base64String = Convert.ToBase64String(imageBytes);
            //return base64String;

        }




        public static string GetBase64Image(Bitmap image, int quality)
        {
            byte[] imageBytes = Array.Empty<byte>();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                    if (jpegCodec == null)
                    {
                        throw new InvalidOperationException("JPEG 编码器不可用！");
                    }

                    // 设置编码参数
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);


                    // 保存 Bitmap 到指定流
                    image.Save(memoryStream, jpegCodec, encoderParams);
                    imageBytes = memoryStream.ToArray();


                    return Convert.ToBase64String(imageBytes);
                }
                catch (Exception ex)
                {
                    Logger.Log($"压缩时发生错误: {ex.Message}");
                }
            }

            return null;
        }

        static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType.Equals(mimeType, StringComparison.OrdinalIgnoreCase))
                {
                    return codec;
                }
            }
            throw new InvalidOperationException($"找不到 MIME 类型为 {mimeType} 的编码器！");
        }




        static Bitmap ConvertBytesToBitmap(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) return null;


            using (MemoryStream memoryStream = new MemoryStream(imageBytes))
            {
                try
                {
                    // 从内存流加载 Bitmap
                    return new Bitmap(memoryStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载 Bitmap 时出错: {ex.Message}");
                    return null;
                }
            }
        }


        // Save processed image
        private static void SaveImage(Bitmap image, string outputImagePath, int quality)
        {
            // Get the JPEG encoder
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

            // Create EncoderParameters object to set the quality
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            // Save the image as JPEG
            image.Save(outputImagePath, jpegCodec, encoderParams);
            Logger.Log($"Image saved to: {outputImagePath}", LogType.Debug);
        }

        // Helper function to get encoder info for the desired format (JPEG)



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

        public static string[] Fonts = [
            "华文中宋"  ,
            "华文仿宋",
            "华文宋体",
            "华文彩云",
            "华文新魏",
            "华文楷体",
            "华文琥珀",
            "华文细黑",
            "华文行楷",
            "华文隶书",
            "幼圆",
            "方正姚体",
            "方正舒体",
            "隶书",
            ];

        public static MagickColor[] FontColors = [
            MagickColor.FromRgb(0,0,0),// black
            MagickColor.FromRgb(255,0,0),// red
            MagickColor.FromRgb(255,255,0),// yellow
            MagickColor.FromRgb(0,255,0),// green
            MagickColor.FromRgb(0,255,255),// green-blue
            MagickColor.FromRgb(0,0,255),// blue
            MagickColor.FromRgb(255,0,255),// purple
            ];



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


        public static void ShowFonts()
        {
            InstalledFontCollection MyFont = new InstalledFontCollection();
            FontFamily[] MyFontFamilies = MyFont.Families;
            foreach (var f in MyFontFamilies)
            {
                Logger.Log(f.Name);
            }
        }


        public static MagickImage ImgGeneratePixel2(string text, string fontName, int fontSize)
        {
            int lineMax = 20;
            int lineNum = text.Length / lineMax;
            int slide = (int)(fontSize * 0.1);
            int width = (int)(fontSize * Math.Min(lineMax, text.Length) + slide * 2);
            int height = (int)(fontSize * (lineNum + 1) + slide * 2);

            // 创建位图
            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // 设置字体
                System.Drawing.Font font = new System.Drawing.Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                g.Clear(Color.Transparent);
                for (int i = 0; i < text.Length; i++)
                {
                    g.DrawString(text[i].ToString(), font, Brushes.Red,
                        (int)(i % lineMax * fontSize),
                        (int)(i / lineMax * fontSize)); // 绘制文本
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png); // 将 Bitmap 保存到内存流
                    ms.Position = 0;
                    var image = new MagickImage(ms);
                    return image;
                }
            }
        }

        /// <summary>
        /// 生成像素字图片
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static MagickImage ImgGeneratePixel(string text, FontFamily family)
        {
            //ShowFonts();


            uint fontsize = 12;
            int slide = (int)(fontsize * 0.1);
            int lineMax = 12;
            int lineNum = text.Length / lineMax;

            uint width = (uint)(fontsize * Math.Min(lineMax, text.Length) + slide * 2);
            uint height = (uint)(fontsize * (lineNum + 1) + slide * 2);

            //var images = new MagickImageCollection();
            var frame = new MagickImage(
                   MagickColor.FromRgba(0, 0, 0, 0),
                   width,
                   height
               );
            frame.Format = MagickFormat.Png;
            for (int i = 0; i < text.Length; i++)
            {
                frame.Settings.Font = family.Name;
                frame.Settings.TextGravity = Gravity.West;
                frame.Settings.FillColor = MagickColor.FromRgb(255, 0, 0);
                frame.Settings.FontPointsize = fontsize;
                frame.Annotate(
                    text[i].ToString(),
                    new MagickGeometry(
                        (int)(i % lineMax * fontsize),
                        (int)(i / lineMax * fontsize),
                        100,
                        100),
                    Gravity.Northwest,
                    0
                    );
            }
            return frame;

        }



        /// <summary>
        /// 生成 CAPTCHA 图片
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static MagickImageCollection ImgGenerateCaptcha(string text)
        {
            uint fontsize = 33;
            int slide = (int)(fontsize * 0.3);
            int lineMax = 10;
            int lineNum = text.Length / lineMax;

            uint width = (uint)(fontsize * Math.Min(lineMax, text.Length) + slide * 2);
            uint height = (uint)(fontsize * (lineNum + 1) + slide * 2);

            var images = new MagickImageCollection();
            int frameCount = 10;
            uint frameDelay = 10;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                var frame = new MagickImage(
                    MagickColor.FromRgba(255, 255, 255, 0),
                    width,
                    height
                );
                frame.Format = MagickFormat.Gif;
                frame.AnimationDelay = frameDelay;
                for (int i = 0; i < text.Length; i++)
                {

                    frame.Settings.Font = Fonts[MyRandom.Next(Fonts)];
                    frame.Settings.TextGravity = Gravity.West;
                    frame.Settings.FillColor = FontColors[MyRandom.Next(FontColors)];
                    frame.Settings.FontPointsize = fontsize + MyRandom.Next(-5, 5);
                    frame.Annotate(
                        text[i].ToString(),
                        new MagickGeometry(
                            (int)(i % lineMax * fontsize + MyRandom.Next(5, 15) + slide),
                            (int)(i / lineMax * fontsize + MyRandom.Next(-5, 5) + slide),
                            100,
                            100),
                        Gravity.Northwest,
                        MyRandom.NextDouble
                        );
                }


                //// 添加干扰线
                //int lineCount = 1;
                //Drawables drawables = new Drawables();
                //for (int i = 0; i < lineCount; i++)
                //{
                //    int x1 = MyRandom.Next(frame.Width);
                //    int y1 = MyRandom.Next(frame.Height);
                //    int x2 = MyRandom.Next(frame.Width);
                //    int y2 = MyRandom.Next(frame.Height);
                //    drawables.StrokeColor(MagickColors.Gray)
                //                .StrokeWidth(2)
                //                .Line(x1, y1, x2, y2);
                //}
                //drawables.Draw(image);
                frame.AddNoise(NoiseType.Gaussian, 0.4);
                // 小幅度扭曲
                frame.Distort(DistortMethod.Shepards, new double[] { 0, 0, 5, 10 });

                images.Add(frame);
            }
            images.Optimize();
            images.OptimizeTransparency();
            return images;
            //using (MemoryStream ms = new MemoryStream())
            //{

            //    images.Write(ms, MagickFormat.Gif);
            //    byte[] imageBytes = ms.ToArray();
            //    string base64String = Convert.ToBase64String(imageBytes);
            //    return base64String;
            //}


            //using (MagickImage image = new MagickImage(MagickColors.White, width, height))
            //{
            //    //MagickReadSettings settings = new MagickReadSettings
            //    //{
            //    //    Font = "Arial",
            //    //    Width = width,
            //    //    Height = height,
            //    //    FillColor = MagickColors.Black,
            //    //    TextGravity = Gravity.Center,
            //    //    FontPointsize = fontsize
            //    //};



            //    //return (MagickImage)image.Clone(); 
            //}
        }


        /// <summary>
        /// 生成红包封面图片
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static MagickImage GetHongbao(string text)
        {
            uint maxfontsize = 50;
            uint minfontsize = 5;
            uint fontsize = 55;
            int slide = (int)(fontsize * 0.1);
            int lineMax = 7;
            int lineNum = text.Length / lineMax;
            int offsetx = 20;
            int offsety = 200;

            int offsetsingle = (int)fontsize + slide;
            uint width = (uint)(fontsize * Math.Min(lineMax, text.Length) + slide * 2);
            uint height = (uint)(fontsize * (lineNum + 1) + slide * 2);


            var image = new MagickImage(
                Config.Instance.FullPath("hongbao.png")
            //MagickColor.FromRgba(255, 255, 255, 0),
            //width,
            //height
            );

            //frame.AnimationDelay = frameDelay;
            image.Settings.Font = "华文细黑";
            image.Settings.TextGravity = Gravity.West;
            image.Settings.FillColor = MagickColor.FromRgb(0xD7, 0x99, 0x00);
            image.Settings.FontPointsize = fontsize;

            for (int i = 0; i < text.Length; i++)
            {
                image.Annotate(
                text[i].ToString(),
                new MagickGeometry(
                    (int)(image.Width / 2 - width + i % lineMax * offsetsingle + offsetx),
                   (int)(offsety - height / 2 + i / lineMax * offsetsingle),
                    width,
                    height),
                Gravity.North, 0
                );

            }

            image.Format = MagickFormat.Png;
            return image;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    images.Optimize();
            //    images.OptimizeTransparency();
            //    images.Write(ms, MagickFormat.Png);
            //    byte[] imageBytes = ms.ToArray();
            //    string base64String = Convert.ToBase64String(imageBytes);
            //    return images;
            //}

        }

        // 添加随机干扰线
        private static void AddRandomLines(MagickImage image, int lineCount)
        {
            Random random = new Random();


        }



        /// <summary>
        /// 从视频 URL 中截取第一帧的截图
        /// </summary>
        /// <param name="videoUrl">视频的 URL</param>
        /// <param name="outputImagePath">输出图片的路径（如 "C:\output.jpg"）</param>
        /// <returns>是否成功截取</returns>
        public static bool CaptureFirstFrame(string videoUrl, string outputImagePath)
        {
            try
            {
                // 构建 FFmpeg 命令
                string ffmpegCommand = $"-i \"{videoUrl}\" -vf \"select=eq(n\\,0)\" -q:v 2 -frames:v 1 \"{outputImagePath}\"";
                Logger.Log(ffmpegCommand);
                // 启动 FFmpeg 进程
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "D:\\ffmpeg\\bin\\ffmpeg.exe ", // FFmpeg 可执行文件路径
                    Arguments = ffmpegCommand,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    process.WaitForExit();

                    // 检查输出文件是否存在
                    if (File.Exists(outputImagePath))
                    {
                        Logger.Log("截图成功保存到: " + outputImagePath);
                        return true;
                    }
                    else
                    {
                        Logger.Log("截图失败，请检查 FFmpeg 是否安装或视频 URL 是否有效。");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("捕获帧时发生错误: " + ex.Message);
                return false;
            }
        }


        /// <summary>
        /// 图片卷动
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static MagickImageCollection ImgRoll(MagickImageCollection img, double degree = 0, int looptime = 1)
        {
            try
            {
                var images = new MagickImageCollection();
                uint frameDelay = 5;
                List<MagickImage> frames = new List<MagickImage>();

                img.Coalesce();

                int frameCount = img.Count * looptime;
                if (img.Count == 1) frameCount = 10;
                int dx = (int)(img.First().Width * Math.Cos(degree) / frameCount);
                int dy = (int)(-img.First().Height * Math.Sin(degree) / frameCount);
                if (dx != 0 && dy != 0) frameCount *= ((int)(Util.LCM((Util.LCM(int.Abs(dx), img.First().Width) / img.First().Width), (Util.LCM(int.Abs(dy), img.First().Height) / img.First().Height))));
                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    var imgg = img[frameIndex % img.Count];
                    var frame = new MagickImage(
                        MagickColor.FromRgba(255, 255, 255, 0),
                        img.First().Width,
                        img.First().Height
                    );
                    frame.Format = MagickFormat.Gif;
                    frame.AnimationDelay = imgg.AnimationDelay;
                    if (frame.AnimationDelay <= 0) frame.AnimationDelay = frameDelay;
                    int x = (int)(frameIndex * dx % frame.Width);
                    int y = (int)(frameIndex * dy % frame.Height);

                    frame.Composite(imgg, x, y, CompositeOperator.Over);
                    if (x > 0) frame.Composite(imgg, x - (int)frame.Width, y, CompositeOperator.Over);
                    if (x < 0) frame.Composite(imgg, x + (int)frame.Width, y, CompositeOperator.Over);
                    if (y > 0) frame.Composite(imgg, x, y - (int)frame.Height, CompositeOperator.Over);
                    if (y < 0) frame.Composite(imgg, x, y + (int)frame.Height, CompositeOperator.Over);
                    if (x > 0 && y > 0) frame.Composite(imgg, x - (int)frame.Width, y - (int)frame.Height, CompositeOperator.Over);
                    if (x > 0 && y < 0) frame.Composite(imgg, x - (int)frame.Width, y + (int)frame.Height, CompositeOperator.Over);
                    if (x < 0 && y > 0) frame.Composite(imgg, x + (int)frame.Width, y - (int)frame.Height, CompositeOperator.Over);
                    if (x < 0 && y < 0) frame.Composite(imgg, x + (int)frame.Width, y + (int)frame.Height, CompositeOperator.Over);
                    frame.GifDisposeMethod = GifDisposeMethod.Background;
                    images.Add(frame);
                }

                //images.Optimize();
                images.OptimizeTransparency();
                //images.Optimize();
                return images;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }



        /// <summary>
        /// 图片抖动
        /// </summary>
        /// <param name="img"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static MagickImageCollection ImgShake(MagickImageCollection img, double degree = 0.5, bool change_size = true)
        {
            try
            {
                uint width = (uint)(img.FirstOrDefault().Width * (1 + degree / 5.0));
                uint height = (uint)(img.FirstOrDefault().Height * (1 + degree / 5.0));
                if (!change_size)
                {
                    width = img.FirstOrDefault().Width;
                    height = img.FirstOrDefault().Height;
                }

                var images = new MagickImageCollection();
                int frameCount = img.Count;
                if (frameCount == 1)
                {
                    var imgg = img[0];
                    uint frameDelay = 5;
                    for (int frameIndex = 0; frameIndex < 10; frameIndex++)
                    {
                        var frame = new MagickImage(
                           MagickColor.FromRgba(255, 255, 255, 0),
                           width,
                           height
                        );
                        frame.Format = MagickFormat.Gif;
                        frame.AnimationDelay = frameDelay;
                        double dx = width * ((MyRandom.NextDouble - 0.5) * degree / 10);
                        double dy = height * ((MyRandom.NextDouble - 0.5) * degree / 10);
                        int x = (int)(width / 2 - imgg.Width / 2 + dx);
                        int y = (int)(height / 2 - imgg.Height / 2 + dy);
                        frame.Composite(imgg, x, y, CompositeOperator.Over);
                        frame.GifDisposeMethod = GifDisposeMethod.Background;
                        images.Add(frame);
                    }
                }
                else
                {
                    img.Coalesce();
                    for (int frameIndex = 0; frameIndex < img.Count; frameIndex++)
                    {
                        var imgg = img[frameIndex];
                        var frame = new MagickImage(
                           MagickColor.FromRgba(255, 255, 255, 0),
                           width,
                           height
                        );
                        frame.Format = MagickFormat.Gif;
                        frame.AnimationDelay = imgg.AnimationDelay;
                        double dx = width * ((MyRandom.NextDouble - 0.5) * degree / 10);
                        double dy = height * ((MyRandom.NextDouble - 0.5) * degree / 10);
                        int x = (int)(width / 2 - imgg.Width / 2 + dx);
                        int y = (int)(height / 2 - imgg.Height / 2 + dy);
                        frame.Composite(imgg, x, y, CompositeOperator.Over);
                        frame.GifDisposeMethod = GifDisposeMethod.Background;
                        images.Add(frame);
                    }
                }
                //images.Optimize();
                images.OptimizeTransparency();
                //images.Optimize();
                return images;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }




        /// <summary>
        /// 图片拼接，横向或者纵向
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <param name="horizional"></param>
        /// <returns></returns>
        public static MagickImageCollection combineImage(MagickImageCollection img1, MagickImageCollection img2, bool horizional = true)
        {
            MagickImageCollection img = new MagickImageCollection();

            var fullframes = Util.LCM(img1.Count, img2.Count);
            uint i1w = 0, i1h = 0, i2w = 0, i2h = 0;
            foreach (var f in img1)
            {
                i1w = uint.Max(i1w, f.Width);
                i1h = uint.Max(i1h, f.Height);
            }
            foreach (var f in img2)
            {
                i2w = uint.Max(i2w, f.Width);
                i2h = uint.Max(i2h, f.Height);
            }
            //uint i1w = img1.First().Width;
            //uint i1h = img1.First().Height;
            //uint i2w = img2.First().Width;
            //uint i2h = img2.First().Height;
            float r1 = 1;
            float r2 = 1;
            if (horizional) {
                if (i1h > i2h) r2 = ((float)i1h) / i2h;
                else if (i1h < i2h) r1 = ((float)i2h) / i1h;
            } else {
                if (i1w > i2w) r2 = ((float)i1w) / i2w;
                else if (i1w < i2w) r1 = ((float)i2w) / i1w;
            }
            i1w = (uint)(i1w * r1);
            i1h = (uint)(i1h * r1);
            i2w = (uint)(i2w * r2);
            i2h = (uint)(i2h * r2);
            img1.Coalesce();
            img2.Coalesce();
            for (int i = 0; i < fullframes; i++)
            {
                var img1f = img1[i % img1.Count];
                var img2f = img2[i % img2.Count];
                img1f.Resize(i1w, i1h);
                img2f.Resize(i2w, i2h);
                MagickImage frame = null;
                if (horizional)
                {
                    frame = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), i1w + i2w, i1h);
                    frame.Composite(img1f, 0, 0, CompositeOperator.Over);
                    frame.Composite(img2f, (int)i1w, 0, CompositeOperator.Over);
                }
                else
                {
                    frame = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), i1w, i1h + i2h);
                    frame.Composite(img1f, 0, 0, CompositeOperator.Over);
                    frame.Composite(img2f, 0, (int)i1h, CompositeOperator.Over);
                }
                frame.GifDisposeMethod = GifDisposeMethod.Background;
                frame.Format = fullframes == 1 ? MagickFormat.Png : MagickFormat.Gif;
                frame.AnimationDelay = 5;
                img.Add(frame);
            }
            img.OptimizeTransparency();

            return img;
        }











        /// <summary>
        /// 幻影坦克（黑白）
        /// </summary>
        /// <param name="front"></param>
        /// <param name="back"></param>
        /// <returns></returns>

        public static MagickImageCollection ImageBlend(MagickImage front, MagickImage back)
        {
            // 确保两张图尺寸相同
            if (front.Width != back.Width || front.Height != back.Height)
            {
                //Console.WriteLine("Warning: images have different sizes, resizing back to front size.");
                back.Resize(front.Width, front.Height);
                back.Extent(front.Width, front.Height, Gravity.Center, MagickColors.Black);
            }

            var res = new MagickImageCollection();

            var frame = new MagickImage(MagickColor.FromRgba(0,0,0,0), front.Width, front.Height);
            frame.Format = MagickFormat.Png;
            res.Add(frame);
            //Logger.Log($"{front.Width},{front.Height} ||  {back.Width},{back.Height}");
            // 获取像素集合
            var frontPixels = front.GetPixels();
            var backPixels = back.GetPixels();
            var resPixels = frame.GetPixels();

            // 亮度系数（与 Shader 完全一致）
            const double rWeight = 0.222;
            const double gWeight = 0.707;
            const double bWeight = 0.071;
            ushort[] pixel = new ushort[4];
            const double MAX_16 = 65535.0;
            // 显式双循环
            for (int y = 0; y < front.Height; y++)
            {
                for (int x = 0; x < front.Width; x++)
                {
                    // 读取前景像素 (R,G,B,A) → 仅用 RGB
                    var fp = frontPixels.GetPixel(x, y);
                    double fr = fp.GetChannel(0) / MAX_16;
                    double fg = fp.GetChannel(1) / MAX_16;
                    double fb = fp.GetChannel(2) / MAX_16;

                    // 读取背景像素
                    var bp = backPixels.GetPixel(x, y);
                    double br = bp.GetChannel(0) / MAX_16;
                    double bg = bp.GetChannel(1) / MAX_16;
                    double bb = bp.GetChannel(2) / MAX_16;

                    // ---- Shader: color1.rgb = dot(rgb, (.222,.707,.071)) ----
                    double gray1 = fr * rWeight + fg * gWeight + fb * bWeight;

                    // ---- Shader: color2.rgb = dot(...) * 0.3 ----
                    double gray2 = (br * rWeight + bg * gWeight + bb * bWeight) * 0.3;

                    // ---- Shader: a = 1 - color1.r + color2.r ----
                    double a = 1.0 - gray1 + gray2;

                    // 防止除以 0（理论上 a >= 0.0）
                    double r = a > 1e-8 ? gray2 / a : 0.0;

                    // 限制到 [0,1]
                    r = Math.Clamp(r, 0.0, 1.0);
                    a = Math.Clamp(a, 0.0, 1.0);

                    // 转为 0~255 并写入结果
                    ushort outR = (ushort)Math.Round(r * MAX_16);
                    ushort outA = (ushort)Math.Round(a * MAX_16);

                    // 写入单像素：R=G=B=r, A=a
                    pixel[0] = (ushort)Math.Round(r * MAX_16);  // R
                    pixel[1] = pixel[0];                     // G
                    pixel[2] = pixel[0];                     // B
                    pixel[3] = (ushort)Math.Round(a * MAX_16);  // A
                    
                    resPixels.SetArea(x, y, 1, 1, pixel);
                }
            }
            return res;
        }


        /// <summary>
        /// 幻影坦克（彩色）
        /// </summary>
        /// <param name="wimg"></param>
        /// <param name="bimg"></param>
        /// <param name="wlight"></param>
        /// <param name="blight"></param>
        /// <param name="wcolor"></param>
        /// <param name="bcolor"></param>
        /// <param name="chess"></param>
        public static MagickImageCollection ImageBlendColorful(
            MagickImage wimg, MagickImage bimg,
            double wlight = 1.0, double blight = 0.35,
            double wcolor = 0.5, double bcolor = 0.7,
            bool chess = false)
        {

            // 1. 亮度增强
            wimg.BrightnessContrast(new Percentage((wlight - 1) * 100), new Percentage(0));
            bimg.BrightnessContrast(new Percentage((blight - 1) * 100), new Percentage(0));

            // 2. 转为 RGB 并对齐尺寸
            wimg.ColorType = ColorType.TrueColor;
            bimg.ColorType = ColorType.TrueColor;

            var width = wimg.Width;
            var height = wimg.Height;
            if (bimg.Width != width || bimg.Height != height)
            {
                bimg.Resize(width, height); 
                bimg.Extent(wimg.Width, wimg.Height, Gravity.Center, MagickColors.Black);
            }
                

            // 3. 创建 16-bit 工作图像（高精度计算）
            wimg.Depth = 16;
            bimg.Depth = 16;

            var result = new MagickImageCollection();
            var frame = new MagickImage(new MagickColor(0, 0, 0, 0), width, height);
            frame.Format = MagickFormat.Png;
            result.Add(frame);
            var wPixels = wimg.GetPixels();
            var bPixels = bimg.GetPixels();
            var outPixels = frame.GetPixels();

            const double MAX_16 = 65535.0;
            ushort[] pixel = new ushort[4];

            // 灰度权重（Python: 0.334, 0.333, 0.333 ≈ 1/3）
            const double grayR = 0.334, grayG = 0.333, grayB = 0.333;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 读取像素 (RGBA16)
                    ushort[] wp = wPixels.GetArea(x, y, 1, 1);
                    ushort[] bp = bPixels.GetArea(x, y, 1, 1);

                    double wr = wp[0] / MAX_16, wg = wp[1] / MAX_16, wb = wp[2] / MAX_16;
                    double br = bp[0] / MAX_16, bg = bp[1] / MAX_16, bb = bp[2] / MAX_16;

                    // --- 棋盘格模式 ---
                    if (chess)
                    {
                        if ((x % 2 == 0) && (y % 2 == 0))
                        {
                            wr = wg = wb = 1.0;
                        }
                        if ((x % 2 == 1) && (y % 2 == 1))
                        {
                            br = bg = bb = 0.0;
                        }
                    }

                    // --- 前景：颜色 + 灰度混合 ---
                    double wgray = wr * grayR + wg * grayG + wb * grayB;
                    double wcolorKeep = 1.0 - wcolor;
                    wr = wr * wcolor + wgray * wcolorKeep;
                    wg = wg * wcolor + wgray * wcolorKeep;
                    wb = wb * wcolor + wgray * wcolorKeep;

                    // --- 背景：颜色 + 灰度混合 ---
                    double bgray = br * grayR + bg * grayG + bb * grayB;
                    double bcolorKeep = 1.0 - bcolor;
                    br = br * bcolor + bgray * bcolorKeep;
                    bg = bg * bcolor + bgray * bcolorKeep;
                    bb = bb * bcolor + bgray * bcolorKeep;

                    // --- d = 1 - w + b ---
                    double dr = 1.0 - wr + br;
                    double dg = 1.0 - wg + bg;
                    double db = 1.0 - wb + bb;

                    // --- d 转灰度 (0.222, 0.707, 0.071) ---
                    double dgray = dr * 0.222 + dg * 0.707 + db * 0.071;

                    // --- p = b / d * 255 ---
                    double pr = dgray > 1e-8 ? br / dgray * 255.0 : 255.0;
                    double pg = dgray > 1e-8 ? bg / dgray * 255.0 : 255.0;
                    double pb = dgray > 1e-8 ? bb / dgray * 255.0 : 255.0;

                    // --- a = dgray * 255 ---
                    double pa = dgray * 255.0;

                    // --- 限制到 0~255 ---
                    pr = Math.Clamp(pr, 0.0, 255.0);
                    pg = Math.Clamp(pg, 0.0, 255.0);
                    pb = Math.Clamp(pb, 0.0, 255.0);
                    pa = Math.Clamp(pa, 0.0, 255.0);

                    // --- 写入 RGBA8 结果 ---
                    pixel[0] = (ushort)Math.Round(pr * 257.0); // 255 -> 65535
                    pixel[1] = (ushort)Math.Round(pg * 257.0);
                    pixel[2] = (ushort)Math.Round(pb * 257.0);
                    pixel[3] = (ushort)Math.Round(pa * 257.0);

                    outPixels.SetArea(x, y, 1, 1, pixel);
                }
            }
            return result;
            
        }




    } 
}
