using System;

namespace Interpreter.Parsing.Productions
{
    public record Literal(object Value) : Expression
    {
       public override T Accept<T>(ISyntaxTreeVisitor<T> visitor)
       {
           return visitor.VisitLiteral(this);
       }

       public override void PrintNode(string indent, bool last)
       {
            indent = ShowIndent(indent, true); 
            Console.WriteLine($"{GetType().Name} {Value}");
       }
    }
}