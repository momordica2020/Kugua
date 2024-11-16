using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugua
{

    public class JPEGreenSimulator
    {
        // Clamp values to be within 0-255
        private static int Clamp(int x)
        {
            return x < 0 ? 0 : (x > 255 ? 255 : x);
        }

        // Convert RGB to YUV
        private static int[] Rgb2Yuv(int r, int g, int b)
        {
            int y = (int)(r * 0.299 + g * 0.587 + b * 0.114);
            int u = (int)(r * -0.168736 + g * -0.331264 + b * 0.500 + 128);
            int v = (int)(r * 0.500 + g * -0.418688 + b * -0.081312 + 128);
            return new int[] { Clamp(y), Clamp(u), Clamp(v) };
        }

        // Convert YUV back to RGB
        private static int[] Yuv2Rgb(int y, int u, int v)
        {
            int r = (int)(y + 1.4075 * (v - 128));
            int g = (int)(y - 0.3455 * (u - 128) - 0.7169 * (v - 128));
            int b = (int)(y + 1.7790 * (u - 128));
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
        /// <param name="inputImagePath"></param>
        /// <param name="iterations"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static string ProcessImage(Bitmap image, int iterations = 16, int quality = 75)
        {
            if (image == null) return null;
            // Load the initial image

            // Check if the image dimensions exceed 1000x1000 and resize if necessary
            //if (image.Width > 200 || image.Height > 200)
            {
                
                image = ResizeImage(image, image.Width / 3);
                image = ResizeImage(image, image.Width * 3); // Resize back to original width
            }

            for (int iter = 0; iter < iterations; iter++)
            {
                Bitmap resultImage = new Bitmap(image.Width, image.Height);

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        int[] yuv = Rgb2Yuv(pixelColor.R, pixelColor.G, pixelColor.B);

                        // Apply green shift (UV shift)
                        yuv[1] = yuv[1] - 1; // Slight shift to simulate the green effect

                        // Convert back to RGB and set the pixel
                        int[] rgb = Yuv2Rgb(yuv[0], yuv[1], yuv[2]);
                        resultImage.SetPixel(x, y, Color.FromArgb(rgb[0], rgb[1], rgb[2]));
                    }
                }

                // After each iteration, update the image to the result of this iteration
                image = new Bitmap(resultImage);  // Ensure the current image is updated for the next iteration

                // Optionally, we can apply compression or quality adjustments
                if (iter == iterations - 1)
                {
                    return GetBase64Image(resultImage, quality);  // Save the final result
                }
            }

            return null; 
        }
        // Convert processed image to Base64 string
        private static string GetBase64Image(Bitmap image, int quality)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Set the encoder parameters for JPEG quality
                ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                // Save the image to the memory stream in JPEG format
                image.Save(ms, jpegCodec, encoderParams);

                // Convert the memory stream to a Base64 string
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
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
            Logger.Instance.Log($"Image saved to: {outputImagePath}", LogType.Debug);
        }

        // Helper function to get encoder info for the desired format (JPEG)
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }
            return null;
        }
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
