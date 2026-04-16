namespace Kugua.Core.Chinese
{
    /// <summary>
    /// 国学相关
    /// </summary>
    internal static class ChineseCulture
    {


        private static readonly string[] Stems = { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };
        private static readonly string[] Branches = { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };

        /// <summary>
        /// 根据 1-60 的序数返回对应的干支名称
        /// </summary>
        /// <param name="index">1 到 60 的干支序号</param>
        public static string GetGanZhi(int index)
        {
            // 将 1-60 转换为 0-59 的索引
            int i = (index - 1) % 60;

            string stem = Stems[i % 10];
            string branch = Branches[i % 12];

            return stem + branch;
        }
    }
}