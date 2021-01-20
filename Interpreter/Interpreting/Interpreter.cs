using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using Interpreter.Lexing;
using Interpreter.Parsing;
using Interpreter.Parsing.Productions;
using Expression = Interpreter.Parsing.Productions.Expression;

namespace Interpreter.Interpreting
{
    public class Interpreter: ISyntaxTreeVisitor<object>
    {
        public object VisitBinary(Binary binary)
        {
            var left = Evaluate(binary.Left);
            var right = Evaluate(binary.Right);

            return binary.Token.Type switch
            {
                TokenType.Greater => (double) left > (double) right,
                TokenType.GreaterEqual => (double) left >= (double) right,
                TokenType.Less => (double) left < (double) right,
                TokenType.LessEqual => (double) left <= (double) right,
                TokenType.Plus => (left, right) switch
                {
                    (string l, string r) => l + r,
                    (double l, double r) => l + r,
                    _ => null!
                },
                TokenType.Minus => (double) left - (double) right,
                TokenType.Star => (double) left * (double) right,
                TokenType.Slash => (double) left / (double) right,
                _ => null!
            };
        }

        public object VisitGrouping(Grouping grouping)
        {
            return Evaluate(grouping.Expression);
        }

        public object VisitLiteral(Literal literal)
        {
            return literal.Value;
        }

        public object VisitUnary(Unary unary)
        {
            var right = Evaluate(unary.Right);

            return unary.Operator.Type switch
            {
                TokenType.Minus => -(double) right,
                TokenType.Not => IsTruthy(right),
                _ => null!
            };
        }

        private object Evaluate(Expression expression)
        {
            return expression.Accept(this);
        }
        
        private bool IsTruthy(object obj) 
            => obj is not bool b || b;
    }
}