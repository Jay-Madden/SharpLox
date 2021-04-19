using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record Lambda(IEnumerable<Token> Parameters, Block Body) : Expression
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitLambda(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            var newIndent = ShowIndent(indent, last);

            var args = string.Join(", ", Parameters.Select(x => x.Lexeme));
            Console.WriteLine($"{GetType().Name}: args={args}");
            Body.PrintNode(newIndent, false);
        }
    }
}