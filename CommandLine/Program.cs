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
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: sharplox [script]");
                Environment.Exit(64);
            }
            SharpLox.InitializeInterpreter(args);
        }
    }
}
