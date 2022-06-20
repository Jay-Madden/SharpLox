using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
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

    public object Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out var val))
        {
            return val;
        }

        var method = _loxClass.GetMethod(name);

        if (method is null)
        {
            throw new RuntimeErrorException(name, $"Undefined property {name.Lexeme}");
        }

        if (name.Lexeme == "init")
        {
            throw new RuntimeErrorException(name, "Can not directly call initializer on instance");
        }
        
        return method.Bind(this);
    }

    public void Set(Token name, object value)
    {
        _fields[name.Lexeme] = value;
    }
    
    public override string ToString()
        => $"Instance of: {_loxClass.Name}";
}