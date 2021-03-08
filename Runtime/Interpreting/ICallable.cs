using System.Collections.Generic;

namespace Runtime.Interpreting
{
    public interface ICallable
    {
        public int Arity { get;  }
        public object Call(Interpreter interpreter, IEnumerable<object> arguments);
    }
}