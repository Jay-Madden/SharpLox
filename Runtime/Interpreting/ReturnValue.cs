using System;
using Runtime.Parsing.Productions;

namespace Runtime.Interpreting
{
    public class ReturnValue: Exception 
    {
        public object Value { get; }
        
        public ReturnValue(object val)
        {
            Value = val;
        }
        
    }
}