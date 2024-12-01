using ImageMagick;
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
                foreach (var image in images)
                {
                    image.AnimationDelay = (uint)(image.AnimationDelay / Math.Abs(speed));
                    if (image.AnimationDelay <2 )
                    {
                        image.AnimationDelay = 2;
                    }

                    image.ColorFuzz = new Percentage(5);
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
                    uint smallW = (uint)(oriW * Math.Min(1, quality * 2));
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
