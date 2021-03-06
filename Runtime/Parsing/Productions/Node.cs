using System;

namespace Runtime.Parsing.Productions
{
    public abstract record Node
    {
        public abstract T Accept<T>(ISyntaxTreeVisitor<T> visitor);
        
        public abstract void PrintNode(string indent, bool last);

        protected string ShowIndent(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("├─ ");
                indent += "│ ";
            }
            return indent;
        }
        
    }
}