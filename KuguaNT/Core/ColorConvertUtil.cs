namespace Kugua.Core
{
    /// <summary>
    /// 颜色参数值CIE转换类
    /// </summary>
    public class ColorConvertUtil
    {
        /// <summary>
        /// sRGB转换矩阵（D65白点）
        /// </summary>
        private static readonly double[,] XYZ_to_RGB = {
            { 3.2404542, -1.5371385, -0.4985314 },
            { -0.9692660, 1.8760108, 0.0415560 },
            { 0.0556434, -0.2040259, 1.0572252 }
        };



        /// <summary>
        /// 将CIE XYZ值（基于D65照明体和CIE 1964 10°观察者）转化为 sRGB。
        /// 超出可视范围的参数将被截断
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        public static (int R, int G, int B) XYZtoRGB(double X, double Y, double Z)
        {
            // 1. XYZ到线性RGB转换
            double rLinear = XYZ_to_RGB[0, 0] * X + XYZ_to_RGB[0, 1] * Y + XYZ_to_RGB[0, 2] * Z;
            double gLinear = XYZ_to_RGB[1, 0] * X + XYZ_to_RGB[1, 1] * Y + XYZ_to_RGB[1, 2] * Z;
            double bLinear = XYZ_to_RGB[2, 0] * X + XYZ_to_RGB[2, 1] * Y + XYZ_to_RGB[2, 2] * Z;

            // 2. 确保线性RGB值在合理范围内（裁剪到[0, 1]）
            rLinear = Math.Clamp(rLinear, 0.0, 1.0);
            gLinear = Math.Clamp(gLinear, 0.0, 1.0);
            bLinear = Math.Clamp(bLinear, 0.0, 1.0);

            // 3. 应用sRGB伽马校正
            double r = ApplySrgbGamma(rLinear);
            double g = ApplySrgbGamma(gLinear);
            double b = ApplySrgbGamma(bLinear);

            // 4. 转换为8位RGB值（0-255）
            int R = (int)Math.Round(r * 255.0);
            int G = (int)Math.Round(g * 255.0);
            int B = (int)Math.Round(b * 255.0);

            return (R, G, B);
        }

        /// <summary>
        /// sRGB伽马校正（近似）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static double ApplySrgbGamma(double value)
        {
            return Math.Pow(value, 1.0 / 2.2); // sRGB近似伽马校正
        }

        /// <summary>
        /// 将CIE xyY色度坐标（x, y, Y）转化为 CIE XYZ值（基于D65照明体和CIE 1964 10°观察者）。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static (double X, double Y, double Z) xyYtoXYZ(double x, double y, double Y)
        {
            if (y == 0) return (0, 0, 0); // 避免除以零
            double X = x * Y / y;
            double Z = (1 - x - y) * Y / y;
            return (X, Y, Z);
        }

        /// <summary>
        /// 将CIE xyY色度坐标（x, y, Y）转化为RGB
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static (int R, int G, int B) xyYtoRGB(double x, double y, double Y)
        {
            var (X, Y_out, Z) = xyYtoXYZ(x, y, Y);
            return XYZtoRGB(X, Y_out, Z);
        }

        // 测试代码
        public static void Main()
        {
            // 示例：D65白点（x=0.3127, y=0.3290, Y=1.0）
            double x = 0.3127;
            double y = 0.3290;
            double Y = 1.0;

            var (R, G, B) = xyYtoRGB(x, y, Y);
            Console.WriteLine($"RGB: ({R}, {G}, {B})");

            // 示例：直接输入XYZ值
            double X = 0.95047; // D65白点的XYZ值
            double Y_out = 1.0;
            double Z = 1.08883;
            (R, G, B) = XYZtoRGB(X, Y_out, Z);
            Console.WriteLine($"RGB from XYZ: ({R}, {G}, {B})");
        }
    }
}
