using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugua
{

    public class ImageGreenSimulator
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

        // Resize the image to a new width while maintaining aspect ratio
        private static Bitmap ResizeImage(Bitmap image, int newWidth)
        {
            int newHeight = (int)((float)image.Height / image.Width * newWidth);
            Bitmap resizedImage = new Bitmap(image, new Size(newWidth, newHeight));
            return resizedImage;
        }




        /// <summary>
        /// 处理图片做旧，返回base64编码的图像数据
        /// </summary>
        /// <param name="images"></param>
        /// <param name="iterations"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static string ProcessImage(MagickImageCollection images, int iterations = 16, int quality = 75, bool dealGreen = true)
        {
            if (images == null) return null;

            foreach (var image in images)
            {
                //Logger.Log("IMG " + image.Height + "," + image.Width);
                uint oriW = image.Width;
                uint oriH = image.Height;
                uint smallW = (uint)(image.Width * 0.69);
                uint smallH = (uint)((float)image.Height / image.Width * smallW);
                image.Resize(smallW, smallH);
                image.Resize(oriW,oriH);

                for (int iter = 0; iter < iterations; iter++)
                {
                    //Bitmap resultImage = new Bitmap((int)(image.Width), (int)(image.Height));
                    if (dealGreen)
                    {
                        var pixels = image.GetPixels();
                        int no = 0;
                        foreach(var pixel in pixels)
                        {
                            
                            var pixelColor = pixel.ToColor();
                            var yuv = Rgb2Yuv(pixelColor.R, pixelColor.G, pixelColor.B);
                            // Apply green shift (UV shift)
                            yuv[1] = yuv[1] - 255; // Slight shift to simulate the green effect
                             // Convert back to RGB and set the pixel
                            int[] rgb = Yuv2Rgb(yuv[0], yuv[1], yuv[2]);
   
                            pixel.SetChannel(0, (ushort)rgb[0]);//.SetPixel(x, y, Color.FromArgb(rgb[0], rgb[1], rgb[2]));
                            pixel.SetChannel(1, (ushort)rgb[1]);
                            pixel.SetChannel(2, (ushort)rgb[2]);
                            // pixels.SetPixel(pixel);
                        }
                    }
                }
            }
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (var image in images)
                {
                    //image.Format = MagickFormat.Jpeg;
                    image.Quality = (uint)quality;
                    //image.Write(ms);
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




    public class PicDealer
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
