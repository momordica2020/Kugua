using ImageMagick;
using ImageMagick.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZhipuApi.Modules;
using static System.Runtime.InteropServices.JavaScript.JSType;

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


        public static string ImgRotate(MagickImageCollection images, double rotate)
        {
            if (images == null) return null;

            foreach (var image in images)
            {
                image.Rotate(rotate);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                images.Write(ms);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static string ImgMirror(MagickImageCollection images, double degree)
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

            using (MemoryStream ms = new MemoryStream())
            {
                var settings = new QuantizeSettings();
                settings.Colors = 256;

                outputGif.Quantize(settings);
                outputGif.OptimizeTransparency();
                outputGif.Write(ms);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }


        public static string GifSpeed(MagickImageCollection images, double speed)
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
          
            using (MemoryStream ms = new MemoryStream())
            {
                var settings = new QuantizeSettings();
                settings.Colors = 256;
                
                images.Quantize(settings);
                images.OptimizeTransparency();
                images.Write(ms);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }



        /// <summary>
        /// 处理图片做旧，返回base64编码的图像数据
        /// </summary>
        /// <param name="images"></param>
        /// <param name="iterations"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static string ImageGreen(MagickImageCollection images, int iterations = 16, double quality = 0.75)
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
            using (MemoryStream ms = new MemoryStream())
            {
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
                            image.Quality = (uint)(quality*90);
                            //image.Write(ms);
                        }
                        images.Write(mss);
                        byte[] imageBytess = mss.ToArray();
                        images = new MagickImageCollection(imageBytess);
                    } 
                }
                images.Write(ms);
                byte[] imageBytes = ms.ToArray();
                //Logger.Log("bytes => " + imageBytes.Length);
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
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



        public static byte[] RemoveBackground(byte[] imageBytes)
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

        public static string RemoveBackground(MagickImageCollection images)
        {
            try
            {
                //MagickImageCollection m2 = new MagickImageCollection();
                //while (m2.Count > 0) m2.RemoveAt(0);
                int num = images.Count;
                images.Coalesce();

                for (int i = 0; i < num; i++)
                {
                    var resb = RemoveBackground(images[i].ToByteArray());
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

                using (MemoryStream ms = new MemoryStream())
                {
                    var settings = new QuantizeSettings();
                    settings.Colors = 256;
                     
                    images.Quantize(settings);
                    images.OptimizeTransparency();
                    images.Write(ms,MagickFormat.Gif);
                    byte[] imageBytes = ms.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
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
            MagickColor.FromRgb(255,255,0),// red
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
        public static string GenerateCaptchaImage(string text)
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
            using (MemoryStream ms = new MemoryStream())
            {
                images.Optimize();
                images.OptimizeTransparency();
                images.Write(ms, MagickFormat.Gif);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }


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

        // 添加随机干扰线
        private static void AddRandomLines(MagickImage image, int lineCount)
        {
            Random random = new Random();
            
            
        }

    }




    public class ImageUtil2
    {
        //public static void setGray(Bitmap bm)
        //{
        //    //   Bitmap bm2 = (Bitmap)bm.Clone();
        //    Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
        //    BitmapData bmpdata = bm.LockBits(rect, ImageLockMode.ReadWrite, bm.PixelFormat);

        //    int row = bmpdata.Height;
        //    int col = bmpdata.Width;
        //    //byte[][] res = new byte[row][];

        //    try
        //    {
        //        unsafe
        //        {
        //            byte* ptr = (byte*)(bmpdata.Scan0);
        //            for (int i = 0; i < row; i++)
        //            {
        //                //res[i] = new byte[col];
        //                for (int j = 0; j < col; j++)
        //                {
        //                    ptr = (byte*)(bmpdata.Scan0) + bmpdata.Stride * i + 3 * j;
        //                    byte ptrgray = (byte)(0.299 * ptr[2] + 0.587 * ptr[1] + 0.114 * ptr[0]);
        //                    ptr[0] = ptrgray;
        //                    ptr[1] = ptrgray;
        //                    ptr[2] = ptrgray;
        //                    //res[i][j] = (byte)(0.299 * ptr[2] + 0.587 * ptr[1] + 0.114 * ptr[0]);
        //                }
        //            }
        //        }

        //    }
        //    catch
        //    {

        //    }
        //    bm.UnlockBits(bmpdata);
        //    //return res;
        //}

    }
}
