using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

namespace Kugua.Algorithms
{
    /// <summary>
    /// 证明一个数字=另一些数字序列的基本运算
    /// </summary>
    public class MathProof
    {
        
        List<double> tbase = new List<double>();

        /* Maintains the number of ways to decompose the given number. */
        int counter = 0;

        List<string> proofres = new List<string>();

        /* Maintains the number of calculations performed. */
        int calculation = 0;
        double desired = 0;


        /// <summary>
        /// 整数的数字论证
        /// </summary>
        /// <param name="desired1"></param>
        /// <returns></returns>
        public List<string> Proof(long desired1, List<int> baseNumbers)
        {
            List<string> proofChain = new List<string>();
            desired = desired1;
            if (baseNumbers == null ||  baseNumbers.Count <= 0) return proofChain;
            // string result = "";

            var res = GetStrongProofs(desired, baseNumbers);
            if (res.Count > 0)
            {
                // strong proof found.
                proofChain.Add($"{res[MyRandom.Next(res.Count)]}");
            }
            else
            {
                // try sum
                try
                {
                    string desc = "";
                    desired = Math.Abs(desired);
                    int time = 5;
                    while (time-- > 0)
                    {
                        proofChain.Add($"{String.Join("+", desired.ToString().ToArray())}");

                        desired = desired.ToString().Sum(c => c - '0');
                        
                        proofChain.Add($"{desired}");
                        res = GetStrongProofs(desired,baseNumbers);
                        if (res.Count > 0)
                        {
                            proofChain.Add(proofres[MyRandom.Next(proofres)]);
                            break;
                        }
                    }
                }
                catch
                {
                }
            }

            return proofChain;
        }

        /// <summary>
        /// 数字求和，并打印求和表达式字符串
        /// </summary>
        /// <param name="num"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        long getNumSum(long num, out string description)
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

        public List<string> GetStrongProofs(double inputDesired, List<int> inputTbase)
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
