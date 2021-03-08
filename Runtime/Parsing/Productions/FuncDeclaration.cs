using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions
{
    public record FuncDeclaration(Token name, IEnumerable<Token> parameters, Block body) : Statement
    {
        public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
        {
            return visitor.VisitFuncDeclaration(this);
        }

        public override void PrintNode(string indent, bool last)
        {
            var newIndent = ShowIndent(indent, last);

            var args = string.Join(", ", parameters.Select(x => x.Lexeme));
            Console.WriteLine($"{GetType().Name}: {name.Lexeme} args={args}");
            body.PrintNode(newIndent, false);
        }
    }
}