using ImageMagick;
using ImageMagick.Colors;
using ImageMagick.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZhipuApi.Modules;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Image = System.Drawing.Image;

namespace Kugua
{
    /// <summary>
    /// 图像处理相关
    /// </summary>
    public class ImageUtil
    {
        // Clamp values to be within 0-65535
        private static int Clamp(int x)
        {
            return x < 0 ? 0 : (x > 65535 ? 65535 : x);
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
                int pos =  (int)(degree);
                
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
                while(images.Count>1)images.RemoveAt(1);
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
            byte b = (byte)(color);
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
            byte r = (byte)((color >> 11) & 0x1F); // 提取红色分量（5 位）
            byte g = (byte)((color >> 5) & 0x3F);  // 提取绿色分量（6 位）
            byte b = (byte)(color & 0x1F);         // 提取蓝色分量（5 位）

            // 将 5/6 位分量扩展到 8 位
            r = (byte)((r << 3) | (r >> 2)); // 5 位 -> 8 位
            g = (byte)((g << 2) | (g >> 4)); // 6 位 -> 8 位
            b = (byte)((b << 3) | (b >> 2)); // 5 位 -> 8 位

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
            
            foreach(var colorCode in colorCodes)
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
        public static MagickImage GetColorSample(string colorCode, int size=80)
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
            for(int i =0;i<iterations;i++)
            {
                    
                using(MemoryStream mss=new MemoryStream())
                {
                    foreach (var image in images)
                    {
                        //image.Format = MagickFormat.Jpeg;
                        image.Quality = (uint)(quality*90 * MyRandom.NextDouble());
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
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

            // Save the image as JPEG
            image.Save(outputImagePath, jpegCodec, encoderParams);
            Logger.Log($"Image saved to: {outputImagePath}", LogType.Debug);
        }

        // Helper function to get encoder info for the desired format (JPEG)



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
                    var resb = ImgRemoveBackground(images[i].ToByteArray());
                    var img = new MagickImage(resb);
                    img.GifDisposeMethod = GifDisposeMethod.Background;
                    img.AnimationDelay = images[i].AnimationDelay;
                    // img.Transparent(new MagickColor(0, 0, 0,0));
                    //if(i!=0) img.Alpha(AlphaOption.Copy);
                    images.Add(img);
                }
                for (int i = 0; i < num; i++)
                {
                    images.RemoveAt(0);
                }
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
            
            uint width = (uint)(fontsize * Math.Min(lineMax,text.Length) + slide * 2);
            uint height = (uint)(fontsize * (lineNum + 1) + slide * 2);

            var images = new MagickImageCollection();
            int frameCount = 10;
            uint frameDelay = 10;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                var frame = new MagickImage(
                    MagickColor.FromRgba(255,255,255,0),
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
                            (int)((i% lineMax) * fontsize + MyRandom.Next(5,15) + slide),
                            (int)((i / lineMax) * fontsize + MyRandom.Next(-5,5) + slide),
                            100, 
                            100), 
                        Gravity.Northwest, 
                        MyRandom.NextDouble()
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
                        (int)(image.Width / 2 - width + (int)((i% lineMax)) * offsetsingle + offsetx),
                       (int)(offsety - height / 2 + (int)((i/ lineMax)) * offsetsingle),
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

    }


    
    /// <summary>
    /// 计算图片相似度。基于区域哈希算法
    /// </summary>
    class ImageSimilar
    {
        Image SourceImg;

        //public SimilarPhoto(string filePath)
        //{
        //    SourceImg = Image.FromFile(filePath);
        //}

        public ImageSimilar(string base64)
        {
            if (!string.IsNullOrWhiteSpace(base64))
            {
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    SourceImg = Image.FromStream(ms);
                }
            }
        }

        public ImageSimilar(Stream stream)
        {
            SourceImg = Image.FromStream(stream);
        }

        /// <summary>
        /// 计算图片的像素哈希
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static string GetHashFromBase64(string base64)
        {
            var img = new ImageSimilar(base64);
            return img.GetHash(base64);
        }

        public string GetHash(string base64)
        {
            var resized = ReduceSize();
            var blurred = ApplyGaussianBlur(resized);
            var grayValues = ReduceColor(blurred);
            var median = CalcMedian(grayValues);
            return ComputeBits(grayValues, median);
        }

        /// <summary>
        /// Step 1 : Reduce size to 8*8
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Image ReduceSize(int width = 8, int height = 8)
        {
            var destImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(SourceImg, 0, 0, width, height);
            }
            return destImage;
        }

        /// <summary>
        /// （高斯模糊）​减少噪声干扰，提升特征稳定性。
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap ApplyGaussianBlur(Image image)
        {
            var blurred = new Bitmap(image.Width, image.Height);
            using (var graphics = Graphics.FromImage(blurred))
            using (var blurEffect = new System.Drawing.Imaging.ImageAttributes())
            {
                // 使用高斯模糊（需System.Drawing.Common）
                blurEffect.SetColorMatrix(new ColorMatrix(new float[][]
                {
                    new float[] { 0.1f, 0.1f, 0.1f, 0, 0 },
                    new float[] { 0.1f, 0.2f, 0.1f, 0, 0 },
                    new float[] { 0.1f, 0.1f, 0.1f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                }));
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, image.Width, image.Height, GraphicsUnit.Pixel, blurEffect);
            }
            return blurred;
        }

        /// <summary>
        /// Step 2 : Reduce Color
        /// </summary>
        /// <param name="oriImage"></param>
        /// <returns></returns>
        private Byte[] ReduceColor(Image image)
        {

            Bitmap bitMap = new Bitmap(image);
            Byte[] grayValues = new Byte[image.Width * image.Height];

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    Color color = bitMap.GetPixel(x, y);
                    // 使用浮点系数计算
                    //byte grayValue = (byte)((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                    byte grayValue = (byte)(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                    grayValues[x * image.Width + y] = grayValue;
                }
            return grayValues;

        }


        /// <summary>
        /// 计算归一化汉明距离（相似度的百分比，范围在0~1）
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double CalcSimilarDegree(string a, string b)
        {
            if (a.Length != b.Length) throw new ArgumentException();
            int differences = a.Zip(b, (c1, c2) => c1 != c2 ? 1 : 0).Sum();
            return 1.0 - (double)differences / a.Length;
        }

        /// <summary>
        /// 计算中位数.平均值对光照敏感，改用中位数提升鲁棒性
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private Byte CalcMedian(byte[] values)
        {
            var sorted = values.OrderBy(v => v).ToArray();
            int mid = sorted.Length / 2;
            return (sorted.Length % 2 != 0) ? sorted[mid] : (byte)((sorted[mid - 1] + sorted[mid]) / 2);
        }

        private string ComputeBits(byte[] values, byte threshold, int width = 8)
        {
            var hash = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                // 差异哈希（比较右侧像素）
                if (i % width != width - 1)
                    hash.Append(values[i] > values[i + 1] ? "1" : "0");
            }
            return hash.ToString();
        }



    }


}
