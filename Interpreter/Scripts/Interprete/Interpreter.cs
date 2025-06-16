using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Interpreter.ASTNodes;
using Interpreter.Errores;

namespace Interpreter.Interprete
{

    
    public class Interpreter
    {
        private ProgramNode _program;
        private int _canvasWidth;
        private int _canvasHeight;
        private Color[,] _canvas;
        private int _wallEX, _wallEY;
        private Color _brushColor;
        private int _brushSize;
        private Dictionary<string, object> _variables;
        private Dictionary<string, int> _labels; // Maps label name to statement index

        public Interpreter(ProgramNode program, int initialCanvasSize)
        {
            _program = program;
            _canvasWidth = initialCanvasSize;
            _canvasHeight = initialCanvasSize;
            _canvas = new Color[_canvasWidth, _canvasHeight];
            ClearCanvas();
            _wallEX = 0;
            _wallEY = 0;
            _brushColor = Color.Transparent; // Default color 
            _brushSize = 1; // Default size 
            _variables = new Dictionary<string, object>();
            _labels = new Dictionary<string, int>();
            PreprocessLabels(); // Populate _labels dictionary
        }

        private void PreprocessLabels()
        {
            for (int i = 0; i < _program.Statements.Count; i++)
            {
                if (_program.Statements[i] is LabelStatement labelNode)
                {
                    if (_labels.ContainsKey(labelNode.LabelName))
                    {
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Semantic Error, Duplicate label {labelNode.LabelName}."));
                    }
                    else
                    {
                        _labels[labelNode.LabelName] = i;
                    }
                }
            }
        }

        private void ClearCanvas()
        {
            for (int x = 0; x < _canvasWidth; x++)
            {
                for (int y = 0; y < _canvasHeight; y++)
                {
                    _canvas[x, y] = Color.White; // Canvas is initially white 
                }
            }
        }

        public void SetCanvasSize(int size)
        {
            _canvasWidth = size;
            _canvasHeight = size;
            _canvas = new Color[_canvasWidth, _canvasHeight];
            ClearCanvas(); // Clear canvas on resize 
        }

        public Color[,] GetCanvasState()
        {
            return _canvas;
        }

