using System.Collections.Generic;

namespace Runtime.Lexing
{
    public class ReservedKeywords
    {
        // Keywords.
        public static Dictionary<string, TokenType> Keywords => new()
        {
            {"and", TokenType.And},
            {"or", TokenType.Or},
            {"class", TokenType.Class},
            {"else", TokenType.Else},
            {"false", TokenType.False},
            {"func", TokenType.Func},
            {"if", TokenType.If},
            {"nil", TokenType.Nil},
            {"for", TokenType.For},
            {"this", TokenType.This},
            {"return", TokenType.Return},
            {"super", TokenType.Super},
            {"true", TokenType.True},
            {"var", TokenType.Var},
            {"while", TokenType.While},
            {"break", TokenType.Break},
        };
    }
}