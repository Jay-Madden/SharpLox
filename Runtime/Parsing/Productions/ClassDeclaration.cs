using System;
using System.Collections.Generic;
using Runtime.Lexing;

namespace Runtime.Parsing.Productions;

public record ClassDeclaration(Token Name, VariableAccess? SuperClass, IEnumerable<Node> Methods) : Node
{
    public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
    {
        return visitor.VisitClassDeclaration(this);
    }

    public override void PrintNode(string indent, bool last)
    {
        var newIndent = ShowIndent(indent, last);

        Console.WriteLine($"{GetType().Name}: {Name.Lexeme}");
        foreach (var method in Methods)
        {
            method.PrintNode(newIndent, false);
        }
    }
}