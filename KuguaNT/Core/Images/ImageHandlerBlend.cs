using ImageMagick;

namespace Kugua.Core.Images
{
    // 该部分是幻影坦克算法
    public partial class ImageHandler
    {

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

            var frame = new MagickImage(MagickColor.FromRgba(0, 0, 0, 0), front.Width, front.Height);
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
