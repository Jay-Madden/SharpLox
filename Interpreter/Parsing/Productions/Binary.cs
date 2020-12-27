using System;
using Interpreter.Lexing;

namespace Interpreter.Parsing.Productions
{
    public record Binary(Expression Left, Token Token, Expression Right): Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"{GetType().Name} {Token.Lexeme}");
            Right.PrintNode(indent, false);
            Left.PrintNode(indent, true);
        }
    }
}