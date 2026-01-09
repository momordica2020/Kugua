using Kugua.Core;
using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Text;

namespace Kugua.Integrations.Generators
{
    /// <summary>
    /// 生成乱数假文
    /// </summary>
    public class LoremText
    {

        /// <summary>
        /// 简体汉字标点符号，根据出现频率调整
        /// </summary>
        public static string _jtPunctuationDB = "，，，，，，、、；。。。。！！……？？**%@";

        /// <summary>
        /// 整句末尾标点符号
        /// </summary>
        public static string[] _endPunctuations = { "。", "！", "？" };



        ///// <summary>
        ///// 生成纯随机字符文本
        ///// </summary>
        //private string GetPlainCharacters(int textSize, string characterDB)
        //{
        //    if (textSize > characterDB.Length || textSize <= 0)
        //    {
        //        throw new ArgumentException("文本长度无效");
        //    }

        //    StringBuilder plainCharacters = new StringBuilder();
        //    for (int i = 0; i < textSize; i++)
        //    {
        //        plainCharacters.Append(characterDB[GetRandomNumber(characterDB.Length)]);
        //    }

        //    return plainCharacters.ToString();
        //}

        /// <summary>
        /// 生成带标点符号的随机文本
        /// </summary>
        public static string GetSim(int textSize)
        {
            StringBuilder characters = new StringBuilder();
            characters.Append(Util.jt[MyRandom.Next(Util.jt.Length)]);

            for (int i = 1; i < textSize; i++)
            {
                if (MyRandom.NextDouble < 0.04)
                {
                    if (_jtPunctuationDB.IndexOf(characters[characters.Length - 1]) == -1)
                    {
                        characters.Append(_jtPunctuationDB[MyRandom.Next(_jtPunctuationDB.Length)]);
                        continue;
                    }
                }
                characters.Append(Util.jt[MyRandom.Next(Util.jt.Length)]);
            }

            // 添加末尾标点
            if (_jtPunctuationDB.IndexOf(characters[characters.Length - 1]) == -1)
            {
                characters.Append(_endPunctuations[MyRandom.Next(_endPunctuations.Length)]);
            }

            // 处理换行和特殊标点
            string result = $"{characters}";
            result = result.Replace("*", "。\r\n")
                         .Replace("%", "！\r\n")
                         .Replace("@", "？\r\n");

            return result;
        }

    }
}
