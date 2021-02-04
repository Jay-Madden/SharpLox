using System;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record Logical(Expression Left, Token Op, Expression Right): Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitLogical(this);

        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"{GetType().Name} {Op.Type}");
            Right.PrintNode(indent, false);
            Left.PrintNode(indent, true);
        }
    }
}