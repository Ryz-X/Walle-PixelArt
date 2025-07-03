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
using Interpreter.Scripts;

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
        private DateTime Time1;
        private int MaxTime = 30;

        public Interpreter(ProgramNode program, int initialCanvasSize)
        {
            _program = program;
            _canvasWidth = initialCanvasSize;
            _canvasHeight = initialCanvasSize;
            _canvas = new Color[_canvasWidth, _canvasHeight];
            ClearCanvas();
            LocalMemory.InicializeMemory();
            _wallEX = 0;
            _wallEY = 0;
            _brushColor = Color.Transparent; // Default color 
            _brushSize = 1; // Default size
            PreprocessLabels(); // Populate _labels dictionary
            Time1 = DateTime.Now;
        }

        private void PreprocessLabels()
        {
            for (int i = 0; i < _program.Statements.Count; i++)
            {
                if (_program.Statements[i] is LabelStatement labelNode)
                {
                    if (LocalMemory._labels.ContainsKey(labelNode.LabelName))
                    {
                        ErrorList.errors.Add(new CompilingError( labelNode.Line, ErrorCode.ExecusionTime, $"Duplicate label {labelNode.LabelName}at line:"));
                    }
                    else
                    {
                        LocalMemory._labels[labelNode.LabelName] = i;
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
                TimeSpan lapse = DateTime.Now - Time1;
                if (lapse.Seconds >= MaxTime)
                {
                    ErrorList.errors.Add(new CompilingError(lapse.Seconds, ErrorCode.ExecusionTime, $"Time is too long, unsuported management, time in seconds: "));
                    break;
                }

                AstNode statement = _program.Statements[i];

                if (!spawnFound)
                {
                    if (statement is SpawnCommand spawnCmd)
                    {
                        int x = 0;
                        int y = 0;
                        bool convert = false;

                        try
                        {
                            x = (int)EvaluateExpression(spawnCmd.X, spawnCmd.Line);
                            y = (int)EvaluateExpression(spawnCmd.Y, spawnCmd.Line);
                            convert = true;
                        }
                        catch
                        {
                            ErrorList.errors.Add(new CompilingError( spawnCmd.Line, ErrorCode.ExecusionTime, $"Parameters in Spawn are not corrects at line:"));
                        }

                        if (convert)
                        {
                            if (x < 0 || x >= _canvasWidth || y < 0 || y >= _canvasHeight)
                            {
                                ErrorList.errors.Add(new CompilingError( spawnCmd.Line, ErrorCode.Unexpected, $"Initial Wall-E position ({x},{y}) is out of canvas bounds at line:"));
                            }
                            else
                            {
                                _wallEX = x;
                                _wallEY = y;
                                spawnFound = true;
                            }
                        }
                    }
                    else
                    {
                        ErrorList.errors.Add(new CompilingError( statement.Line, ErrorCode.Unexpected, "Every valid code must start with a Spawn command. line:"));
                    }
                }
                else
                {
                    if (statement is SpawnCommand spawnCmd)
                    {
                        ErrorList.errors.Add(new CompilingError(spawnCmd.Line, ErrorCode.ExecusionTime, $"Is not possible re-initialize Wall-E with Spawn. line:"));
                    }
                    else if (statement is ReSpawnCommand respawnCmd)
                    {
                        int x = 0;
                        int y = 0;
                        bool convert = false;

                        try
                        {
                            x = (int)EvaluateExpression(respawnCmd.X, respawnCmd.Line);
                            y = (int)EvaluateExpression(respawnCmd.Y, respawnCmd.Line);
                            convert = true;
                        }
                        catch
                        {
                            ErrorList.errors.Add(new CompilingError( respawnCmd.Line, ErrorCode.ExecusionTime, $"Parameters in ReSpawn are not corrects at line:"));
                        }

                        if (convert)
                        {
                            if (x < 0 || x >= _canvasWidth || y < 0 || y >= _canvasHeight)
                            {
                                ErrorList.errors.Add(new CompilingError( respawnCmd.Line, ErrorCode.Unexpected, $"Wall-E position ({x},{y}) is out of canvas bounds. line:"));
                            }
                            else
                            {
                                _wallEX = x;
                                _wallEY = y;
                            }
                        }
                    }
                    else if (statement is ColorCommand colorCmd)
                    {
                        string colorName = "";
                        bool convert = false;
                        try
                        {
                            colorName = (string)EvaluateExpression(colorCmd.Color, colorCmd.Line);
                            convert = true;
                        }
                        catch
                        {
                            ErrorList.errors.Add(new CompilingError( colorCmd.Line, ErrorCode.ExecusionTime, $"Parameters in Color are not corrects. line:"));
                        }
                        if (convert && colorName != null) { _brushColor = ParseColor(colorName, colorCmd.Line); }
                    }
                    else if (statement is SizeCommand sizeCmd)
                    {

                        int k = 0;
                        bool convert = false;
                        try
                        {
                            k = (int)EvaluateExpression(sizeCmd.Size, sizeCmd.Line);
                            convert = true;
                        }
                        catch
                        {
                            ErrorList.errors.Add(new CompilingError( sizeCmd.Line, ErrorCode.ExecusionTime, $"Parameters in Size are not corrects. line:"));
                        }

                        if (convert)
                        {
                            if (k <= 0)
                            {
                                ErrorList.errors.Add(new CompilingError( sizeCmd.Line, ErrorCode.Unexpected, $"Brush size must be positive. Received {k}. line:"));
                            }
                            else
                            {
                                _brushSize = k % 2 == 0 ? k - 1 : k;
                            }
                        }
                    }
                    else if (statement is DrawLineCommand drawLineCmd)
                    {
                        int dirX = 0;
                        int dirY = 0;
                        int distance = 0;
                        bool convert = false;

                        try
                        {
                            dirX = (int)EvaluateExpression(drawLineCmd.DirX, drawLineCmd.Line);
                            dirY = (int)EvaluateExpression(drawLineCmd.DirY, drawLineCmd.Line);
                            distance = (int)EvaluateExpression(drawLineCmd.Distance, drawLineCmd.Line);
                            convert = true;

                            if (dirX > 1 || dirY > 1 || dirX < -1 || dirY < -1)
                            {
                                ErrorList.errors.Add(new CompilingError( drawLineCmd.Line, ErrorCode.Unexpected, $"Parameters 'directions' in DrawLine are not correct. line:"));
                                convert = false;
                            }
                            if (distance < 0)
                            {
                                ErrorList.errors.Add(new CompilingError( drawLineCmd.Line, ErrorCode.Unexpected, $"Parameter 'distance' in DrawLine can not be negative. line:"));
                                convert = false;
                            }
                        }
                        catch
                        {
                            ErrorList.errors.Add(new CompilingError( drawLineCmd.Line, ErrorCode.ExecusionTime, $"Parameters in DrawLine are not correct. line:"));
                        }

                        if (convert) { DrawLine(_wallEX, _wallEY, dirX, dirY, distance); }
                    }
                    else if (statement is DrawCircleCommand drawCircleCmd)
                    {
                        int dirX = 0;
                        int dirY = 0;
                        int radius = 0;
                        bool convert = false;

                        try
                        {
                            dirX = (int)EvaluateExpression(drawCircleCmd.DirX, drawCircleCmd.Line);
                            dirY = (int)EvaluateExpression(drawCircleCmd.DirY, drawCircleCmd.Line);
                            radius = (int)EvaluateExpression(drawCircleCmd.Radius, drawCircleCmd.Line);
                            convert = true;

                            if (dirX > 1 || dirY > 1 || dirX < -1 || dirY < -1)
                            {
                                ErrorList.errors.Add(new CompilingError( drawCircleCmd.Line, ErrorCode.Unexpected, $"Parameters 'directions' in DrawCircle are not correct. line:"));
                                convert = false;
                            }

                            if (radius < 0)
                            {
                                ErrorList.errors.Add(new CompilingError( drawCircleCmd.Line, ErrorCode.Unexpected, $"Parameter 'radius' in DrawCircle can not be negative. line:"));
                                convert = false;
                            }
                        }
                        catch
                        {
                            ErrorList.errors.Add(new CompilingError(drawCircleCmd.Line, ErrorCode.ExecusionTime, $"Parameters in DrawCircle are not corrects. line:"));
                        }

                        if (convert) { DrawCircle(dirX, dirY, radius); }
                    }
                    else if (statement is DrawRectangleCommand drawRectangleCmd)
                    {
                        int dirX = 0;
                        int dirY = 0;
                        int distance = 0;
                        int width = 0;
                        int height = 0;
                        bool convert = true;

                        try
                        {
                            dirX = (int)EvaluateExpression(drawRectangleCmd.DirX, drawRectangleCmd.Line);
                            dirY = (int)EvaluateExpression(drawRectangleCmd.DirY, drawRectangleCmd.Line);
                            distance = (int)EvaluateExpression(drawRectangleCmd.Distance, drawRectangleCmd.Line);
                            width = (int)EvaluateExpression(drawRectangleCmd.Width, drawRectangleCmd.Line);
                            height = (int)EvaluateExpression(drawRectangleCmd.Height, drawRectangleCmd.Line);
                            convert = true;

                            if (dirX > 1 || dirY > 1 || dirX < -1 || dirY < -1)
                            {
                                ErrorList.errors.Add(new CompilingError( drawRectangleCmd.Line, ErrorCode.Unexpected, $"Parameters 'directions' in DrawRectangle are not correct. line:"));
                                convert = false;
                            }

                            if (distance < 0)
                            {
                                ErrorList.errors.Add(new CompilingError( drawRectangleCmd.Line, ErrorCode.Unexpected, $"Parameter 'distance' in DrawRectangle can not be negative. line"));
                                convert = false;
                            }

                            if (width < 0 || height < 0)
                            {
                                ErrorList.errors.Add(new CompilingError( drawRectangleCmd.Line, ErrorCode.Unexpected, $"Parameters 'width' or/and 'height' in DrawRectangle can not be negative. line:"));
                                convert = false;
                            }
                        }
                        catch
                        {
                            ErrorList.errors.Add(new CompilingError( drawRectangleCmd.Line, ErrorCode.ExecusionTime, $"Parameters in DrawRectangle are not corrects. line:"));
                        }

                        if (convert) { DrawRectangle(dirX, dirY, distance, width, height); }
                    }
                    else if (statement is FillCommand fillCmd)
                    {
                        Fill();
                    }
                    else if (statement is AssignmentStatement assignStmt)
                    {
                        object value = EvaluateExpression(assignStmt.Expression, assignStmt.Line);
                        bool convert = false;
                        if (value != null)
                        {
                            convert = true;
                        }
                        else
                        {
                            ErrorList.errors.Add(new CompilingError( assignStmt.Line, ErrorCode.ExecusionTime, $"Assigment Expression is not corrects. line:"));
                        }
                        if (convert) { LocalMemory._variables[assignStmt.VariableName] = value; }
                    }
                    else if (statement is GoToStatement gotoStmt)
                    {
                        if (!LocalMemory._labels.ContainsKey(gotoStmt.LabelName))
                        {
                            ErrorList.errors.Add(new CompilingError( gotoStmt.Line, ErrorCode.Unexpected, $"Label '{gotoStmt.LabelName}' not found. line:"));
                        }
                        else
                        {
                            bool conditionResult = (bool)EvaluateExpression(gotoStmt.Condition, gotoStmt.Line);
                            if (conditionResult)
                            {
                                i = LocalMemory._labels[gotoStmt.LabelName]; // Jump to label
                            }
                        }
                    }
                    else if (statement is LabelStatement) { }
                    else
                    {
                        ErrorList.errors.Add(new CompilingError(i, ErrorCode.Unexpected, $"Unknown statement type: {statement.GetType().Name}. line:"));
                    }
                }
            }
        }

        private object EvaluateExpression(AstNode expression, int line)
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
                if (LocalMemory._variables.TryGetValue(varExpr.VariableName, out object value))
                {
                    return value;
                }
                ErrorList.errors.Add(new CompilingError( varExpr.Line, ErrorCode.ExecusionTime, $"Variable '{varExpr.VariableName}' not defined. line:"));
                return null;
            }
            else if (expression is BinaryOp binaryOp)
            {
                object left = EvaluateExpression(binaryOp.Left, binaryOp.Line);
                object right = EvaluateExpression(binaryOp.Right, binaryOp.Line);

                if (left == null || right == null) { return null; }

                bool CheckSemantic = binaryOp.CheckSemantic();

                if (CheckSemantic)
                {
                    switch (binaryOp.Operator)
                    {
                        case "+": return (dynamic)left + (dynamic)right;
                        case "-": return (dynamic)left - (dynamic)right;
                        case "*": return (int)((dynamic)left * (dynamic)right);
                        case "/":
                            if ((dynamic)right == 0)
                            {
                                ErrorList.errors.Add(new CompilingError( binaryOp.Line, ErrorCode.ExecusionTime, $"Division by zero. line:"));
                            }
                            else
                            {
                                return (int)((dynamic)left / (dynamic)right);
                            }
                            break;

                        case "**": return (int)(Math.Pow((dynamic)left, (dynamic)right));
                        case "==": return Equals(left, right);
                        case ">=": return (dynamic)left >= (dynamic)right;
                        case "<=": return (dynamic)left <= (dynamic)right;
                        case ">": return (dynamic)left > (dynamic)right;
                        case "<": return (dynamic)left < (dynamic)right;
                        case "&&": return (bool)left && (bool)right;
                        case "||": return (bool)left || (bool)right;

                        default:
                            ErrorList.errors.Add(new CompilingError( binaryOp.Line, ErrorCode.Unexpected, $"Unknown operator: {binaryOp.Operator}. line:"));
                            return null;
                    }

                }
                else
                {
                    ErrorList.errors.Add(new CompilingError( binaryOp.Line, ErrorCode.Semantic, $"Invalid operation: {left} {binaryOp.Operator} {right} at line:"));
                    return null;
                }

            }
            else if (expression is UnaryOp unaryop)
            {
                object right = EvaluateExpression(unaryop.Right, line);

                if (right == null) { return null; }

                bool CheckSemantic = unaryop.CheckSemantic();

                if (CheckSemantic)
                {
                    switch (unaryop.Operator)
                    {
                        case "~": return -(dynamic)right;
                        case "!": return !(bool)right;
                        default:
                            ErrorList.errors.Add(new CompilingError(unaryop.Line, ErrorCode.Unexpected, $"Unknown operation: {unaryop.Operator} at line:"));
                            break;
                    }
                }
                else
                {
                    ErrorList.errors.Add(new CompilingError(unaryop.Line, ErrorCode.Semantic, $"Invalid operation: {unaryop.Operator} {right} at line:"));
                    return null;
                }
            }
            else if (expression is FunctionCallExpression funcCall)
            {
                List<object> evaluatedArgs = funcCall.Arguments.Select(arg => EvaluateExpression(arg, line)).ToList();
                return CallFunction(funcCall.FunctionName, evaluatedArgs, funcCall.Line);
            }
            else
            {
                ErrorList.errors.Add(new CompilingError( line, ErrorCode.Unexpected, $"Unsupported expression type: {expression.GetType().Name}. line:"));
            }
            return null;
        }

        private object CallFunction(string functionName, List<object> args, int Line)
        {
            switch (functionName)
            {
                case "GetActualX": return _wallEX;
                case "GetActualY": return _wallEY;
                case "GetCanvasSize": return _canvasWidth;
                
                case "GetColorCount":
                    if (args.Count != 5 || !(args[0] is string colorName) || !(args[1] is int x1) || !(args[2] is int y1) || !(args[3] is int x2) || !(args[4] is int y2))
                    {
                        ErrorList.errors.Add(new CompilingError( Line, ErrorCode.Unexpected, "Invalid arguments for GetColorCount. line:"));
                    }
                    else
                    {
                        return GetColorCount(colorName, x1, y1, x2, y2, Line);
                    } break;

                case "IsBrushColor":
                    if (args.Count != 1 || !(args[0] is string colorStr))
                    {
                        ErrorList.errors.Add(new CompilingError( Line, ErrorCode.Unexpected, "Invalid arguments for IsBrushColor. line:"));
                    }
                    else
                    {
                        return _brushColor == ParseColor(colorStr, Line)? 1 : 0;
                    } break;
                        
                case "IsBrushSize":
                    if (args.Count != 1 || !(args[0] is int size))
                    {
                        ErrorList.errors.Add(new CompilingError(Line, ErrorCode.Unexpected, "Invalid arguments for IsBrushSize. line:"));
                    }
                    else
                    {
                        return _brushSize == size? 1 : 0;
                    } break;
                        
                case "IsCanvasColor":
                    if (args.Count != 3 || !(args[0] is string colorString) || !(args[1] is int vertical) || !(args[2] is int horizontal))
                    {
                        ErrorList.errors.Add(new CompilingError(Line, ErrorCode.Unexpected, "Invalid arguments for IsCanvasColor. line:"));
                    }
                    else
                    {
                        return IsCanvasColor(colorString, vertical, horizontal, Line);
                    } break;
                        
                default:
                    ErrorList.errors.Add(new CompilingError(Line, ErrorCode.Unexpected, $"Unknown function: {functionName}. line:"));
                    break;
            }
            return null;
        }

        private Color ParseColor(string colorName, int Line)
        {
            switch (colorName.ToLower())
            {
                case "red": return Color.Red;
                case "blue": return Color.Blue;
                case "green": return Color.Green;
                case "yellow": return Color.Yellow;
                case "orange": return Color.Orange;
                case "purple": return Color.Purple;
                case "pink": return Color.Pink;
                case "cyan": return Color.Cyan;
                case "maroon": return Color.Maroon;
                case "crimson": return Color.Crimson;
                case "darkblue": return Color.DarkBlue;
                case "gold": return Color.Gold;
                case "black": return Color.Black;
                case "gray": return Color.Gray;
                case "white": return Color.White;
                case "transparent": return Color.Transparent;
                default:
                    ErrorList.errors.Add(new CompilingError(Line, ErrorCode.Semantic, $"Unknown color '{colorName}', Transparent set as default. line:"));
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
                if (currentX + dirX >= 0 && currentY + dirY >= 0 &&
                    currentX + dirX < _canvasWidth && currentY + dirY < _canvasHeight)
                {
                    currentX += dirX;
                    currentY += dirY;
                }
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

            int LeftMidWidth = (width - 1) / 2;
            int LeftMidHeight = (height - 1) / 2;
            int RightMidWidth = (width - 1) / 2;
            int RightMidHeight = (height - 1) / 2;

            if (width % 2 == 0) { RightMidWidth++; }
            if (height % 2 == 0) { RightMidHeight++; }

            for (int i = -LeftMidWidth; i <= RightMidWidth; i++)
            {
                for(int j = -LeftMidHeight; j <= RightMidHeight; j++)
                {
                    if(i == -LeftMidWidth || j == -LeftMidHeight || i == RightMidWidth || j == RightMidHeight)
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

        private int GetColorCount(string colorName, int x1, int y1, int x2, int y2, int Line)
        {
            Color targetColor = ParseColor(colorName, Line);
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

        private int IsCanvasColor(string colorName, int vertical, int horizontal, int Line)
        {
            Color targetColor = ParseColor(colorName, Line);
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
            int[,] map = Map(remplacecolor);

            while (count == p)
            {
                p = Lee(p, map, targetColor);
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
            _canvas[_wallEX, _wallEY] = _brushColor;
            return map;
        }

        private int Lee(int p, int[,]map, Color color)
        {
            int index = int.MinValue;

            int[,] dir =
            {
                { 0, 1, 0, -1 }, //x
                { 1, 0, -1, 0 }  //y
            };

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == p)
                    {
                        for(int k = 0; k < dir.GetLength(1); k++)
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
