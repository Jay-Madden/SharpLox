using System;
using System.IO;
using Interpreter.Lexing;

namespace SharpLox
{
    public class SharpLox
    {
        private const string PromptStr = "|>> ";
        
        private static bool ErrorState = false;
        
        public static void InitializeInterpreter(string[] args)
        {
            if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            var prog = File.ReadAllText(Path.GetFullPath(path));
            Run(prog);
            if (ErrorState)
            {
                Environment.Exit(65);
            }
        }

        private static void RunPrompt()
        {
            Console.WriteLine("Sharplox Version 0.1 (Alpha)");
            Console.WriteLine("Sharplox REPL initialized:");
            
            while (true)
            {
                Console.Write(PromptStr);
                var line = Console.ReadLine();
                if (line is null)
                {
                    break;
                }

                Run(line);
                ErrorState = false;
            }
        }
        private static void Run(string source)
        {
            var lexer = new Lexer(source, Error);
            var tokens = lexer.LexTokens();

            // For now, just print the tokens.
            foreach(var token in tokens) {
                Console.WriteLine(token);
            }
        }

        public static void Error(int line, string error)
        {
            Report(line, "", error);
        }

        public static void Report(int line, string where, string message)
        {
            Console.WriteLine($"Line: {line} Error: {where}: {message}");
            ErrorState = true;
        }
    }
}