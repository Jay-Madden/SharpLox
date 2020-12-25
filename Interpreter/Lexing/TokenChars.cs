namespace Interpreter.Lexing
{
    public class TokenChars
    {
        // Single-character tokens.
        public const char LeftParen = '(';
        public const char RightParen = ')';
        public const char LeftBrace = '{';
        public const char RightBrace = '}';
        public const char Comma = ',';
        public const char Dot = '.';
        public const char Minus = '-';
        public const char Plus = '+';
        public const char Semicolon = ';';
        public const char Slash = '/';
        public const char Star = '*';

        // One or two character tokens.
        public const char Not = '!';
        public const char Equal = '=';
        public const char Greater = '>';
        public const char Lesser = '<';

        // Keywords.
        public const string And = "and";
        public const string Class = "class";
        public const string Else = "else";
        public const string False = "false";
        public const string Func = "func";
        public const string For = "for";
        public const string If = "if";
        public const string Nil = "nil";
        public const string Or = "or";
        public const string Print = "print";
        public const string Return = "return";
        public const string Super = "super";
        public const string This = "this";
        public const string True = "true";
        public const string Var = "var";
        public const string While = "while";

        public const char Space = ' ';
        public const char Carriage = '\r';
        public const char Tab = '\t';
        public const char NewLine= '\n';
    }
}