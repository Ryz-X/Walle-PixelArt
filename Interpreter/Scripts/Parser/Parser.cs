using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter.ASTNodes;
using Interpreter.Errores;
using Interpreter.Lexer;

namespace Interpreter.Parser
{
    public class Parser
    {
        private IEnumerator<Token> _tokens;
        private Token _currentToken;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens.GetEnumerator();
            _tokens.MoveNext(); // Initialize _currentToken
            _currentToken = _tokens.Current;
        }

        private bool Eat(TokenType expectedType)
        {
            if (_currentToken.Type == expectedType)
            {
                _tokens.MoveNext();
                _currentToken = _tokens.Current;
                return true;
            }
            else
            {
                ErrorList.errors.Add(new CompilingError( _currentToken.Line, ErrorCode.Unexpected, "Unexpected TokenType at line:"));
            }
            return false;
        }

        public ProgramNode Parse()
        {
            ProgramNode program = new ProgramNode();
            while (_currentToken.Type != TokenType.EOF)
            {
                // Implement parsing logic for different statements (commands, assignments, labels, GoTo)
                // Example:
                if (_currentToken.Type == TokenType.Keyword)
                {
                    if (_currentToken.Value == Keys.Spawn.ToString())
                    {
                        // Parse Spawn command
                        Eat(TokenType.Keyword); // Eat "Spawn"

                        bool a = Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression x = ParseExpression();
                        bool b = Eat(TokenType.Coma); // Eat ","
                        Expression y = ParseExpression();
                        bool c = Eat(TokenType.Right_Parentesis); // Eat ")"

                        if (a && b && c && x != null && y != null)
                        {
                            program.Statements.Add(new SpawnCommand(x, y, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError( _currentToken.Line, ErrorCode.Unexpected, "Invalid call of Spawn at line:"));
                        }
                            
                    }
                    else if (_currentToken.Value == Keys.ReSpawn.ToString())
                    {
                        Eat(TokenType.Keyword);

                        bool a = Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression x = ParseExpression();
                        bool b = Eat(TokenType.Coma); // Eat ","
                        Expression y = ParseExpression();
                        bool c = Eat(TokenType.Right_Parentesis); // Eat ")"

                        if (a && b && c && x != null && y != null)
                        {
                            program.Statements.Add(new ReSpawnCommand(x, y, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError( _currentToken.Line, ErrorCode.Unexpected, "Invalid call of ReSpawn at line:"));
                        }
                    }
                    else if (_currentToken.Value == Keys.Color.ToString())
                    {
                        // Parse Color command
                        Eat(TokenType.Keyword); // Eat "Color"
                        bool a = Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression color = ParseExpression();
                        bool b = Eat(TokenType.Right_Parentesis); // Eat ")"
                        if (a && b && color != null)
                        {
                            program.Statements.Add(new ColorCommand(color, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, "Invalid call of Color at line:"));
                        }
                    }
                    else if (_currentToken.Value == Keys.Size.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "Size"
                        bool a = Eat(TokenType.Left_Parentesis); // Eat "(
                        Expression size = ParseExpression();
                        bool b = Eat(TokenType.Right_Parentesis); // Eat ")"
                        if (a && b && size != null)
                        {
                            program.Statements.Add(new SizeCommand(size, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, "Invalid call of Size at line:"));
                        }
                    }
                    else if (_currentToken.Value == Keys.DrawLine.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "DrawLine"
                        bool a = Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression dirx = ParseExpression();
                        bool b = Eat(TokenType.Coma); // Eat ","
                        Expression diry = ParseExpression();
                        bool c = Eat(TokenType.Coma); // Eat ","
                        Expression distance = ParseExpression();
                        bool d = Eat(TokenType.Right_Parentesis); // Eat ")"
                        if (a && b && c && d && dirx != null && diry != null && distance != null)
                        {
                            program.Statements.Add(new DrawLineCommand(dirx, diry, distance, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, "Invalid call of Drawline at line:"));
                        }
                    }
                    else if (_currentToken.Value == Keys.DrawCircle.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "DrawCircle"
                        bool a = Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression dirx = ParseExpression();
                        bool b = Eat(TokenType.Coma); // Eat ","
                        Expression diry = ParseExpression();
                        bool c = Eat(TokenType.Coma); // Eat ","
                        Expression radius = ParseExpression();
                        bool d = Eat(TokenType.Right_Parentesis); // Eat ")"
                        if (a && b && c && d && dirx != null && diry != null && radius != null)
                        {
                            program.Statements.Add(new DrawCircleCommand(dirx, diry, radius, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, "Invalid call of DrawCircle at line:"));
                        }
                    }
                    else if (_currentToken.Value == Keys.DrawRectangle.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "DrawRectangle"
                        bool a = Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression dirx = ParseExpression();
                        bool b = Eat(TokenType.Coma); // Eat ","
                        Expression diry = ParseExpression();
                        bool c = Eat(TokenType.Coma); // Eat ","
                        Expression distance = ParseExpression();
                        bool d = Eat(TokenType.Coma); // Eat ","
                        Expression width = ParseExpression();
                        bool e = Eat(TokenType.Coma); // Eat ","
                        Expression height = ParseExpression();
                        bool f = Eat(TokenType.Right_Parentesis); // Eat ")"
                        if (a && b && c && d && e && f && dirx != null && diry != null && distance != null && width != null && height != null)
                        {
                            program.Statements.Add(new DrawRectangleCommand(dirx, diry, distance, width, height, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, "Invalid call of DrawRectangle at line:"));
                        }
                    }
                    else if (_currentToken.Value == Keys.Fill.ToString())
                    {
                        Eat(TokenType.Keyword);
                        bool a = Eat(TokenType.Left_Parentesis);
                        bool b = Eat(TokenType.Right_Parentesis);
                        if (a && b)
                        {
                            program.Statements.Add(new FillCommand(_currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, "Invalid call of Fill at line:"));
                        }
                    }
                    else if (_currentToken.Value == Keys.GoTo.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "Goto"
                        bool a = Eat(TokenType.Left_Clasp); // Eat "["
                        string labelname = _currentToken.Value;
                        bool b = Eat(TokenType.Identifier);  // Eat label
                        bool c = Eat(TokenType.Right_Clasp); // Eat "]"
                        bool d = Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression condition = ParseExpression();
                        bool e = Eat(TokenType.Right_Parentesis); // Eat ")"
                        if (a && b && c && d && e)
                        {
                            program.Statements.Add(new GoToStatement(labelname, condition, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, "Invalid call of GoTo at line:"));
                        }
                    }
                    else
                    {
                        ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, "Keyword not defined at line:"));
                    }
                
                }
                else if (_currentToken.Type == TokenType.Identifier)
                {
                    // Could be a variable assignment or a label
                    string identifier = _currentToken.Value;
                    _tokens.MoveNext(); // Move past identifier
                    _currentToken = _tokens.Current;

                    if (_currentToken.Type == TokenType.Operator && (_currentToken.Value == "<-" || _currentToken.Value == "="))
                    {
                        // It's an assignment
                        Eat(TokenType.Operator); // Eat "<-"
                        Expression expr = ParseExpression();
                        if (expr != null)
                        {
                            program.Statements.Add(new AssignmentStatement(identifier, expr, _currentToken.Line));
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.ExecusionTime, "No value is asignate. line:"));
                        }
                    }
                    else if (_currentToken.Type == TokenType.NewLine || _currentToken.Type == TokenType.EOF)
                    {
                        // It's a label
                        program.Statements.Add(new LabelStatement(identifier, _currentToken.Line));
                    }
                    else
                    {
                        ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, $"Unexpected token {_currentToken.Value} after identifier {identifier} at line:"));
                    }
                }
                else if (_currentToken.Type == TokenType.NewLine)
                {
                    Eat(TokenType.NewLine);
                }
                else if (_currentToken.Type == TokenType.Right_Parentesis || _currentToken.Type == TokenType.Right_Clasp)
                {
                    ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, $"Unexpected token {_currentToken.Value} at line:"));
                    Eat(_currentToken.Type);
                }
                else
                {
                    ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, $"Unexpected token {_currentToken.Value} at line:"));
                    Eat(_currentToken.Type);
                }

            }
            return program;
        }

        private Expression ParseExpression()
        {
            // Implement operator precedence for arithmetic and boolean expressions
            // This is a simplified example, typically involves multiple methods for different precedence levels
            Expression expr = ParseComparison();

            if (_currentToken.Type == TokenType.Number || _currentToken.Type == TokenType.String || _currentToken.Type == TokenType.Boolean)
            {
                ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, $"Unexpected Token {_currentToken.Value} at line:"));
            }

            return expr; // Start with lowest precedence
        }

        private Expression ParseComparison()
        {
            Expression expr = ParseComparison1();

            while (_currentToken.Type == TokenType.Operator && (
                   _currentToken.Value == "==" || _currentToken.Value == "!=" ))
            {
                string op = _currentToken.Value;
                Eat(TokenType.Operator);
                Expression right = ParseComparison1();
                expr = new BinEqualOpExpression(expr, op, right, _currentToken.Line);
            }
            return expr;
        }

        private Expression ParseComparison1()
        {
            Expression expr = ParseAdditive();

            while (_currentToken.Type == TokenType.Operator && (
                   _currentToken.Value == ">=" || _currentToken.Value == "<=" ||
                   _currentToken.Value == ">" || _currentToken.Value == "<"))
            {
                string op = _currentToken.Value;
                Eat(TokenType.Operator);
                Expression right = ParseAdditive();
                expr = new BinCompExpression(expr, op, right, _currentToken.Line);
            }
            return expr;
        }

        private Expression ParseAdditive()
        {
            Expression expr = ParseMultiplicative();
            while (_currentToken.Type == TokenType.Operator && (_currentToken.Value == "+" || _currentToken.Value == "-"))
            {
                string op = _currentToken.Value;
                Eat(TokenType.Operator);
                Expression right = ParseMultiplicative();
                expr = new BinAritExpression(expr, op, right, _currentToken.Line);
            }
            return expr;
        }

        private Expression ParseMultiplicative()
        {
            Expression expr = ParseMultiplicative2();
            while (_currentToken.Type == TokenType.Operator && (_currentToken.Value == "*" || _currentToken.Value == "/" || _currentToken.Value == "%"))
            {
                string op = _currentToken.Value;
                Eat(TokenType.Operator);
                Expression right = ParseMultiplicative2();
                expr = new BinAritExpression(expr, op, right, _currentToken.Line);
            }
            return expr;
        }

        private Expression ParseMultiplicative2()
        {
            Expression expr = ParseNegative();
            while (_currentToken.Type == TokenType.Operator && _currentToken.Value == "**")
            {
                string op = _currentToken.Value;
                Eat(TokenType.Operator);
                Expression right = ParseNegative();
                expr = new BinAritExpression(expr, op, right, _currentToken.Line);
            }
            return expr;
        }

        private Expression ParseNegative()
        {
            if (_currentToken.Type == TokenType.Operator && _currentToken.Value == "-")
            {
                Eat(TokenType.Operator);
                Expression expr = ParseOposite();
                return new UnaryAritmeticOpExpression(expr, "~", _currentToken.Line);
            }
            else
            {
                return ParseOposite();
            }
        }

        private Expression ParseOposite()
        {
            if (_currentToken.Type == TokenType.Operator && _currentToken.Value == "!")
            {
                string op = _currentToken.Value;
                Eat(TokenType.Operator);
                Expression expr = ParseParentesis();
                return new UnaryBooleanOpExpression(expr, op, _currentToken.Line);
            }
            else
            {
                return ParseParentesis();
            }
        }

        private Expression ParseParentesis()
        {
            if (_currentToken.Type == TokenType.Left_Parentesis)
            {
                Eat(TokenType.Left_Parentesis);
                Expression expr = ParseComparison();
                Eat(TokenType.Right_Parentesis);
                return expr;
            }
            else
            {
                return ParseClasp();
            }
        }

        private Expression ParseClasp()
        {
            if (_currentToken.Type == TokenType.Left_Clasp)
            {
                Eat(TokenType.Left_Clasp);
                Expression expr = ParseComparison();
                Eat(TokenType.Right_Clasp);
                return expr;
            }
            else
            {
                return ParsePrimary();
            }
        }

        private Expression ParsePrimary()
        {
            if (_currentToken.Type == TokenType.Number)
            {
                int value = int.Parse(_currentToken.Value);
                Eat(TokenType.Number);
                return new LiteralExpression<int>(value, _currentToken.Line);
            }
            else if (_currentToken.Type == TokenType.String)
            {
                string value = _currentToken.Value;
                Eat(TokenType.String);
                return new LiteralExpression<string>(value, _currentToken.Line);
            }
            else if (_currentToken.Type == TokenType.Identifier)
            {

                string identifier = _currentToken.Value;
                Eat(TokenType.Identifier);

                if (_currentToken.Type == TokenType.Left_Parentesis && _currentToken.Value == "(")
                {
                    // Function call
                    Eat(TokenType.Left_Parentesis); // Eat "("
                    List<AstNode> args = new List<AstNode>();

                    if ((_currentToken.Type != TokenType.Right_Parentesis || _currentToken.Value != ")"))
                    {
                        args.Add(ParseExpression());
                        while (_currentToken.Type == TokenType.Coma && _currentToken.Value == ",")
                        {
                            Eat(TokenType.Coma); // Eat ","
                            args.Add(ParseExpression());
                        }
                    }
                    Eat(TokenType.Right_Parentesis); // Eat ")"
                    return new FunctionCallExpression(identifier, args, _currentToken.Line);
                }
                return new VariableExpression(identifier, _currentToken.Line);
            }
            else
            {
                ErrorList.errors.Add(new CompilingError(_currentToken.Line, ErrorCode.Unexpected, $"Unexpected token {_currentToken.Value} expression at line:"));
            }
            return null;
        }

    }
}
