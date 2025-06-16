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

        private void Eat(TokenType expectedType)
        {
            if (_currentToken.Type == expectedType)
            {
                _tokens.MoveNext();
                _currentToken = _tokens.Current;
            }
            else
            {
                ErrorList.errors.Add(new CompilingError(_currentToken.Codelocation, ErrorCode.ExecusionTime, "Unexpected TokenType at line"));
            }
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
                        Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression x = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression y = ParseExpression();
                        Eat(TokenType.Right_Parentesis); // Eat ")"
                        program.Statements.Add(new SpawnCommand(x, y));
                    }
                    else if (_currentToken.Value == Keys.Color.ToString())
                    {
                        // Parse Color command
                        Eat(TokenType.Keyword); // Eat "Color"
                        Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression color = ParseExpression();
                        Eat(TokenType.Right_Parentesis); // Eat ")"
                        program.Statements.Add(new ColorCommand(color));
                    }
                    else if (_currentToken.Value == Keys.Size.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "Size"
                        Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression size = ParseExpression();
                        Eat(TokenType.Right_Parentesis); // Eat ")"
                        program.Statements.Add(new SizeCommand(size));
                    }
                    else if (_currentToken.Value == Keys.DrawLine.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "DrawLine"
                        Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression dirx = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression diry = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression distance = ParseExpression();
                        Eat(TokenType.Right_Parentesis); // Eat ")"
                        program.Statements.Add(new DrawLineCommand(dirx, diry, distance));
                    }
                    else if (_currentToken.Value == Keys.DrawCircle.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "DrawCircle"
                        Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression dirx = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression diry = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression radius = ParseExpression();
                        Eat(TokenType.Right_Parentesis); // Eat ")"
                        program.Statements.Add(new DrawCircleCommand(dirx, diry, radius));
                    }
                    else if (_currentToken.Value == Keys.DrawRectangle.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "DrawRectangle"
                        Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression dirx = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression diry = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression distance = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression width = ParseExpression();
                        Eat(TokenType.Coma); // Eat ","
                        Expression height = ParseExpression();
                        Eat(TokenType.Right_Parentesis); // Eat ")"
                        program.Statements.Add(new DrawRectangleCommand(dirx, diry, distance, width, height));
                    }
                    else if (_currentToken.Value == Keys.Fill.ToString())
                    {
                        Eat(TokenType.Keyword);
                        Eat(TokenType.Left_Parentesis);
                        Eat(TokenType.Right_Parentesis);
                        program.Statements.Add(new FillCommand());
                    }
                    else if (_currentToken.Value == Keys.GoTo.ToString())
                    {
                        Eat(TokenType.Keyword); // Eat "Goto"
                        Eat(TokenType.Left_Clasp); // Eat "["
                        string labelname = _currentToken.Value;
                        Eat(TokenType.Identifier);  // Eat label
                        Eat(TokenType.Right_Clasp); // Eat "]"
                        Eat(TokenType.Left_Parentesis); // Eat "("
                        Expression condition = ParseExpression();
                        Eat(TokenType.Right_Parentesis); // Eat ")"
                        program.Statements.Add(new GoToStatement(labelname, condition));
                    }


                    // ... handle other commands
                
                
                
                
                }
                else if (_currentToken.Type == TokenType.Identifier)
                {
                    // Could be a variable assignment or a label
                    string identifier = _currentToken.Value;
                    _tokens.MoveNext(); // Move past identifier
                    _currentToken = _tokens.Current;

                    if (_currentToken.Type == TokenType.Operator && _currentToken.Value == "<-")
                    {
                        // It's an assignment
                        Eat(TokenType.Operator); // Eat "<-"
                        Expression expr = ParseExpression();
                        program.Statements.Add(new AssignmentStatement(identifier, expr));
                    }
                    else if (_currentToken.Type == TokenType.NewLine || _currentToken.Type == TokenType.EOF)
                    {
                        // It's a label
                        program.Statements.Add(new LabelStatement(identifier));
                    }
                    else
                    {
                        ErrorList.errors.Add(new CompilingError(_currentToken.Codelocation, ErrorCode.Invalid, $"Unexpected token {_currentToken.Value} after identifier {identifier} at line"));
                        Eat(_currentToken.Type);
                    }
                }
                else if (_currentToken.Type == TokenType.NewLine)
                {
                    Eat(TokenType.NewLine);
                }
                else if (_currentToken.Type == TokenType.Right_Parentesis || _currentToken.Type == TokenType.Right_Clasp)
                {
                    ErrorList.errors.Add(new CompilingError(_currentToken.Codelocation, ErrorCode.Invalid, $"Unexpected token {_currentToken.Value} at line"));
                    Eat(_currentToken.Type);
                }
                else
                {
                    ErrorList.errors.Add(new CompilingError(_currentToken.Codelocation, ErrorCode.Invalid, $"Unexpected token {_currentToken.Value} at line"));
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
                ErrorList.errors.Add(new CompilingError(_currentToken.Codelocation, ErrorCode.Invalid, $"Unexpected token {_currentToken.Value} at line"));
            }

            return expr; // Start with lowest precedence
        }

        private Expression ParseComparison()
        {
            Expression expr = ParseAdditive();

            while (_currentToken.Type == TokenType.Operator && (
                   _currentToken.Value == "==" || _currentToken.Value == ">=" ||
                   _currentToken.Value == "<=" || _currentToken.Value == ">" ||
                   _currentToken.Value == "<"))
            {
                string op = _currentToken.Value;
                Eat(TokenType.Operator);
                Expression right = ParseAdditive();
                expr = new BinaryOperationExpression(expr, op, right);
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
                expr = new BinaryOperationExpression(expr, op, right);
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
                expr = new BinaryOperationExpression(expr, op, right);
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
                expr = new BinaryOperationExpression(expr, op, right);
            }
            return expr;
        }

        private Expression ParseNegative()
        {
            if (_currentToken.Type == TokenType.Operator && _currentToken.Value == "-")
            {
                Eat(TokenType.Operator);
                Expression expr = ParseOposite();
                return new UnaryOperationExpression(expr, "~");
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
                return new UnaryOperationExpression(expr, op);
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
                return new LiteralExpression<int>(value);
            }
            else if (_currentToken.Type == TokenType.String)
            {
                string value = _currentToken.Value;
                Eat(TokenType.String);
                return new LiteralExpression<string>(value);
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
                    return new FunctionCallExpression(identifier, args);
                }
                return new VariableExpression(identifier);
            }
            else
            {
                ErrorList.errors.Add(new CompilingError(_currentToken.Codelocation, ErrorCode.Invalid, $"Unexpected token {_currentToken.Value} expression at line"));
            }
            return null;
        }

    }
}
