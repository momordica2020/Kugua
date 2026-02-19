namespace Kugua.Integrations
{

    /// <summary>
    /// 体脂率计算
    /// </summary>
    public class BodyFatCalculator
    {
        
        /// <summary>
         /// 方法1：Deurenberg BMI公式（最常用、最简单，只需身高、体重、年龄、性别）
         /// 公式来源：成人常用版本（西方/中文社区广泛使用）
         /// 男性：体脂率 = 1.20 × BMI + 0.23 × 年龄 - 16.2
         /// 女性：体脂率 = 1.20 × BMI + 0.23 × 年龄 - 5.4
         /// </summary>
        public static double CalculateByBMI(double heightCm, double weightKg, int age, bool isMale = true)
        {
            if (heightCm <= 0 || weightKg <= 0) throw new ArgumentException("身高或体重无效");

            double heightM = heightCm / 100.0;
            double bmi = weightKg / (heightM * heightM);

            double bodyFat;
            if (isMale)
            {
                bodyFat = 1.20 * bmi + 0.23 * age - 16.2;
            }
            else
            {
                bodyFat = 1.20 * bmi + 0.23 * age - 5.4;
            }

            return Math.Round(bodyFat, 2); // 保留两位小数
        }

        /// <summary>
        /// 方法2：美国海军法（U.S. Navy Body Fat Formula） - 精度较高
        /// 男性：体脂率 = 86.010 × log10(腰围 - 颈围) - 70.041 × log10(身高) + 36.76
        /// 女性：体脂率 = 163.205 × log10(腰围 + 臀围 - 颈围) - 97.684 × log10(身高) - 78.387
        /// 所有长度单位：cm（公式内部会转为英寸计算，但这里直接用cm版等效公式）
        /// </summary>
        public static double CalculateByNavyMethod(
            double heightCm,
            double waistCm,
            double neckCm,
            double? hipCm = null,  // 女性必填，男性可传null
            bool isMale = true)
        {
            if (heightCm <= 0 || waistCm <= 0 || neckCm <= 0)
                throw new ArgumentException("测量值无效");

            double log10(double x) => Math.Log10(x);

            double bodyFat;

            if (isMale)
            {
                // 男性公式（cm版等效）
                double diff = waistCm - neckCm;
                if (diff <= 0) throw new ArgumentException("腰围必须大于颈围");

                bodyFat = 86.010 * log10(diff) - 70.041 * log10(heightCm) + 36.76;
            }
            else
            {
                if (!hipCm.HasValue || hipCm.Value <= 0)
                    throw new ArgumentException("女性需要提供臀围");

                double sum = waistCm + hipCm.Value - neckCm;
                if (sum <= 0) throw new ArgumentException("腰围+臀围必须大于颈围");

                bodyFat = 163.205 * log10(sum) - 97.684 * log10(heightCm) - 78.387;
            }

            return Math.Round(bodyFat, 2);
        }

        /// <summary>
        /// 方法3：常见中文圈腰围简化公式（适合快速估算）
        /// 男性：体脂率 = (腰围 × 0.74 - 体重 × 0.082 - 44.74) / 体重 × 100
        /// 女性：体脂率 = (腰围 × 0.74 - 体重 × 0.082 - 34.89) / 体重 × 100
        /// </summary>
        public static double CalculateByWaistSimple(double waistCm, double weightKg, bool isMale)
        {
            if (waistCm <= 0 || weightKg <= 0) throw new ArgumentException("测量值无效");

            double constant = isMale ? 44.74 : 34.89;

            double fatMass = waistCm * 0.74 - weightKg * 0.082 - constant;
            double bodyFat = (fatMass / weightKg) * 100;

            return Math.Round(bodyFat, 2);
        }


    }
}
