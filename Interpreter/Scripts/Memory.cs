using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Scripts
{
    public static class LocalMemory
    {
        public static Dictionary<string, object> _variables;
        public static Dictionary<string, int> _labels; // Maps label name to statement index
    
        public static void InicializeMemory()
        {
            _variables = new Dictionary<string, object>();
            _labels = new Dictionary<string, int>();
        }
    }
}
