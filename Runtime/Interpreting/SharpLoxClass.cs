using System.Collections.Generic;

namespace Runtime.Interpreting;

public class SharpLoxClass : ICallable
{
    public string Name { get; set; }
    
    public int Arity => 0;

    public SharpLoxClass(string name)
    {
        Name = name;
    }

    public override string ToString() 
        => Name;

    public object Call(Interpreter interpreter, IEnumerable<object> arguments)
    {
        return new SharpLoxInstance(this);
    }
}