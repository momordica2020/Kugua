using Kugua.Core;
using Kugua.Mods.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;




namespace Kugua.Mods
{

    /// <summary>
    /// 字符串数字论证
    /// </summary>
    public class ModProof : Mod
    {

        Dictionary<string, int> bhdict = new Dictionary<string, int>();
        List<double> tbase = new List<double>();

        /* Maintains the number of ways to decompose the given number. */
        int counter = 0;

        List<string> proofres = new List<string>();
        string finalproof = "";

        /* Maintains the number of calculations performed. */
        int calculation = 0;
        double desired = 0;




        public override bool Init(string[] args)
        {
            try
            {
                ModCommands.Add(new ModCommand(new Regex(@"^数字论证(.+)", RegexOptions.Singleline), GetProof));


                bhdict = new Dictionary<string, int>();
                var lines = LocalStorage.ReadResourceLines("Bihua");
                foreach (var line in lines)
                {
                    string[] vitem = line.Split('\t');
                    if (vitem.Length >= 2) bhdict[vitem[0]] = int.Parse(vitem[1]);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            return true;
        }

        /// <summary>
        /// homo特有的数字论证
        /// 数字论证犬走椛/数字论证12dora
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string GetProof(MessageContext context, string[] param)
        {
            var message = param[1].Trim();
            long trynum;
            bool succeed = false;
            if (long.TryParse(message, out trynum))
            {
                // 纯数字
                succeed = getProof(trynum);
                if (succeed)
                {
                    return finalproof;
                }
            }
            else
            {
                const string numb = "0123456789零一二三四五六七八九";
                const string engc = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string symb = "\t\r\n []【】！@#￥%…&*（）+=-—_!@#$%^&*()|/\\。、，？?“”\"',";
                bool eng = false;
                bool chn = false;
                var tmp = new List<(char c , int v)>(); 
                foreach(var c in message)
                {
                    long sum = 0;
                    if (symb.Contains(c)) continue;
                    if(numb.Contains(c))
                    {
                        tmp.Add((c, numb.IndexOf(c) % 10 ));
                        eng = true;
                        continue;
                    }
                    if (engc.Contains(c))
                    {
                        tmp.Add((c, engc.IndexOf(c) % 26 + 1));
                        eng = true;
                        continue;
                    }
                    if(bhdict.TryGetValue(c.ToString(), out var cv))
                    {
                        tmp.Add((c, cv));
                        chn = true;
                        continue;
                    }
                }

                if (tmp.Count > 0)
                {
                    var trysum = tmp.Sum(a=>a.v);
                    succeed = getProof(trysum);
                    if (succeed)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("换算成");
                        if (eng) sb.Append($"字母序号 ");
                        if (chn) sb.Append($"笔画数");
                        sb.Append("，");

                        sb.Append($"{message} = {string.Join(" + ",tmp.Select(t=>t.v))} = {finalproof}");
                        return sb.ToString();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(finalproof))
            {
                return $"论 证 大 失 败";
            }
            return "";
        }


        

        /// <summary>
        /// 数字求和，并打印求和表达式字符串
        /// </summary>
        /// <param name="num"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public long getNumSum(long num, out string description)
        {
            string ns = num.ToString();
            long sum = 0;
            description = "";
            foreach (var c in ns)
            {
                description += c + " + ";
                sum += long.Parse(c.ToString());
            }
            if (description.EndsWith("+ ")) description = description.Substring(0, description.Length - 2);
            description += "= " + sum;
            return sum;
        }

        /// <summary>
        /// 整数的数字论证
        /// </summary>
        /// <param name="desired1"></param>
        /// <returns></returns>
        public bool getProof(long desired1)
        {
            desired = desired1;
            // string result = "";

            var res = StrongProof(desired, [1, 1, 4, 5, 1, 4, ]);
            if (res.Count > 0)
            {
                // have strong.
                finalproof = $"{res[MyRandom.Next(res.Count)]}\r\nQ.E.D";
                return true;
            }
            else
            {
                // try sum
                try
                {
                    finalproof = "";
                    int time = 5;
                    while (time > 0)
                    {
                        time--;
                        string desc = "";
                        desired = getNumSum((long)desired, out desc);
                        if (finalproof.Length > 0) finalproof += " = ";
                        finalproof += desc;
                        res = StrongProof(desired, [1, 1, 4, 5, 1, 4, ]);
                        if (res.Count > 0)
                        {
                            string p1 = proofres[MyRandom.Next(proofres.Count)];
                            finalproof += $" = {p1}\r\nQ.E.D";
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;

        }

        public List<string> StrongProof(double inputDesired, List<int> inputTbase)
        {
            desired = inputDesired;
            proofres.Clear();
            calculation = 0;
            counter = 0;
            var alltbase = getMixed(inputTbase);
            alltbase.Sort((left, right) =>
            {
                return left.Count.CompareTo(right.Count);
            });
            foreach (var t in alltbase)
            {
                if (t.Count <= 1)
                {
                    continue;
                }
                tbase = t.ConvertAll(x => (double)x);


                Process([0, 0], t.Count - 1 + t.Count);

                Process([-1, 0], t.Count - 1 + t.Count);
            }


            return proofres;
        }

        List<List<int>> getMixed(List<int> inputTbase)
        {
            var res = new List<List<int>>();

            if (inputTbase.Count <= 1)
            {
                res.Add(inputTbase);
            }
            //else if (inputTbase.Count == 2)
            //{
            //    res.Add(inputTbase);
            //    res.Add(new List<int> { convert(inputTbase[0], inputTbase[1]) });
            //}
            else
            {
                var r0 = inputTbase[0];
                var r1 = getMixed(inputTbase.GetRange(1, inputTbase.Count - 1));
                foreach (var r in r1)
                {
                    var rr1 = new List<int>();
                    rr1.Add(r0);
                    rr1.AddRange(r.ToArray());
                    res.Add(rr1);
                    var rr2 = new List<int>();
                    var cr = convert(r0, r[0]);
                    if (cr > 0)
                    {
                        rr2.Add(convert(r0, r[0]));
                        rr2.AddRange(r.GetRange(1, r.Count - 1));
                        res.Add(rr2);
                    }



                }
            }

            return res;
        }

        int convert(int a, int b)
        {
            string aa = Math.Abs(a).ToString();
            string bb = Math.Abs(b).ToString();
            try
            {
                return int.Parse(aa + bb);
            }
            catch
            {
                return -1;
            }

        }

        void Process(List<int> v, int maxLength)
        {
            Put(v, 2, 2, 0, maxLength);
        }

        /// <summary>
        /// Recursively generate Reverse Polish Notation (RPN逆波兰表达式).
        /// -1: -1, 0: number, 1: +, 2: -, 3: *, 4: /, 5: ^
        /// </summary>
        /// <param name="v"></param>
        /// <param name="pos"></param>
        /// <param name="numCount"></param>
        /// <param name="symCount"></param>
        /// <param name="length"></param>

        void Put(List<int> v, int pos, int numCount, int symCount, int length)
        {
            if (proofres.Count > 0) return;
            if (pos == length)
            {
                if (CheckExpression(v))
                {
                    counter++;
                    PrintRPN(v);
                }
                calculation++;
                return;
            }

            int lowerBound = (numCount == (length + 1) / 2) ? 1 : 0;
            int upperBound = (symCount == (length + 1) / 2 - 1 || symCount == numCount - 1) ? 1 : (length + 1) / 2;

            for (int value = lowerBound; value < upperBound; value++)
            {
                if (pos >= v.Count)
                {
                    v.Add(value);
                }
                else
                {
                    v[pos] = value;
                }

                if (value == 0)
                {
                    Put(v, pos + 1, numCount + 1, symCount, length);
                }
                else
                {
                    Put(v, pos + 1, numCount, symCount + 1, length);
                }
            }
        }

        bool CheckExpression(List<int> seed)
        {
            Stack<double> stack = new Stack<double>();
            int numIndex = 0;

            foreach (int token in seed)
            {
                if (token == 0 || token == -1)
                {
                    double value = (token == -1 && tbase.Count > 0) ? -tbase[0] : (numIndex < tbase.Count ? tbase[numIndex++] : 0);
                    stack.Push(value);
                }
                else
                {
                    if (stack.Count < 2) return false;

                    double second = stack.Pop();
                    double first = stack.Pop();

                    switch (token)
                    {
                        case 1: stack.Push(first + second); break;
                        case 2: stack.Push(first - second); break;
                        case 3: stack.Push(first * second); break;
                        case 4: stack.Push(first / second); break;
                        case 5: stack.Push(Math.Pow(first, second)); break;
                    }
                }
            }

            return stack.Count == 1 && stack.Peek() == desired;
        }

        void PrintRPN(List<int> seed)
        {
            StringBuilder output = new StringBuilder();
            int numIndex = 0;

            foreach (int token in seed)
            {
                switch (token)
                {
                    case -1:
                        output.AppendFormat("-{0} ", numIndex < tbase.Count ? (int)tbase[numIndex++] : 0);
                        numIndex++;
                        break;
                    case 0:
                        output.AppendFormat("{0} ", numIndex < tbase.Count ? (int)tbase[numIndex++] : 0);
                        break;
                    case 1: output.Append("+ "); break;
                    case 2: output.Append("- "); break;
                    case 3: output.Append("* "); break;
                    case 4: output.Append("/ "); break;
                    case 5: output.Append("^ "); break;
                }
            }
            var rs = Translate(output.ToString());
            if (!string.IsNullOrWhiteSpace(rs))
            {
                string result = $"{rs}\r\n";
                proofres.Add(result);
                //return result;
            }

        }

        string Translate(string input)
        {
            Stack<(string, char)> stack = new Stack<(string, char)>();
            StringBuilder operand = new StringBuilder();
            string symbols = "+-*/^";
            //char lastSymLeft = ' ';
            //char lastSymRight = ' ';
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ')
                {
                    continue;
                }
                if (symbols.IndexOf(input[i]) == -1)// || (input[i] =='-' && input[i+1] != ' ' && symbols.IndexOf(input[i+1]) == -1))
                {
                    operand.Clear();
                    //if(lastSymRight!=' ')lastSymLeft = lastSymRight;
                    //lastSymRight = ' ';
                    while (i < input.Length && input[i] != ' ')
                    {
                        operand.Append(input[i++]);
                    }
                    stack.Push((operand.ToString(), ' '));
                }
                else
                {
                    if (stack.Count < 2) return "";

                    var right = stack.Pop();
                    var left = stack.Pop();
                    char sym = input[i];
                    string subExpr = "";
                    if (levelLow(left.Item2, sym))
                    {
                        subExpr += $"({left.Item1})";
                    }
                    else
                    {
                        subExpr += left.Item1;
                    }
                    subExpr += sym;
                    if (levelLow(right.Item2, sym))
                    {
                        subExpr += $"({right.Item1})";
                    }
                    else
                    {
                        subExpr += right.Item1;
                    }
                    //string subExpr = i == input.Length - 2 ? left + input[i] + right : ();
                    stack.Push((subExpr, sym));
                }
            }
            return stack.Peek().Item1;
        }

        /// <summary>
        /// 里侧比外侧算术优先级低，true则需要加括号
        /// </summary>
        /// <param name="sym1"></param>
        /// <param name="sym2"></param>
        /// <returns></returns>
        bool levelLow(char sym1, char sym2)
        {
            if (sym1 == ' ' || sym2 == ' ') return false;
            //if (sym1 == sym2) return false;
            //if ((sym1 == '+' ) && (sym2 == '-' )) return true;
            if ((sym1 == '+') && (sym2 == '-')) return true;
            if ((sym1 == '-') && (sym2 == '-')) return true;
            if ((sym1 == '+' || sym1 == '-') && (sym2 != '+' && sym2 != '-')) return true;
            if ((sym1 == '*') && (sym2 == '/')) return true;
            if ((sym1 == '/') && (sym2 == '*' || sym2 == '/')) return true;
            if ((sym1 == '*' || sym1 == '/') && (sym2 == '^')) return true;
            if ((sym1 == '^') && (sym2 == '^')) return true;
            return false;
        }
    }


}
