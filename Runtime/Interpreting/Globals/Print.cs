using System;
using System.Collections.Generic;
using System.Linq;

namespace Runtime.Interpreting.Globals
{
    public class Print : ICallable
    {
        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            Console.WriteLine(arguments.FirstOrDefault()?.ToString() ?? "Nil");
            return null!;
        }

        public int Arity => 1;
    }
}