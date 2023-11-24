using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LabCalculator
{
    public class LabCalculatorVisitor : LabCalculatorBaseVisitor<double>
    {
        // Таблиця ідентифікаторів
        Dictionary<string, double> tableIdentifier = new Dictionary<string, double>();

        public override double VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);
            return result;
        }

        public override double VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            var result = context.GetText();
            double value;
            // Видобути значення змінної з таблиці ідентифікаторів
            if (tableIdentifier.TryGetValue(result, out value))
            {
                return value;
            }
            else
            {
                return 0.0; // Повертати 0.0, якщо ідентифікатор не знайдений в таблиці
            }
        }

        public override double VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitExponentialExpr(LabCalculatorParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            Debug.WriteLine("{0} ^ {1}", left, right);
            return Math.Pow(left, right);
        }

        public override double VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
                Debug.WriteLine("{0} + {1}", left, right);
                return left + right;
            }
            else // LabCalculatorLexer.SUBTRACT
            {
                Debug.WriteLine("{0} - {1}", left, right);
                return left - right;
            }
        }

        public override double VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
            {
                Debug.WriteLine("{0} * {1}", left, right);
                return left * right;
            }
            else if (context.operatorToken.Type == LabCalculatorLexer.DIVIDE)
            {
                Debug.WriteLine("{0} / {1}", left, right);
                return left / right;
            }
            else if (context.operatorToken.Type == LabCalculatorLexer.MOD)
            {
                Debug.WriteLine("{0} mod {1}", left, right);
                return left % right;
            }
            else if (context.operatorToken.Type == LabCalculatorLexer.DIV)
            {
                Debug.WriteLine("{0} div {1}", left, right);
                return (int)left / (int)right;
            }
            return 0.0;
        }

        public override double VisitComparisonExpr(LabCalculatorParser.ComparisonExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            switch (context.operatorToken.Type)
            {
                case LabCalculatorLexer.EQUALS:
                    Debug.WriteLine("{0} = {1}", left, right);
                    return left == right ? 1 : 0;
                case LabCalculatorLexer.LESS:
                    Debug.WriteLine("{0} < {1}", left, right);
                    return left < right ? 1 : 0;
                case LabCalculatorLexer.GREATER:
                    Debug.WriteLine("{0} > {1}", left, right);
                    return left > right ? 1 : 0;
                case LabCalculatorLexer.LESSEQUAL:
                    Debug.WriteLine("{0} <= {1}", left, right);
                    return left <= right ? 1 : 0;
                case LabCalculatorLexer.GREATEREQUAL:
                    Debug.WriteLine("{0} >= {1}", left, right);
                    return left >= right ? 1 : 0;
                case LabCalculatorLexer.NOTEQUAL:
                    Debug.WriteLine("{0} <> {1}", left, right);
                    return left != right ? 1 : 0;
                default:
                    return 0.0;
            }
        }

        private double WalkLeft(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(0));
        }

        private double WalkRight(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(1));
        }
    }
}
