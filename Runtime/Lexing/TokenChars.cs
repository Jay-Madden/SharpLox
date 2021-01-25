namespace Runtime.Lexing
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
        public const char DoubleQuote = '"';
        public const char Question = '?';
        public const char Colon = ':';

        // One or two character tokens.
        public const char Not = '!';
        public const char Equal = '=';
        public const char Greater = '>';
        public const char Lesser = '<';



        public const char Space = ' ';
        public const char Carriage = '\r';
        public const char Tab = '\t';
        public const char NewLine= '\n';
    }
}