using System;
using Interpreter.Parsing.Productions;

namespace Interpreter.Parsing
{
    public class PrettyPrinter: ISyntaxTreeVisitor<string>
    {
        private string _output = string.Empty;
        
        public string Print<T>(T node) where T: Expression
        {
            _output = string.Empty;
            
            return node.Accept(this);
        }
        
        public string VisitBinary(Binary binary)
        {
            return $"(Binary {binary.Left} {binary.Right})";
        }

        public string VisitGrouping(Grouping grouping)
        {
            return $"(Grouping {grouping.Expression})";
        }

        public string VisitLiteral(Literal literal)
        {
            return literal.Value.ToString() ?? string.Empty;
        }

        public string VisitUnary(Unary unary)
        {
            return $"(unary {unary.Right})";
        }
        
        
    }
}