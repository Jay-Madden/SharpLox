using System;
using System.Collections.Generic;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record PropertyGet(Token Identifier, Expression Expression): Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitPropertyGet(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Console.WriteLine($"Property Name: {Identifier.Lexeme}");
            Expression.PrintNode(indent, false);
        }
    }
}