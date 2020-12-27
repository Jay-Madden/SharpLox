using System.Collections.Generic;
using Interpreter.Parsing.Productions;

namespace Interpreter.Parsing
{
    public interface ISyntaxTreeVisitor<out T>
    {
        public T VisitBinary(Binary binary);
        public T VisitGrouping(Grouping grouping);
        public T VisitLiteral(Literal literal);
        public T VisitUnary(Unary unary);
    }
}