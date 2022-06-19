using System;
using System.Collections.Generic;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record PropertySet(Expression Name, Token Identifier, Expression Value): Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitPropertySet(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            indent = ShowIndent(indent, last);
            Name.PrintNode(indent, false);
            Console.WriteLine($"Property Name: {Identifier.Lexeme}");
            Value.PrintNode(indent, false);
        }
    }
}