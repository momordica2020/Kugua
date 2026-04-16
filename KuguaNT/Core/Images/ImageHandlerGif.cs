using ImageMagick;
using Kugua.Core.Algorithms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Kugua.Core.Images
{
    /// <summary>
    /// 图像处理相关
    /// </summary>
    public partial class ImageHandler
    {
        




        /// <summary>
        /// 将gif每一帧提取平铺道图上
        /// </summary>
        /// <param name="gif"></param>
        /// <param name="totalCols"></param>
        /// <returns></returns>
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
            for (int row = 0; row < totalRows; row++) 
            {
                for (int col = 0; col < totalCols; col++)
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


        /// <summary>
        /// 静态或者gif的范围裁切，传入上下左右的裁切像素
        /// </summary>
        /// <param name="images"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
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
                images.First().Crop(new MagickGeometry(left, top, newWidth, newHeight));
                images.First().ResetPage();
                return images;
            }

            MagickImageCollection newImages = new MagickImageCollection();
            images.Coalesce();
            foreach (var frame in images)
            {
                var newFrame = frame.Clone();
                newFrame.Crop(new MagickGeometry(left, top, newWidth, newHeight));
                newFrame.ResetPage();
                newFrame.Format = MagickFormat.Gif;
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
                string ffmpegCommand = $" -i \"{videoUrl}\" -vf \"select=eq(n\\,0)\" -q:v 2 -frames:v 1 \"{outputImagePath}\"";
                Logger.Log(ffmpegCommand);
                // 启动 FFmpeg 进程
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",///"D:\\ffmpeg\\bin\\ffmpeg.exe", // FFmpeg 可执行文件路径
                    Arguments = ffmpegCommand,
                    CreateNoWindow = true
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //UseShellExecute = false,
                    //CreateNoWindow = true
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
                if (dx != 0 && dy != 0) frameCount *= ((int)(MathUtil.LCM((MathUtil.LCM(int.Abs(dx), img.First().Width) / img.First().Width), (MathUtil.LCM(int.Abs(dy), img.First().Height) / img.First().Height))));
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
        /// 静态和动态的图片拼接，横向或者纵向
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <param name="horizional"></param>
        /// <returns></returns>
        public static MagickImageCollection combineImage(MagickImageCollection img1, MagickImageCollection img2, bool horizional = true)
        {
            MagickImageCollection img = new MagickImageCollection();

            var fullframes = MathUtil.LCM(img1.Count, img2.Count);
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
        /// 将一张图片变成 10 帧“弹性压扁 → 回弹 → 轻微超伸 → 恢复”的透明 GIF 动画
        /// </summary>
        /// <param name="source">原始图片（支持透明）</param>
        /// <returns>MagickImageCollection（已设置好 Delay 和 Loop）</returns>
        public static MagickImageCollection ToElasticBounceGif(MagickImageCollection source)
        {
            var originalWidth = source.First().Width;
            var originalHeight = source.First().Height;

            var frames = new MagickImageCollection();
            var frameAnimNum = 10;
            var frameNum = MathUtil.LCM(source.Count, frameAnimNum);
            
            // 10 帧关键参数
            var scaleX = new[] { 1.00, 1.08, 1.18, 1.25, 1.20, 1.10, 0.95, 0.98, 1.00, 1.00 };
            var scaleY = new[] { 1.00, 0.88, 0.70, 0.58, 0.70, 0.92, 1.15, 1.08, 1.02, 1.00 };
            var delay = new[] { 10, 8, 7, 8, 7, 6, 8, 8, 10, 10 }; // 单位：1/100 秒

            for (int i = 0; i < frameNum; i++)
            {
                // 克隆一份原始图（保持透明通道）
                using var frame = source[i % source.Count].Clone();

                uint newW = (uint)Math.Round(originalWidth * scaleX[i % frameAnimNum]);
                uint newH = (uint)Math.Round(originalHeight * scaleY[i % frameAnimNum]);

                frame.LiquidRescale(newW, newH, 1.0, 0.0); // 最后一个参数是刚性保护（0=完全弹性）

                // 放进和原图一样大的透明画布中央（保证所有帧尺寸一致）
                var canvas = new MagickImage(MagickColors.Transparent, originalWidth, originalHeight);
                int offsetX = (int)(originalWidth - frame.Width) / 2;
                int offsetY = (int)(originalHeight - frame.Height) / 2;
                canvas.Composite(frame, offsetX, offsetY, CompositeOperator.Over);

                canvas.AnimationDelay = (uint)delay[i % frameAnimNum];
                canvas.GifDisposeMethod = GifDisposeMethod.Background;
                canvas.Format = MagickFormat.Gif;

                frames.Add(canvas);
            }
            frames.OptimizeTransparency();

            return frames;
        }




    } 
}
