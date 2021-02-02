using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using Runtime.Lexing;
using Runtime.Parsing;
using Runtime.Parsing.Productions;
using Expression = Runtime.Parsing.Productions.Expression;

namespace Runtime.Interpreting
{
    public class Interpreter : ISyntaxTreeVisitor<object>
    {
        
        public void Interpret(IEnumerable<Statement> statements)
        {
            foreach (var stmt in statements)
            {
                stmt.Accept(this);
            }
        }
        
        public object VisitBinary(Binary binary)
        {
            var left = Evaluate(binary.Left);
            var right = Evaluate(binary.Right);

            return (binary.Token.Type, CheckNumberOperand<double>(left, right)) switch
            {
                (TokenType.Greater, true) => (double) left > (double) right,
                (TokenType.GreaterEqual, true) => (double) left >= (double) right,
                (TokenType.Less, true) => (double) left < (double) right,
                (TokenType.LessEqual, true) => (double) left <= (double) right,
                (TokenType.EqualCompare, _) => IsEqual(left, right),
                (TokenType.NotEqual, _) => !IsEqual(left, right),
                (TokenType.Plus, _) => (left, right) switch
                    { 
                        (string l, string r) => l + r,
                        (double l, double r) => l + r,
                        _ => throw new RuntimeErrorException(binary.Token, "Operands must be two numbers or two strings.")
                    },
                (TokenType.Minus, true) => (double) left - (double) right,
                (TokenType.Star, true) => (double) left * (double) right,
                (TokenType.Slash, true) => (double) left / (double) right,
                (_, false) => throw new RuntimeErrorException(binary.Token, "Operands must be numbers."),
                _ => null!
            };
        }

        public object VisitConditional(Ternary ternary)
            => IsTruthy(Evaluate(ternary.Condition)) 
                ? Evaluate(ternary.TrueCase)
                : Evaluate(ternary.FalseCase);

        public object VisitGrouping(Grouping grouping) 
            => Evaluate(grouping.Expression);

        public object VisitLiteral(Literal literal) 
            => literal.Value;

        public object VisitUnary(Unary unary)
        {
            var right = Evaluate(unary.Right);

            return (unary.Operator.Type, CheckNumberOperand<double>(right)) switch
            {
                (TokenType.Minus, false) => throw new RuntimeErrorException(unary.Operator, "Operand must be a number."),
                (TokenType.Minus, _) => -(double) right,
                (TokenType.Not, _) => IsTruthy(right),
                _ => null!
            };
        }

        public object VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            Evaluate(expressionStatement.Expression);
            return null!;
        }

        public object VisitPrintStatement(PrintStatement printStatement)
        {
            Console.Write(Evaluate(printStatement.Expression));
            return null!;
        }

        private object Evaluate(Expression expression) 
            => expression.Accept(this);

        private bool IsTruthy(object obj)
            => obj is not bool b || b;

        private bool IsEqual(object? a, object? b) 
            => a?.Equals(b) ?? b == null;
        
        private bool CheckNumberOperand<T>(params object[] operands) 
            => operands.All(x => x is T);
    }
}