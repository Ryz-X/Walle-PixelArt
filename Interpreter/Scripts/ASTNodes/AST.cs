using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.ASTNodes
{
    public abstract class AstNode { }

    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; set; } = new List<AstNode>();
    }

    public class SpawnCommand : AstNode
    {
        public AstNode X { get; }
        public AstNode Y { get; }
        public SpawnCommand(AstNode x, AstNode y) { X = x; Y = y; }
    }

    public class ColorCommand : AstNode
    {
        public AstNode Color { get; }
        public ColorCommand(AstNode color) { Color = color; }
    }

    public class SizeCommand : AstNode
    {
        public AstNode Size { get; }
        public SizeCommand(AstNode size) { Size = size; }
    }

    public class DrawLineCommand : AstNode
    {
        public AstNode DirX { get; }
        public AstNode DirY { get; }
        public AstNode Distance { get; }
        public DrawLineCommand(AstNode dirX, AstNode dirY, AstNode distance)
        {
            DirX = dirX;
            DirY = dirY;
            Distance = distance;
        }
    }

    public class DrawCircleCommand : AstNode
    {
        public AstNode DirX { get; }
        public AstNode DirY { get; }
        public AstNode Radius { get; }
        public DrawCircleCommand(AstNode dirX, AstNode dirY, AstNode radius)
        {
            DirX = dirX;
            DirY = dirY;
            Radius = radius;
        }
    }

    public class DrawRectangleCommand : AstNode
    {
        public AstNode DirX { get; }
        public AstNode DirY { get; }
        public AstNode Distance { get; }
        public AstNode Width { get; }
        public AstNode Height { get; }
        public DrawRectangleCommand(AstNode dirX, AstNode dirY, AstNode distance, AstNode width, AstNode height)
        {
            DirX = dirX;
            DirY = dirY;
            Distance = distance;
            Width = width;
            Height = height;
        }
    }

    public class FillCommand : AstNode { }

    public class AssignmentStatement : AstNode
    {
        public string VariableName { get; }
        public AstNode Expression { get; }
        public AssignmentStatement(string varName, AstNode expr) { VariableName = varName; Expression = expr; }
    }

    public class LabelStatement : AstNode
    {
        public string LabelName { get; }
        public LabelStatement(string labelName) { LabelName = labelName; }
    }

    public class GoToStatement : AstNode
    {
        public string LabelName { get; }
        public AstNode Condition { get; }
        public GoToStatement(string labelName, AstNode condition) { LabelName = labelName; Condition = condition; }
    }

    // Expressions
    public abstract class Expression : AstNode { }

    public class LiteralExpression<T> : Expression
    {
        public T Value { get; }
        public LiteralExpression(T value) { Value = value; }
    }

    public class VariableExpression : Expression
    {
        public string VariableName { get; }
        public VariableExpression(string varName) { VariableName = varName; }
    }

    public class BinaryOperationExpression : Expression
    {
        public Expression Left { get; }
        public string Operator { get; }
        public Expression Right { get; }
        public BinaryOperationExpression(Expression left, string op, Expression right) { Left = left; Operator = op; Right = right; }
    }

    public class UnaryOperationExpression : Expression
    { 
        public Expression Right { get; }
        public string Operator { get; }
        public UnaryOperationExpression(Expression right, string op) { Operator = op; Right = right; }
    }

    public class FunctionCallExpression : Expression
    {
        public string FunctionName { get; }
        public List<AstNode> Arguments { get; }
        public FunctionCallExpression(string functionName, List<AstNode> args) { FunctionName = functionName; Arguments = args; }
    }
}
