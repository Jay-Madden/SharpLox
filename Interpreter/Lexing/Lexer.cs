using System;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter.Lexing
{
    public class Lexer
    {
        private int Start;
        private int Current;
        private int Line = 1;

        private readonly Action<int, string> ErrorHandler;
        
        private readonly string Source;

        private readonly List<Token> Tokens = new();

        private string CurrentLexeme => Source[Start..Current];
        
        private bool IsAtEnd => Current >= Source.Length;

        public Lexer(string source, Action<int, string> errorHandler)
        {
            Source = source;
            ErrorHandler = errorHandler;
        }

        public List<Token> LexTokens()
        {
            while (!IsAtEnd)
            {
                Start = Current;
                var token = LexToken();
                if (token is null)
                {
                    continue;
                }
                
                switch (token.Type)
                {
                    case TokenType.Comment:
                    case TokenType.WhiteSpace:
                        break;
                    case TokenType.NewLine:
                        Line++;
                        break;
                    default:
                        Tokens.Add(token);
                        break;
                }
            }
            
            Tokens.Add(new Token(TokenType.Eof, "", null!, Line));
            return Tokens;
        }

        private Token? LexToken()
        {
            switch (Advance())
            {
                case TokenChars.LeftParen:
                    return CreateToken(TokenType.LeftParen);
                case TokenChars.RightParen:
                    return CreateToken(TokenType.RightParen);
                case TokenChars.LeftBrace:
                    return CreateToken(TokenType.LeftBrace);
                case TokenChars.RightBrace:
                    return CreateToken(TokenType.RightBrace);
                case TokenChars.Comma:
                    return CreateToken(TokenType.Comma);
                case TokenChars.Dot:
                    return CreateToken(TokenType.Dot);
                case TokenChars.Plus:
                    return CreateToken(TokenType.Plus);
                case TokenChars.Minus:
                    return CreateToken(TokenType.Minus);
                case TokenChars.Star:
                    return CreateToken(TokenType.Star);
                case TokenChars.Semicolon:
                    return CreateToken(TokenType.Semicolon);
                case TokenChars.Not:
                    return CreateToken(
                        LookAhead(TokenChars.Equal) ?
                            TokenType.NotEqual :
                            TokenType.Not);
                case TokenChars.Equal:
                    return CreateToken(
                        LookAhead(TokenChars.Equal) ?
                            TokenType.EqualCompare :
                            TokenType.EqualAssign);
                case TokenChars.Greater:
                    return CreateToken(
                        LookAhead(TokenChars.Equal) ?
                            TokenType.GreaterEqual :
                            TokenType.Greater);
                case TokenChars.Lesser:
                    return CreateToken(
                        LookAhead(TokenChars.Equal) ?
                            TokenType.LessEqual :
                            TokenType.Less);
                case TokenChars.Slash:
                    return Comment();
                case TokenChars.Space:
                case TokenChars.Carriage:
                case TokenChars.Tab:
                    return CreateToken(TokenType.WhiteSpace);
                case TokenChars.NewLine:
                    return CreateToken(TokenType.NewLine);
                case TokenChars.DoubleQuote:
                    return String();
                case { } c when char.IsDigit(c):
                    return Number();
                case { } c when char.IsLetter(c):
                    return Identifier();
                default:
                    ErrorHandler(Line, "Unexpected Character");
                    return null;
            }
        }

        private Token Identifier()
        {
            while (char.IsLetterOrDigit(PeekAhead()))
            {
                Advance();
            }

            if (ReservedKeywords.Keywords.TryGetValue(CurrentLexeme, out var keyword))
            {
                return CreateToken(keyword);
            }

            return CreateToken(TokenType.Identifier);
        }

        private Token? Number()
        {
            while (char.IsDigit(PeekAhead()))
            {
                Advance();
            }

            if (PeekAhead() == TokenChars.Dot && char.IsDigit(PeekNext()))
            {
                Advance();
                while (char.IsDigit(PeekAhead()))
                {
                    Advance();
                }
            }

            if (!double.TryParse(CurrentLexeme, out var val))
            {
                ErrorHandler(Line, "Invalid number literal provided");
                return null;
            }

            return CreateToken(TokenType.Number, CurrentLexeme);
        }
        private Token Comment()
        {
            if(PeekAhead() == TokenChars.Slash)
            {
                Advance();
                while (PeekAhead() != TokenChars.NewLine && !IsAtEnd)
                {
                    Advance();
                }
                return CreateToken(TokenType.Comment);
            }
            else if (PeekAhead() == TokenChars.Star)
            {
                Advance();
                while (!IsAtEnd && PeekAhead() != TokenChars.Star && PeekNext() != TokenChars.Slash)
                {
                    Advance();
                }
                Advance(2);
                return CreateToken(TokenType.Comment);
            }
            return CreateToken(TokenType.Slash);
        }

        private Token? String()
        {
            while (PeekAhead() != TokenChars.DoubleQuote && !IsAtEnd)
            {
                if (PeekAhead() == TokenChars.NewLine)
                {
                    Line++;
                }
                Advance();
            }

            if (IsAtEnd)
            {
                ErrorHandler(Line, "Unterminated String");
                return null;
            }

            Advance();

            //Return the created token with the value of the string and the "" trimmed off the ends
            return CreateToken(TokenType.String, Source[(Start + 1)..(Current - 1)]);
        }
        
        private bool LookAhead(char expected) {
            if (IsAtEnd || Source[Current] != expected)
            {
                return false;
            }

            Current++;
            return true;
        }
        
        private char PeekAhead() => IsAtEnd ? '\0' : Source[Current];
        
        private char PeekNext() {
            if (Current + 1 >= Source.Length)
            {
                return '\0';
            }
            return Source[Current + 1];
        } 

        private Token CreateToken(TokenType type)
            => new(type, CurrentLexeme, null!, Line);
        
        private Token CreateToken(TokenType type, string source)
            => new(type, source, null!, Line);
        
        private char Advance(int count=1) {
            Current += count;
            return Source[Current - 1];
        }
    }
}