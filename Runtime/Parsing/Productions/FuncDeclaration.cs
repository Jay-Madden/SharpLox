using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record FuncDeclaration(Token Name, IEnumerable<Token> Parameters, Block Body) : Statement
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitFuncDeclaration(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            var newIndent = ShowIndent(indent, last);

            var args = string.Join(", ", Parameters.Select(x => x.Lexeme));
            Console.WriteLine($"{GetType().Name}: {Name.Lexeme} args={args}");
            Body.PrintNode(newIndent, false);
        }
    }
}