
namespace Kugua.Core{


    public class Calculator
    {
        private string _expression;
        private int _index;

        public Calculator(string expression)
        {
            _expression = expression.Replace(" ", "").Replace("（", "(").Replace("）", ")");
            _index = 0;
        }

        public double Evaluate()
        {
            return ParseExpression();
        }

        private double ParseExpression()
        {
            double result = ParseTerm();
            while (_index < _expression.Length)
            {
                char op = _expression[_index];
                if (op == '+' || op == '-')
                {
                    _index++;
                    double term = ParseTerm();
                    if (op == '+')
                        result += term;
                    else
                        result -= term;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private double ParseTerm()
        {
            double result = ParseFactor();
            while (_index < _expression.Length)
            {
                char op = _expression[_index];
                if (op == '*' || op == '/' || op == '%')
                {
                    _index++;
                    double factor = ParseFactor();
                    if (op == '*')
                        result *= factor;
                    else if (op == '/')
                        result /= factor;
                    else if (op == '%')
                        result %= factor;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private double ParseFactor()
        {
            double result = ParsePrimary();
            while (_index < _expression.Length)
            {
                char op = _expression[_index];
                if (op == '^')
                {
                    _index++;
                    double primary = ParsePrimary();
                    result = Math.Pow(result, primary);
                }
                else if (_expression[_index] == '√')
                {
                    _index++;
                    result = Math.Sqrt(result);
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private double ParsePrimary()
        {
            if (_expression[_index] == '(')
            {
                _index++;
                double result = ParseExpression();
                if (_expression[_index] != ')')
                    throw new Exception("Expected closing parenthesis");
                _index++;
                return result;
            }
            else if (_expression[_index] == '-')
            {
                _index++;
                return -ParsePrimary();
            }
            else
            {
                return ParseNumber();
            }
        }

        private double ParseNumber()
        {
            int start = _index;
            if (_expression[_index] == 'e')
            {
                _index++;
                return Math.E;
            }
            if (_index + 1 < _expression.Length && _expression[_index]=='p' &&  _expression[_index + 1] == 'i')
            {
                _index += 2;
                return Math.PI;
            }
            while (_index < _expression.Length && (char.IsDigit(_expression[_index]) || _expression[_index] == '.'))
            {
                _index++;
            }
            return double.Parse(_expression.Substring(start, _index - start));
        }
    }


}