        public void Execute()
        {
            bool spawnFound = false;

            for (int i = 0; i < _program.Statements.Count; i++)
            {
                AstNode statement = _program.Statements[i];

                if (!spawnFound)
                {
                    if (statement is SpawnCommand spawnCmd)
                    {
                        int x = (int)EvaluateExpression(spawnCmd.X);
                        int y = (int)EvaluateExpression(spawnCmd.Y);

                        if (x < 0 || x >= _canvasWidth || y < 0 || y >= _canvasHeight)
                        {
                            ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Runtime Error: Initial Wall-E position ({x},{y}) is out of canvas bounds."));
                        }
                        else
                        {
                            _wallEX = x;
                            _wallEY = y;
                            spawnFound = true;
                        }
                    }
                    else
                    {
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, "Semantic Error: Every valid code must start with a Spawn command."));
                    }
                }
                else
                {

                    if (statement is SpawnCommand spawnCmd)
                    {
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Runtime Error: Is not possible re-initialize Wall-E"));
                    }
                    else if (statement is ColorCommand colorCmd)
                    {
                        string colorName = (string)EvaluateExpression(colorCmd.Color);
                        _brushColor = ParseColor(colorName);
                    }
                    else if (statement is SizeCommand sizeCmd)
                    {
                        int k = (int)EvaluateExpression(sizeCmd.Size);
                        if (k <= 0)
                        {
                            ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Brush size must be positive. Received {k}."));
                        }
                        else
                        {
                            _brushSize = k % 2 == 0 ? k - 1 : k;
                        }
                    }
                    else if (statement is DrawLineCommand drawLineCmd)
                    {
                        int dirX = (int)EvaluateExpression(drawLineCmd.DirX);
                        int dirY = (int)EvaluateExpression(drawLineCmd.DirY);
                        int distance = (int)EvaluateExpression(drawLineCmd.Distance);
                        DrawLine(_wallEX, _wallEY, dirX, dirY, distance);
                    }
                    else if (statement is DrawCircleCommand drawCircleCmd)
                    {
                        int dirX = (int)EvaluateExpression(drawCircleCmd.DirX);
                        int dirY = (int)EvaluateExpression(drawCircleCmd.DirY);
                        int radius = (int)EvaluateExpression(drawCircleCmd.Radius);
                        DrawCircle(dirX, dirY, radius);
                    }
                    else if (statement is DrawRectangleCommand drawRectangleCmd)
                    {
                        int dirX = (int)EvaluateExpression(drawRectangleCmd.DirX);
                        int dirY = (int)EvaluateExpression(drawRectangleCmd.DirY);
                        int distance = (int)EvaluateExpression(drawRectangleCmd.Distance);
                        int width = (int)EvaluateExpression(drawRectangleCmd.Width);
                        int height = (int)EvaluateExpression(drawRectangleCmd.Height);
                        DrawRectangle(dirX, dirY, distance, width, height);
                    }
                    else if (statement is FillCommand fillCmd)
                    {
                        Fill();
                    }
                    else if (statement is AssignmentStatement assignStmt)
                    {
                        object value = EvaluateExpression(assignStmt.Expression);
                        _variables[assignStmt.VariableName] = value;
                    }
                    else if (statement is GoToStatement gotoStmt)
                    {
                        if (!_labels.ContainsKey(gotoStmt.LabelName))
                        {
                            ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Compilation Error: Label '{gotoStmt.LabelName}' not found."));
                        }
                        else
                        {
                            bool conditionResult = (bool)EvaluateExpression(gotoStmt.Condition);
                            if (conditionResult)
                            {
                                i = _labels[gotoStmt.LabelName]; // Jump to label
                            }
                        }
                    }
                    else if (statement is LabelStatement) { }
                    else
                    {
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Unknown statement type: {statement.GetType().Name}"));
                    }
                }
            }
        }

        private object EvaluateExpression(AstNode expression)
        {
            if (expression is LiteralExpression<int> intLiteral)
            {
                return intLiteral.Value;
            }
            else if (expression is LiteralExpression<string> stringLiteral)
            {
                return stringLiteral.Value;
            }
            else if (expression is VariableExpression varExpr)
            {
                if (_variables.TryGetValue(varExpr.VariableName, out object value))
                {
                    return value;
                }
                ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Variable '{varExpr.VariableName}' not defined."));
                return null;
            }
            else if (expression is BinaryOperationExpression binaryOp)
            {
                object left = EvaluateExpression(binaryOp.Left);
                object right = EvaluateExpression(binaryOp.Right);

                // Implement arithmetic and boolean operations based on operator and operand types
                switch (binaryOp.Operator)
                {
                    case "+": return (dynamic)left + (dynamic)right;
                    case "-": return (dynamic)left - (dynamic)right;
                    case "*": return (dynamic)left * (dynamic)right;
                    case "/":
                        if ((dynamic)right == 0)
                        {
                            ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Division by zero."));
                        }
                        else
                        {
                            return (dynamic)left / (dynamic)right;
                        } break;

                    case "**": return Math.Pow((dynamic)left, (dynamic)right);
                    case "%": return (dynamic)left % (dynamic)right;
                    case "==": return Equals(left, right);
                    case ">=": return (dynamic)left >= (dynamic)right;
                    case "<=": return (dynamic)left <= (dynamic)right;
                    case ">": return (dynamic)left > (dynamic)right;
                    case "<": return (dynamic)left < (dynamic)right;
                    case "&&": return (bool)left && (bool)right;
                    case "||": return (bool)left || (bool)right;

                    default:
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Unknown operator: {binaryOp.Operator}"));
                        break;
                }
            }else if (expression is UnaryOperationExpression unaryop)
            {
                object right = EvaluateExpression(unaryop.Right);

                switch (unaryop.Operator)
                {
                    case "~": return -(dynamic)right;
                    case "!": return !(bool)right;
                    default :
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Unknown operator: {unaryop.Operator}"));
                        break;
                }
            }
            else if (expression is FunctionCallExpression funcCall)
            {
                List<object> evaluatedArgs = funcCall.Arguments.Select(arg => EvaluateExpression(arg)).ToList();
                return CallFunction(funcCall.FunctionName, evaluatedArgs);
            }
            else
            {
                ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Unsupported expression type: {expression.GetType().Name}"));
            }
            return null;
        }

        private object CallFunction(string functionName, List<object> args)
        {
            switch (functionName)
            {
                case "GetActualX": return _wallEX;
                case "GetActualY": return _wallEY;
                case "GetCanvasSize": return _canvasWidth;
                
                case "GetColorCount":
                    if (args.Count != 5 || !(args[0] is string colorName) || !(args[1] is int x1) || !(args[2] is int y1) || !(args[3] is int x2) || !(args[4] is int y2))
                    {
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, "Invalid arguments for GetColorCount."));
                    }
                    else
                    {
                        return GetColorCount(colorName, x1, y1, x2, y2);
                    } break;

                case "IsBrushColor":
                    if (args.Count != 1 || !(args[0] is string colorStr))
                    {
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, "Invalid arguments for IsBrushColor."));
                    }
                    else
                    {
                        return _brushColor == ParseColor(colorStr)? 1 : 0;
                    } break;
                        
                case "IsBrushSize":
                    if (args.Count != 1 || !(args[0] is int size))
                    {
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, "Invalid arguments for IsBrushSize."));
                    }
                    else
                    {
                        return _brushSize == size? 1 : 0;
                    } break;
                        
                case "IsCanvasColor":
                    if (args.Count != 3 || !(args[0] is string colorString) || !(args[1] is int vertical) || !(args[2] is int horizontal))
                    {
                        ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, "Invalid arguments for IsCanvasColor."));
                    }
                    else
                    {
                        return IsCanvasColor(colorString, vertical, horizontal);
                    } break;
                        
                default:
                    ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Unknown function: {functionName}"));
                    break;
            }
            return null;
        }

        private Color ParseColor(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "red": return Color.Red;
                case "blue": return Color.Blue;
                case "green": return Color.Green;
                case "yellow": return Color.Yellow;
                case "orange": return Color.Orange;
                case "purple": return Color.Purple;
                case "black": return Color.Black;
                case "white": return Color.White;
                case "transparent": return Color.Transparent;
                default:
                    ErrorList.errors.Add(new CompilingError(new Lexer.Localization(), ErrorCode.ExecusionTime, $"Semantic Error: Unknown color '{colorName}'."));
                    break;
            }
            return Color.Transparent;
        }

        private void DrawLine(int startX, int startY, int dirX, int dirY, int distance)
        {
            int currentX = startX;
            int currentY = startY;

            for (int i = 0; i < distance; i++)
            {
                SetPixel(currentX, currentY); // Draw the current pixel with brush size
                currentX += dirX;
                currentY += dirY;
            }
            SetPixel(currentX, currentY); // Draw the last pixel
            _wallEX = currentX;
            _wallEY = currentY;
        }

        private void DrawCircle(int x, int y, int radius)
        {
            for (int i = 0; i < radius; i++)
            {
                if(_wallEX + x >= 0 &&  _wallEY + y >= 0 && _wallEX + x < _canvasWidth && _wallEY + y < _canvasHeight)
                {
                    _wallEX += x;
                    _wallEY += y;
                }
            }

            //Eje x
            for (int i = -radius; i <= radius; i++)
            {

                int pixelX = _wallEX + i;
                int Image = CircleImage(i, radius);

                for (int j = 0; j < 2; j++)
                {
                    int pixelY = (int)Math.Pow(-1, j) * Image + _wallEY;

                    if (pixelX >= 0 && pixelX < _canvasWidth && pixelY >= 0 && pixelY < _canvasHeight)
                    {
                        SetPixel(pixelX, pixelY);
                    }
                }
            }

            //Eje y
            for (int i = -radius; i <= radius; i++)
            {

                int pixelY = _wallEY + i;
                int Image = CircleImage(i, radius);

                for (int j = 0; j < 2; j++)
                {
                    int pixelX = (int)Math.Pow(-1, j) * Image + _wallEX;

                    if (pixelX >= 0 && pixelX < _canvasWidth && pixelY >= 0 && pixelY < _canvasHeight)
                    {
                        SetPixel(pixelX, pixelY);
                    }
                }
            }
        }
        private void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
        {
            for (int i = 0; i < distance; i++)
            {
                if (_wallEX + dirX >= 0 && _wallEY + dirY >= 0 && _wallEX + dirX < _canvasWidth && _wallEY + dirY < _canvasHeight)
                {
                    _wallEX += dirX;
                    _wallEY += dirY;
                }
            }

            int MidWidth = width / 2;
            int MidHeight = height / 2;

            for(int i = -MidWidth-1; i <= MidWidth+1; i++)
            {
                for(int j = -MidHeight-1; j <= MidHeight+1; j++)
                {
                    if(i == -MidWidth - 1 || j == -MidHeight - 1 || i == MidWidth + 1 || j == MidHeight + 1)
                    {
                        int pixelX = _wallEX + i;
                        int pixelY = _wallEY + j;

                        if (pixelX >= 0 && pixelX < _canvasWidth && pixelY >= 0 && pixelY < _canvasHeight)
                        {
                            SetPixel(pixelX, pixelY);
                        }
                    }
                }
            }


        }

        private void SetPixel(int x, int y)
        {
            if (_brushColor == Color.Transparent) return; // Transparent color implies no change 

            // Apply brush size (simple square brush for now)
            int halfBrush = _brushSize / 2;
            for (int dx = -halfBrush; dx <= halfBrush; dx++)
            {
                for (int dy = -halfBrush; dy <= halfBrush; dy++)
                {
                    int pixelX = x + dx;
                    int pixelY = y + dy;

                    if (pixelX >= 0 && pixelX < _canvasWidth && pixelY >= 0 && pixelY < _canvasHeight && Around(x, y, pixelX, pixelY, halfBrush))
                    {
                        _canvas[pixelX, pixelY] = _brushColor;
                    }
                }
            }
        }

        public bool Around(int x, int y, int a, int b, int r) => Math.Pow((x - a), 2) + Math.Pow((y - b), 2) <= Math.Pow(r, 2);
        
        public int CircleImage(int a, int r)
        {
            int Y2 = (int)(Math.Pow(r, 2) - Math.Pow(a, 2));
            Y2 = Math.Abs(Y2);

            return Math.Round(Math.Sqrt(Y2),3) >= (int)Math.Sqrt(Y2) + 0.5? (int)Math.Sqrt(Y2)+1: (int)Math.Sqrt(Y2);
        } 

        private int GetColorCount(string colorName, int x1, int y1, int x2, int y2)
        {
            Color targetColor = ParseColor(colorName);
            int count = 0;

            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);

            if (x1 < 0 || y1 < 0 || x2 < 0 || y2 < 0 ||
                x1 >= _canvasWidth || y1 >= _canvasHeight || x2 >= _canvasWidth || y2 >= _canvasHeight)
            {
                return 0;
            }

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (_canvas[x, y] == targetColor)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private int IsCanvasColor(string colorName, int vertical, int horizontal)
        {
            Color targetColor = ParseColor(colorName);
            int targetX = _wallEX + horizontal;
            int targetY = _wallEY + vertical;

            if (targetX < 0 || targetX >= _canvasWidth || targetY < 0 || targetY >= _canvasHeight)
            {
                return 0;
            }

            return _canvas[targetX, targetY] == targetColor ? 1 : 0;
        }

        private void Fill()
        {
            if (!(_wallEX >= 0 && _wallEX < _canvasWidth && _wallEY >= 0 && _wallEY < _canvasHeight))
            {
                return;
            }

            Color targetColor = _brushColor;
            Color remplacecolor = _canvas[_wallEX, _wallEY];

            int count = 0;
            int p = 0;
            while(count == p)
            {
                p = Lee(p, Map(remplacecolor), targetColor);
                count++;
            }
        }

        private int[,] Map(Color tarjetcolor)
        {
            int[,] map = new int[_canvasWidth, _canvasHeight];

            for (int i = 0; i < _canvasWidth; i++)
            {
                for (int j = 0; j < _canvasHeight; j++)
                {
                    if (_canvas[i, j] == tarjetcolor) { map[i, j] = int.MaxValue; } else { map[i, j] = int.MinValue; }
                }
            }
            map[_wallEX, _wallEY] = 0;
            return map;
        }

        private int Lee(int p, int[,]map, Color color)
        {
            int index = int.MinValue;

            int[,] dir =
            {
                { 0, 1, 1, 1, 0, -1, -1, -1 }, //x
                { 1, 1, 0, -1, -1, -1, 0, 1 }  //y
            };

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == p)
                    {
                        for(int k = 0; k <= 8; k++)
                        {
                            if(i + dir[0,k] >= 0 && i + dir[0,k] < map.GetLength(0) && j + dir[1,k] >= 0 && j + dir[1,k] < map.GetLength(1))
                            {
                                if (map[i + dir[0, k], j + dir[1, k]] > map[i, j])
                                {
                                    map[i + dir[0, k], j + dir[1, k]] = p + 1;
                                    _canvas[i + dir[0, k], j + dir[1, k]] = color;
                                    index = p + 1;
                                }
                            }
                        }
                    }
                }
            }
            return index;
        }

    }
}
