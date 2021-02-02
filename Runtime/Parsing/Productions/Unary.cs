using System;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record Unary(Token Operator, Expression? Right) : Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"{GetType().Name} {Operator.Lexeme}");
            Right.PrintNode(indent, false);
        }
    }
}