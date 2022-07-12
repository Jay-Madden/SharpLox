using System;
using System.IO;
using System.Linq;
using Runtime.Interpreting;
using Runtime.Lexing;
using Runtime.Parsing;
using Runtime.SemanticAnalysis;

namespace SharpLox
{
    public class SharpLox
    {
        private const string PromptStr = "|>> ";
        
        private static bool ErrorState { get; set; }

        private static bool RuntimeErrorState { get; set; }

        private static readonly Interpreter _interpreter = new();
        
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
            var prog = File.ReadAllText(path);
            Run(prog, false);
            if (ErrorState)
            {
                Environment.Exit(65);
            }

            if (RuntimeErrorState)
            {
                Environment.Exit(70);
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

                Run(line, true);
                ErrorState = false;
            }
        }
        
        private static void Run(string source, bool isRepl)
        {
            var lexer = new Lexer(source, LexError);
            var tokens = lexer.LexTokens();

            var parser = new Parser(tokens, ParseError, isRepl);
            try
            {
                var ast = parser.Parse().ToList();
                
                if (ErrorState)
                {
                    return;
                }
                
                var resolver = new NameResolver(_interpreter, ParseError);
                resolver.Resolve(ast);
                
                if (ErrorState)
                {
                    return;
                }
                _interpreter.Interpret(ast);
            }
            catch (ParseErrorException)
            { }
            catch (RuntimeErrorException e)
            {
                RuntimeError(e);
            }
        }

        private static void LexError(int line, string error)
        {
            Report(line, "", error);
        }
        
        private static void ParseError(Token token, string message) 
        {
            Report(token.Line, 
                token.Type == TokenType.Eof 
                    ? "at end" 
                    : $"at '{token.Lexeme}'", 
                message);
        }

        private static void RuntimeError(RuntimeErrorException e)
        {
            Console.WriteLine($"{e.Message}{Environment.NewLine}Line: {e.Token?.Line ?? -1}");
            RuntimeErrorState = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"Line {line}: Error {where}: {message}");
            ErrorState = true;
        }
    }
}