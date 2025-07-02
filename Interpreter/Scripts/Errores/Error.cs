using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter.Lexer;

namespace Interpreter.Errores
{
    public class CompilingError
    {
        public ErrorCode Code { get; private set; }

        public string Argument { get; private set; }

        public int Location { get; private set; }

        public CompilingError(int location, ErrorCode code, string argument)
        {
            this.Code = code;
            this.Argument = argument;
            Location = location;
        }
    }

    public enum ErrorCode
    {
        Unexpected,
        Semantic,
        Sintactic,
        ExecusionTime,
    }

    public static class ErrorList
    {
        static public List<CompilingError> errors;

        public static void Add(CompilingError error) { errors.Add(error); }

        public static void Clear() { errors.Clear(); }
    }

}
