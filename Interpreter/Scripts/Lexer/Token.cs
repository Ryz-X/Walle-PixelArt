using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lexer
{
    public enum TokenType
    {
        Keyword, Identifier, Number, String, Boolean, Operator, Left_Parentesis, Right_Parentesis, Left_Clasp, Right_Clasp, Coma, NewLine, EOF
    }

    public enum Keys
    {
        Spawn, ReSpawn, Color, Size, DrawLine, DrawCircle, DrawRectangle, Fill, GoTo
    }

    public enum Funtions
    {
        GetActualX, GetActualY, GetCanvasSize, GetColorCount, IsBrushColor, IsBrushSize, IsCanvasColor, IsColor
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public Localization Codelocation { get; }
        public Token(TokenType type, string value, Localization codelocation)
        {
            Type = type;
            Value = value;
            Codelocation = codelocation;
        }
    }

    public struct Localization
    {
        public int line, column;
        public string? file;

        public Localization(int line, int column, string? file = null)
        {
            this.line = line;
            this.column = column;
            this.file = file;
        }
    }
}
