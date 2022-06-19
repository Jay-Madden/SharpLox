using System.Collections.Generic;
using Runtime.Lexing;
using Runtime.Parsing.Productions;

namespace Runtime.Interpreting;

public class SharpLoxInstance
{
    private readonly SharpLoxClass _loxClass;
    
    private readonly Dictionary<string, object> _fields = new ();

    public SharpLoxInstance(SharpLoxClass c)
    {
        _loxClass = c;
    }

    public object Get(Token name) =>
        _fields.TryGetValue(name.Lexeme, out var val)
            ? val
            : throw new RuntimeErrorException(name, $"Undefined property {name.Lexeme}");

    public void Set(Token name, object value)
    {
        _fields[name.Lexeme] = value;
    }
    
    public override string ToString()
        => $"Instance of: {_loxClass.Name}";
}