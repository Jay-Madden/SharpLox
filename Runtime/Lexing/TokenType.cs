namespace Runtime.Lexing 
{
    public enum TokenType
    {
        // Single-character tokens.
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        Comma,
        Dot,
        Minus,
        Plus,
        Semicolon,
        Slash,
        Star,
        Question,
        Colon,

        // One or two character tokens.
        Not,
        NotEqual,
        EqualAssign,
        EqualCompare,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,

        // Keywords.
        And,
        Class,
        Else,
        False,
        Func,
        For,
        If,
        Nil,
        Or,
        Print,
        Return,
        Super,
        This,
        True,
        Var,
        While,

        // Literals.
        Identifier,
        String,
        Number,
        
        Comment,
        NewLine,
        WhiteSpace,
        Eof
    }
}