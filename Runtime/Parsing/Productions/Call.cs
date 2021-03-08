using System;
using System.Collections.Generic;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record Call(Expression callee, Token? paren, IEnumerable<Expression> args): Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitCall(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"{GetType().Name}");
            callee.PrintNode(indent, false);
            foreach (var arg in args)
            {
                arg.PrintNode(indent, false);
            }
        }
    }
}