using System;
using System.IO;
using Interpreter.Lexing;
using Interpreter.Parsing;
using Interpreter.Parsing.Productions;

namespace SharpLox
{
    class Program
    {
        static void Main(string[] args)
        {
            Expression expression = new Binary(
                new Unary(
                    new Token(TokenType.Minus, "-", null!, 1),
                    new Literal(123)),
                new Token(TokenType.Star, "*", null!, 1),
                new Grouping(
                    new Literal(45.67)));

            
            expression.PrintNode("", true);
            
            /*
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: sharplox [script]");
                Environment.Exit(64);
            }
            SharpLox.InitializeInterpreter(args);
            */
        }
    }
}
