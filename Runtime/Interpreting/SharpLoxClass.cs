using System.Collections.Generic;
using Runtime.Lexing;
using SSEHub.Core.Extensions;

namespace Runtime.Interpreting;

public class SharpLoxClass : ICallable
{
    public string Name { get; set; }

    public int Arity => _methods.TryGetValue("init", out var init) ? init.Arity : 0;

    private readonly Dictionary<string, SharpLoxCallable> _methods;

    public SharpLoxClass(string name, Dictionary<string, SharpLoxCallable> methods)
    {
        Name = name;
        _methods = methods;
    }

    public SharpLoxCallable? GetMethod(Token name)
    {
        return _methods.GetOrDefault(name.Lexeme);
    }
    
    public override string ToString() 
        => Name;

    public object Call(Interpreter interpreter, IEnumerable<object> arguments)
    {
        var instance = new SharpLoxInstance(this);
        if (_methods.TryGetValue("init", out var init))
        {
            init.Bind(instance).Call(interpreter, arguments);
        }
        return instance;
    }
}